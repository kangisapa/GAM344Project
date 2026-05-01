using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;

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
    [SerializeField] private int totalWaves = 5;
    [SerializeField] private int creepsPerWave = 5;
    [SerializeField] private float spawnInterval = 1f;
    [SerializeField] private float timeBetweenWaves = 5f;
    

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
    public int TotalWaves     => totalWaves;
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
          while (currentWaveIndex < totalWaves && currentState == GameState.Playing)
        {
            yield return new WaitForSeconds(timeBetweenWaves);

            //waveInProgress = true;
            for (int i = 0; i < creepsPerWave; i++)
            {
                SpawnCreep(0);
                yield return new WaitForSeconds(spawnInterval);
            }

            while (enemiesAlive > 0) yield return null;

            //waveInProgress = false;
            currentWaveIndex++;
            OnWaveChanged?.Invoke(currentWaveIndex);
        }

        if (currentState == GameState.Playing) EndGame(true);
    }
    
    // ---------- Spawning ----------

    // TODO: Chagne these to line up with other code

    public void SpawnCreep(int index)
    {
        //Call Creep spawning element
        GameObject newCreep = Creep.CreateNewCreep(_creepCache[index]);
        newCreep.transform.parent = creepParent;
        //Increase enemies alives
        enemiesAlive++;
    }

    public void SpawnBossCreep(int index)
    {
        GameObject newBoss = Creep.CreateNewBossCreep(_creepCache[index]);
        newBoss.transform.parent = creepParent;

        Creep.BossCreep bossComponent = newBoss.GetComponent<Creep.BossCreep>();
        bossComponent.SetHealth(500f);
        bossComponent.SetSpeed(1.5f);

        enemiesAlive++;
    }

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

