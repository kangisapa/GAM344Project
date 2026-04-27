using TMPro;
using UnityEngine;

public class UIController : MonoBehaviour
{

    public MasterController MasterController;

    // ---------- Player/Round Information ----------
    public TextMeshProUGUI currentHealth;
    public TextMeshProUGUI currentCurrency;
    public TextMeshProUGUI currentWave;


    void Start()
    {

    }

    void Update()
    {
        currentHealth.text = "Health: " + MasterController.PlayerHealth;
        currentCurrency.text = "Currency: " + MasterController.PlayerCurrency;
        currentWave.text = "Wave: " + MasterController.CurrentWave + "/" + MasterController.TotalWaves;
    }
}
