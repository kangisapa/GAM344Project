using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;


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
    public List<CreepSpawnSet> creepSpawnOrder = new List<CreepSpawnSet>();
    public float delayBeforeWave;
}


public class MasterController : MonoBehaviour
{
    // ---------- Singleton ----------
    public static MasterController Instance { get; private set; }

    public enum GameState { Start, Playing, GameOver, Victory }

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

    // ---------- Game Information ----------
    [Header("Game Information")]
    private GameState currentState = GameState.Start;

    [SerializeField] private Transform creepParent, towerParent;


    // ---------- Tower Data Caching ----------

    [SerializeField] private List<string> towerKeys = new List<string> { "BasicTower" };
    private List<TowerData> _towerCache = new List<TowerData>();

    // ---------- Creep Data ----------

    // ---------- Tower Data Caching ----------

    [SerializeField] private List<string> creepKeys = new List<string> { "BasicCreep" };
    private List<CreepData> _creepCache = new List<CreepData>();


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
        CacheInformation();
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

        UpdateUI();
        StartGame();
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
        SetGameState(GameState.Playing);
        StartCoroutine(GameLoop());
    }

    public void EndGame(bool victory)
    {
        StopAllCoroutines();
        SetGameState(victory ? GameState.Victory : GameState.GameOver);
    }

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
                    //spawn said creep
                    SpawnCreep(w.creepSpawnOrder[c].creepIndexToSpawn, wavePaths[w.creepSpawnOrder[c].pathIndex]);
                    yield return new WaitForSeconds(w.creepSpawnOrder[c].deleyPerCreep);
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
    public void SpawnCreep(int index, List<int> pathIndexes)
    {
        //Call Creep spawning element
        GameObject newCreep = Creep.CreateNewCreep(_creepCache[index], pathIndexes);
        newCreep.transform.parent = creepParent;
        //Increase enemies alives
        enemiesAlive++;
    }

    public void SpawnBossCreep(int index, List<int> pathIndexes)
    {
        GameObject newBoss = Creep.CreateNewBossCreep(_creepCache[index], pathIndexes);
        newBoss.transform.parent = creepParent;

        Creep.BossCreep bossComponent = newBoss.GetComponent<Creep.BossCreep>();
        bossComponent.SetHealth(500f);
        bossComponent.SetSpeed(1.5f);

        enemiesAlive++;
    }

    /// <summary>
    /// Cache all the data assets into memory for use
    /// </summary>
    async void CacheInformation()
    {
        foreach(string key in towerKeys)
        {
            TowerData data = await Addressables.LoadAssetAsync<TowerData>(key).Task;
            _towerCache.Add(data);
        }

        foreach(string key in creepKeys)
        {
            CreepData data = await Addressables.LoadAssetAsync<CreepData>(key).Task;
            _creepCache.Add(data);
        }

    }

    /// <summary>
    /// Spawn a tower
    /// </summary>
    /// <param name="index">towwer index from the list of available towers</param>
    /// <param name="position">World position to spawn at</param>
    public void SpawnTower(int index, Vector3 position)
    {
        // Call Tower Spawning Element
        GameObject newTower = Tower.CreateNewTower(_towerCache[index]);
        newTower.transform.parent = towerParent;
        newTower.GetComponent<Tower>().PlaceTower(position);

        // Decrease Money
        playerCurrency -= _towerCache[index].cost;
    }

    public bool CheckCurrency(int index)
    {
        return (playerCurrency - _towerCache[index].cost >= 0); 
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
        audioManager.PlaySFX(audioManager.playerDamgeSFX);
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
    }

    private void OnDestroy()
    {
        foreach(TowerData data in _towerCache)
        {
            Addressables.Release(data);
        }
    }
}

