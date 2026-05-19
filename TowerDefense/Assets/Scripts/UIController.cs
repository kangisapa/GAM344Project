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
    [SerializeField] private Image panicButtonFill;
    private Button panicButton;
    // ---------- End Game Message ----------
    [SerializeField] private TextMeshProUGUI endGameText;
    // ---------- Audio ----------
    private AudioManager audioManager;
    void Start()
    {
        if (endGameText != null) endGameText.text = "";

        MasterController.Instance.OnGameStateChanged += HandleGameStateChanged;

        audioManager = AudioManager.Instance;

        panicButton = panicButtonFill.GetComponent<Button>();



        panicButton.onClick.AddListener(() =>
        { audioManager.PlaySFX(audioManager.panicButton);
          StartCoroutine(PlayTsunamiSound());
        MasterController.Instance.OnRewindInitiated?.Invoke(); 
        });


        for(int i = 0; i < playRateButtons.Length; i++)
        {
            float playRate = playRates[i];
            playRateButtons[i].onClick.AddListener(() => MasterController.Instance.SetTimeScale(playRate));
        }
    }
    private IEnumerator PlayTsunamiSound()
    {
        yield return new WaitForSeconds(.5f);

        audioManager.PlaySFX(audioManager.tsunamiPanicButton);
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
        panicButtonFill.fillAmount = Mathf.Lerp(panicButtonFill.fillAmount, MasterController.Instance.UsesRemainingPercent, Time.deltaTime * 5);
        panicButton.interactable = MasterController.Instance.buttonAvailable;
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