using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.Events;
using UnityEngine.SceneManagement;


[Serializable]
public struct CreepSpawnSet
{
    public int creepIndexToSpawn;
    public int numberToSpawn;
    public int pathIndex;
    public float deleyPerCreep;
}

[Serializable]
public class Wave
{
    [HideInInspector] public string name = "wave";
    [InspectorName("Creep Set")]
    public List<CreepSpawnSet> creepSpawnOrder = new List<CreepSpawnSet>();
    public float delayBeforeWave;
}

public enum PanicButtonBehavior
{
    Standard,
    Slowing,
    Damaging,
}

public class MasterController : MonoBehaviour
{
    // ---------- Singleton ----------
    public static MasterController Instance { get; private set; }

    public string levelOnWin;
    public enum GameState { Start, Playing, GameOver, Victory }

    private Transform pathStartLocationTransform;

    // ---------- Player Information ----------
    [Header("Player Information")]
    [SerializeField] private int startingCurrency = 10;
    [SerializeField] private int startingHealth = 5;
    private int playerCurrency;
    private int playerHealth;

    // ---------- Round Information ----------
    [Header("Round Information")]
    [SerializeField] private List<Wave> waves = new();

    [InspectorName("Path"), Tooltip("Path defined by just listing out the splines wanted for it. (EX: 0,1,2,3,...) so 0,1,2,5 is how one could look ")]
    public List<string> paths;
    //contains the "decompiled" strings that the game actually uses. this is an array with the elements being List<int>.
    //So wavePaths[0] would return the first list defining the path.
    private List<int>[] wavePaths;

    private int currentWaveIndex = 0;
    private int enemiesAlive = 0;
    //private bool waveInProgress = false;  Not Needed Yet

    // ---------- Panic Button Information ----------
    [Header("Panic Button Basic Setup")]
    public int maxButtonUses = 3;
    private int usesRemaining;
    public int UsesRemaining => usesRemaining;
    public int MaxButtonUses => maxButtonUses;
    public event Action<int> OnUsesRemainingChanged;

    [Tooltip("How many seconds of creep progress the panic button reverts"), Min(0)]
    public float rewindTime = 5;
    [Tooltip("How long the rewind will actually last (moving back \"rewindTime\" amount of progress in x seconds")]
    public float rewindActionTime = 3;

    [Header("Panic Button Slow Setup")]
    [Tooltip("Percent of Max Speed the creep will go after button press if that dialog path was chosen"), Range(0, 1)]
    public float slowPercent = 0.75f;
    [Min(0)]
    public float slowTime = 5;

    [Header("Panic Button Damage Setup")]
    [Tooltip("The ammount of damage the panic button would deal if pressed and the dialog path was chosen")]
    public float damageDealt = 1;

    public bool buttonAvailable { get; private set; } = true;

    [HideInInspector]
    public UnityAction OnRewindInitiated;


    // ---------- Game Information ----------
    [Header("Game Information")]
    private GameState currentState = GameState.Start;

    [Header("Spawn Organizers")]
    [SerializeField] private Transform creepParent;
    [SerializeField] private Transform towerParent;
    private float startingProgress;
    private Vector3 startingWorldPosition;

    // ---------- Prefabs ----------
    [Header("Prefab Setup")]

    [SerializeField] private List<GameObject> towerPrefabs = new();
    public float NumberOfAvailableTowers => towerPrefabs.Count;

    public string GetTowerName(int index) => towerPrefabs[index].name;
    public Sprite GetTowerSprite(int index) => towerPrefabs[index].GetComponent<Tower>().GetThumbnailSprite();

    [SerializeField] private List<GameObject> creepPrefabs = new();

    // ---------- UI Events ----------
    public event Action<int> OnCurrencyChanged;
    public event Action<int> OnHealthChanged;
    public event Action<int> OnWaveChanged;
    
    public event Action<GameState> OnGameStateChanged;

    // ---------- Public getters ----------
    public int PlayerCurrency => playerCurrency;
    public int PlayerHealth   => playerHealth;
    public int CurrentWave    => currentWaveIndex;
    public int TotalWaves     => waves.Count;
    public GameState State    => currentState;

    // Audio //
    private AudioManager audioManager;

    // ================================================================

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;

