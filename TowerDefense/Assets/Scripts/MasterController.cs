using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.Splines;


[Serializable]
public struct Wave
{
    [Tooltip("Which spline it is in the path starting from 0")] public List<int> wavePath;
    public List<int> creepIndex;
    public List<int> numberToSpawn;
    public float delayPerCreep;
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
    [SerializeField] private List<Wave> waves;
    //[SerializeField] private int totalWaves = 5;
    //[SerializeField] private int creepsPerWave = 5;
    //[SerializeField] private float spawnInterval = 1f;
    //[SerializeField] private float timeBetweenWaves = 5f;
    

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

    // ================================================================

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;

        playerCurrency = startingCurrency;
        playerHealth   = startingHealth;
        CacheInformation();
    }

    private void Start()
    {
        UpdateUI();
        StartGame();
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
            //Grab the current wave
            Wave w = waves[currentWaveIndex];
            yield return new WaitForSeconds(w.delayBeforeWave);

            //Create the delay between spawning creeps
            WaitForSeconds spawnDelay = new WaitForSeconds(w.delayPerCreep);

            //Go through each creep want to spawn
            for (int c = 0; c < w.creepIndex.Count; c++)
            {
                //count up the for the number to spawn
                for (int n = 0; n < w.numberToSpawn[c]; n++)
                {
                    //spawn said creep
                    SpawnCreep(w.creepIndex[c], w.wavePath);
                    yield return spawnDelay;
                }
            }

            //once all the creeps are dead, continue
            while (enemiesAlive > 0) yield return null;

            //go to the next wave
            currentWaveIndex++;
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

