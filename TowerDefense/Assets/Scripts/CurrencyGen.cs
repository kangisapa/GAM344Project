using UnityEngine;

public class CurrencyGen : MonoBehaviour
{
    [Header("Payment Settings")]
    [Min(0.01f)]
    [SerializeField] private float paymentInterval = 5f;

    [Min(0)]
    [SerializeField] private int paymentAmount = 1;


    private float timer;

    private void Start()
    {
        timer = paymentInterval;
    }

    private void Update()
    {
        if (MasterController.Instance == null) return;

        timer += Time.deltaTime;

        while (timer >= paymentInterval)
        {
            MasterController.Instance.GiveCurrency(paymentAmount);
            timer -= paymentInterval;
        }
    }
}
