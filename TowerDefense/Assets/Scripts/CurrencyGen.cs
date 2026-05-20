using UnityEngine;

public class CurrencyGen : MonoBehaviour
{
    [Header("Payment Settings")]
    [Min(0.01f)]
    [SerializeField] private float paymentInterval = 5f;

    [Min(0)]
    [SerializeField] private int paymentAmount = 1;
    private float timer;

    public float PaymentInterval
    {
        get => paymentInterval;
        set => paymentInterval = Mathf.Max(0.01f, value);
    }

    public int PaymentAmount
    {
        get => paymentAmount;
        set => paymentAmount = Mathf.Max(0, value);
    }

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