        playerCurrency = startingCurrency;
        playerHealth   = startingHealth;
        GeneratePathLists();
    }

    private void OnValidate()
    {
        for(int i = 0; i < waves.Count; i++)
        {
            //name it so it looks nice in editor
            waves[i].name = $"Wave {i + 1}";
        }

        //validation checking during editor for realtime feedback if something is inputted incorrectly
        for (int i = 0; i < paths.Count; i++)
        {
            string trimmed = paths[i].Trim();
            string[] parts = paths[i].Split(',');
            foreach (string part in parts)
            {
                //discarding int since I dont creally care here what the parse is, just if its valid or not
                if (!int.TryParse(part, out _) && !string.IsNullOrEmpty(part))
                {
                    Debug.LogWarning($"Warning: \"{part}\" is not a valid input for a spline index on \"path {i}\". " +
                        $"Make sure your format looks something like: 0,1,3,...");
                }
            }
        }
    }

    private void Start()
    {
        audioManager = AudioManager.Instance;
        pathStartLocationTransform = transform.Find("CreepSpawnLocationApprox");
        pathStartLocationTransform.GetComponent<SpriteRenderer>().enabled = false;
        if (pathStartLocationTransform != null)
        {
            startingProgress = PathController.Instance.GetNearestPointToStart(pathStartLocationTransform.transform.position, 0, out startingWorldPosition);
            UpdateUI();
            StartGame();
        }
        else
        {
            Debug.LogWarning("Unable to find object named \"CreepSpawnLocationApprox\" as a child in the master controller, this object should also have a sprite renderer. Game loop aborted");
        }
    }

    private void GeneratePathLists()
    {
        wavePaths = new List<int>[paths.Count];

        for (int i = 0; i < paths.Count; i++)
        {
            wavePaths[i] = new List<int>();

            string trimmed = paths[i].Trim();
            string[] parts = paths[i].Split(',');
            foreach (string part in parts)
            {
                int output;
                if (int.TryParse(part, out output))
                {
                    wavePaths[i].Add(output);
                }
                else
                {
                    Debug.LogWarning($"Warning: {part} is not a valid input for a spline index on \"path {i}\". " +
                        $"Make sure your format looks something like: 0,1,3,...");
                }
            }
        }
    }


    // ---------- Core flow ----------

    public void StartGame()
    {
        if (currentState == GameState.Playing) return;

        currentWaveIndex = 0;
        enemiesAlive = 0;
        usesRemaining = maxButtonUses;
        buttonAvailable = usesRemaining > 0;
        OnUsesRemainingChanged?.Invoke(usesRemaining);
        OnRewindInitiated += () => StartCoroutine(PanicButtonCooldown());
        SetGameState(GameState.Playing);
        StartCoroutine(GameLoop());
    }

    public void EndGame(bool victory)
    {
        StopAllCoroutines();
        SetGameState(victory ? GameState.Victory : GameState.GameOver);
    }

    public bool pauseSpawning = false;

    private IEnumerator GameLoop()
    {
        while (currentWaveIndex < waves.Count && currentState == GameState.Playing)
        {
            audioManager.PlaySFX(audioManager.startWaveSFX);
            //Grab the current wave
            Wave w = waves[currentWaveIndex];
            yield return new WaitForSeconds(w.delayBeforeWave);

            //Create the delay between spawning creeps

            //Go through each creep sets we want to spawn
            for (int c = 0; c < w.creepSpawnOrder.Count; c++)
            {
                //count up the for the number to spawn in the set of creeps to spawn
                for (int n = 0; n < w.creepSpawnOrder[c].numberToSpawn; n++)
                {
                    if (pauseSpawning)
                    {
                        n--;
                        yield return null;
                    }
                    else
                    {
                        //spawn said creep
                        SpawnCreep(w.creepSpawnOrder[c].creepIndexToSpawn, wavePaths[w.creepSpawnOrder[c].pathIndex], startingProgress);
                        yield return new WaitForSeconds(w.creepSpawnOrder[c].deleyPerCreep);
                    }
                }
            }

            //once all the creeps are dead, continue
            while (enemiesAlive > 0) yield return null;

            //go to the next wave
            currentWaveIndex++;
            audioManager.PlaySFX(audioManager.endWaveSFX); // Currently getting muffled due to no delay in the time from ending the code, maybe add time delay

            OnWaveChanged?.Invoke(currentWaveIndex);
        }

        if (currentState == GameState.Playing) EndGame(true);
    }

    // ---------- Spawning ----------

    /// <summary>
    /// Spawn a new creep in
    /// </summary>
    /// <param name="index">index from the list of available creeps</param>
    /// <param name="pathIndexes">A List of integers containing all the spline indexes we want the creep to follow in order from the spline container on the path controller</param>
    /// <param name="startprogress">Spawns the creep  "startprogress percent (0-1) on the first spline based on where the spwan object is on the level"</param>

    public void SpawnCreep(int index, List<int> pathIndexes, float startprogress)
    {
        //Call Creep spawning element
        GameObject newCreep = Instantiate(creepPrefabs[index]);
        newCreep.transform.position = pathStartLocationTransform.position;
        newCreep.GetComponent<Creep>().SetValves(pathIndexes, startprogress);
        newCreep.transform.parent = creepParent;
        //Increase enemies alives
        enemiesAlive++;
    }

    public Transform CreepParent => creepParent;
    public void IncrementEnemies() => enemiesAlive++;

    /// <summary>
    /// Spawn a tower
    /// </summary>
    /// <param name="index">towwer index from the list of available towers</param>
    /// <param name="position">World position to spawn at</param>
    public void SpawnTower(int index, Vector3 position)
    {
        // Call Tower Spawning Element
        GameObject newTower = Instantiate(towerPrefabs[index]);
        newTower.transform.parent = towerParent;
        newTower.GetComponent<Tower>().PlaceTower(position);

        // Decrease Money
        playerCurrency -= newTower.GetComponent<Tower>().Cost;
    }

    public bool CheckCurrency(int index)
    {
        return (playerCurrency - towerPrefabs[index].GetComponent<Tower>().Cost >= 0); 
    }

    // ---------- Currency / damage hooks ----------

    public void GiveCurrency(int amount)
    {
        playerCurrency += amount;
        OnCurrencyChanged?.Invoke(playerCurrency);
    }

    public void TakeDamage(int damage)
    {
        playerHealth = Mathf.Max(0, playerHealth - damage);
        OnHealthChanged?.Invoke(playerHealth);
      
        AudioClip[] sounds = //replaced original with array for sophistication
       {
            audioManager.playerDamageSFX,
            audioManager.playerDamageSFX2,
            audioManager.playerDamageSFX3,

        };
        if (playerHealth > 0)
        {
            audioManager.PlaySFX(sounds[UnityEngine.Random.Range(0, sounds.Length)]);
        }

        if (playerHealth == 0) EndGame(false);
    }

    public void OnCreepKilled(int reward)
    {
        enemiesAlive = Mathf.Max(0, enemiesAlive - 1);
        GiveCurrency(reward);
    }

    public void OnCreepReachedEnd(int damage)
    {
        enemiesAlive = Mathf.Max(0, enemiesAlive - 1);
        TakeDamage(damage);
    }

    // ---------- UI ----------

    private void UpdateUI()
    {
        OnCurrencyChanged?.Invoke(playerCurrency);
        OnHealthChanged?.Invoke(playerHealth);
        OnWaveChanged?.Invoke(currentWaveIndex);
        OnGameStateChanged?.Invoke(currentState);
    }

    private void SetGameState(GameState newState)
    {
        if (currentState == newState) return;
        currentState = newState;
        OnGameStateChanged?.Invoke(newState);
        if(newState == GameState.Victory)
        {
            GameManager.Instance.GoToNewSceneAfterDelay(10, levelOnWin);
        }
        else if(newState == GameState.GameOver)
        {
            GameManager.Instance.GoToNewSceneAfterDelay(5, SceneManager.GetActiveScene().buildIndex);
        }
    }
    public bool TryUsePanicButton()
    {
        if (!buttonAvailable) return false;
        if (usesRemaining <= 0) return false;
        OnRewindInitiated?.Invoke();
        return true;
    }

    private IEnumerator PanicButtonCooldown()
    {
        buttonAvailable = false;
        usesRemaining--;
        OnUsesRemainingChanged?.Invoke(usesRemaining);
        pauseSpawning = true;
        yield return new WaitForSeconds(rewindTime);
        pauseSpawning = false;
        //cooldown delay, currently an extra 50% of rewind, (3 second rewind -> 4.5 second cooldown consiting of 3 second delay of spawning for pushing back + additonal 1.5 seconds
        yield return new WaitForSeconds(rewindTime * .5f);
        buttonAvailable = usesRemaining > 0;
    }

    /// <summary>
    /// Change how fast the game is bring run (2 => 2x speed, 0.25 => runs 4 times slower)
    /// </summary>
    /// <param name="timeScale">new time scale, minimum of 0.25</param>
    public void SetTimeScale(float timeScale)
    {
        timeScale = Mathf.Max(timeScale, 0);
        Time.timeScale = timeScale;
        audioManager.SetAudioSourcePitches(timeScale);
    }
}

