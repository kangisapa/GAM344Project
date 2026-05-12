using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIController : MonoBehaviour
{
    // ---------- Player/Round Information ----------
    public TextMeshProUGUI currentHealth;
    public TextMeshProUGUI currentCurrency;
    public TextMeshProUGUI currentWave;
    [SerializeField] private Button[] playRateButtons;

    // ---------- End Game Message ----------
    [SerializeField] private TextMeshProUGUI endGameText;

    void Start()
    {
        if (endGameText != null) endGameText.text = "";

        MasterController.Instance.OnGameStateChanged += HandleGameStateChanged;

        for(int i = 1; i <= playRateButtons.Length; i++)
        {
            float playRate = i;
            playRateButtons[i - 1].onClick.AddListener(() => MasterController.Instance.SetTimeScale(playRate));
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
        for(int i = 1;  i <= playRateButtons.Length; i++)
        {
            playRateButtons[i - 1].interactable = !Mathf.Approximately(i, Time.timeScale);
        }
    }

    private void HandleGameStateChanged(MasterController.GameState newState)
    {
        if (endGameText == null) return;

        switch (newState)
        {
            case MasterController.GameState.Victory:
                endGameText.text = "Win";
                break;
            case MasterController.GameState.GameOver:
                endGameText.text = "Lose";
                break;
            default:
                endGameText.text = "";
                break;
        }
    }
}