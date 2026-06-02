using System.Collections;
using System.Runtime.CompilerServices;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIController : MonoBehaviour
{
    // ---------- Player/Round Information ----------
    public TextMeshProUGUI currentHealth;
    public TextMeshProUGUI currentCurrency;
    public TextMeshProUGUI currentWave;
    public int[] playRates = { 1, 4, 6 };
    [SerializeField] private Button[] playRateButtons;

    // ---------- End Game Message ----------
    [SerializeField] private TextMeshProUGUI endGameText;
    // ---------- Audio ----------
    private AudioManager audioManager;
    void Start()
    {
        if (endGameText != null) endGameText.text = "";

        MasterController.Instance.OnGameStateChanged += HandleGameStateChanged;

        audioManager = AudioManager.Instance;


        for (int i = 0; i < playRateButtons.Length; i++)
        {
            float playRate = playRates[i];
            playRateButtons[i].onClick.AddListener(() => MasterController.Instance.SetTimeScale(playRate));
        }
    }


    void OnDestroy()
    {
        MasterController.Instance.OnGameStateChanged -= HandleGameStateChanged;
    }

    void Update()
    {
        currentHealth.text = $"Health: {MasterController.Instance.PlayerHealth}";
        currentCurrency.text = $"Currency: {MasterController.Instance.PlayerCurrency}";
        currentWave.text = $"Waves Completed: {MasterController.Instance.CurrentWave}/{MasterController.Instance.TotalWaves}";
        //make buttons look pretty and mimicks a selected button if that option is "selected"
        for(int i = 0;  i < playRateButtons.Length; i++)
        {
            playRateButtons[i].interactable = !Mathf.Approximately(playRates[i], Time.timeScale);
        }
    }

    private void HandleGameStateChanged(MasterController.GameState newState)
    {
        if (endGameText == null) return;

        switch (newState)
        {
            case MasterController.GameState.Victory:
                endGameText.text = "Win";
                GameManager.Instance.ShowContinueMenu();
                break;
            case MasterController.GameState.GameOver:
                endGameText.text = "Lose";
                GameManager.Instance.ShowContinueMenu("Retry?");
                audioManager.PlaySFX(audioManager.playerDeathShatter);
                break;
            default:
                endGameText.text = "";
                break;
        }
    }
}