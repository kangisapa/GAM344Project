using TMPro;
using UnityEngine;

public class UIController : MonoBehaviour
{
    public MasterController MasterController;

    // ---------- Player/Round Information ----------
    public TextMeshProUGUI currentHealth;
    public TextMeshProUGUI currentCurrency;
    public TextMeshProUGUI currentWave;

    // ---------- End Game Message ----------
    [SerializeField] private TextMeshProUGUI endGameText;

    void Start()
    {
        if (endGameText != null) endGameText.text = "";

        if (MasterController != null)
        {
            MasterController.OnGameStateChanged += HandleGameStateChanged;
            HandleGameStateChanged(MasterController.State);
        }

    }

    void OnDestroy()
    {
        if (MasterController != null)
            MasterController.OnGameStateChanged -= HandleGameStateChanged;
    }

    void Update()
    {
        currentHealth.text = "Health: " + MasterController.PlayerHealth;
        currentCurrency.text = "Currency: " + MasterController.PlayerCurrency;
        currentWave.text = "Wave: " + MasterController.CurrentWave + "/" + MasterController.TotalWaves;
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