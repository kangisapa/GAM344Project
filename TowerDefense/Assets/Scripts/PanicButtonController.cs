using System.Collections;
using System.Collections.Generic;
using UnityEngine;
 

[RequireComponent(typeof(Collider2D))]
public class PanicButtonController : MonoBehaviour
{
    [SerializeField] private List<GameObject> chargeIcons = new List<GameObject>();
 
    private AudioManager audioManager;
 
    private void OnEnable()
{
    if (MasterController.Instance != null)
    {
        MasterController.Instance.OnUsesRemainingChanged += HandleUsesChanged;
        HandleUsesChanged(MasterController.Instance.UsesRemaining);
    }
}
 
    private void OnDisable()
    {
        if (MasterController.Instance != null)
            MasterController.Instance.OnUsesRemainingChanged -= HandleUsesChanged;
    }
 
    private void Start()
{
    audioManager = AudioManager.Instance;

    if (MasterController.Instance != null)
    {
        MasterController.Instance.OnUsesRemainingChanged -= HandleUsesChanged;
        MasterController.Instance.OnUsesRemainingChanged += HandleUsesChanged;
        HandleUsesChanged(MasterController.Instance.UsesRemaining);
    }
}

 

    private void OnMouseDown()
    {
        if (MasterController.Instance == null) return;
 
        if (!MasterController.Instance.TryUsePanicButton()) return;
 
        if (audioManager != null)
        {
            audioManager.PlaySFX(audioManager.panicButton);
            StartCoroutine(PlayTsunamiSound());
        }
    }
 
    private void HandleUsesChanged(int usesRemaining)
    {
        for (int i = 0; i < chargeIcons.Count; i++)
        {
            if (chargeIcons[i] != null)
                chargeIcons[i].SetActive(i < usesRemaining);
        }
    }
 
    private IEnumerator PlayTsunamiSound()
    {
        yield return new WaitForSeconds(.5f);
        audioManager.PlaySFX(audioManager.tsunamiPanicButton);
    }
}