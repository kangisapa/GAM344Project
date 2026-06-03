using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UIElements;

[RequireComponent(typeof(Collider2D))]
public class PanicButtonController : MonoBehaviour
{
    [SerializeField] private List<GameObject> chargeIcons = new List<GameObject>();
    [SerializeField] private PolygonCollider2D agitationZone;
 
    [Header("Wave Settings")]
    [Tooltip("Vertical travel of the wave, in local units.")]
    [SerializeField] private float waveAmplitude = 0.05f;
    [Tooltip("Wave speed (radians/sec) at rest.")]
    [SerializeField] private float slowWaveSpeed = 1.5f;
    [Tooltip("Wave speed (radians/sec) when agitated.")]
    [SerializeField] private float fastWaveSpeed = 6f;
    [Tooltip("Phase offset between adjacent icons, in radians. Creates the travelling-wave look.")]
    [SerializeField] private float wavePhaseOffset = 0.6f;
 
    private AudioManager audioManager;
    private ContactFilter2D contactFilter;
    // Wave state
    private Vector3[] baseLocalPositions;
    private float wavePhase;
    private float currentWaveSpeed;
 
    private void Awake()
    {
        currentWaveSpeed = slowWaveSpeed;
        baseLocalPositions = new Vector3[chargeIcons.Count];
        for (int i = 0; i < chargeIcons.Count; i++)
        {
            if (chargeIcons[i] != null)
                baseLocalPositions[i] = chargeIcons[i].transform.localPosition;
        }
    }
 
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
        contactFilter = new()
        {
            layerMask = LayerMask.GetMask("Creeps")
        };
    }

    private bool hovered = false;
    Collider2D[] overlapDiscard = new Collider2D[1];

    private void Update()
    {
        wavePhase += Time.deltaTime * currentWaveSpeed;
        int overlaps = agitationZone.Overlap(contactFilter, overlapDiscard);
        SetAgitated(overlaps > 0);

        bool greyed = false;
        for (int i = 0; i < chargeIcons.Count; i++)
        {
            GameObject icon = chargeIcons[i];
            if (icon == null || !icon.activeSelf) continue;
            Vector3 basePos = baseLocalPositions[i];
            float y = basePos.y + Mathf.Sin(wavePhase + i * wavePhaseOffset) * waveAmplitude;
            icon.transform.localPosition = new Vector3(basePos.x, y, basePos.z);
        }
        for(int i = chargeIcons.Count - 1; i >= 0; i--)
        {
            GameObject icon = chargeIcons[i];
            if (icon == null || !icon.activeSelf) continue;
            SpriteRenderer sp = icon.GetComponent<SpriteRenderer>();
            if (!greyed && hovered)
            {
                greyed = true;
                sp.color = Color.Lerp(sp.color, Color.grey, Time.unscaledDeltaTime * 10);
            }
            else
            {
                sp.color = Color.Lerp(sp.color, Color.white, Time.unscaledDeltaTime * 10);
            }
        }
    }
 
    public void SetAgitated(bool agitated)
    {
        currentWaveSpeed = agitated ? fastWaveSpeed : slowWaveSpeed;
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

    public void OnMouseEnter()
    {
        hovered = true;
    }

    public void OnMouseExit()
    {
        hovered = false;
    }
}