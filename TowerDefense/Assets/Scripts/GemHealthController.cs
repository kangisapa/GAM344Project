using System.Collections;
using UnityEngine;
 
[RequireComponent(typeof(SpriteRenderer))]
public class GemHealthController : MonoBehaviour
{
 
    [Header("Sprites")]
    [SerializeField] private Sprite fullSprite;
    [SerializeField] private Sprite crackedSprite;
    [SerializeField] private Sprite badlyCrackedSprite;
    [SerializeField] private Sprite shatteredSprite;
 
    [Header("Flash Settings")]
    [SerializeField] private Sprite whiteSprite;
    [Min(1)]
    [SerializeField] private int flashCount = 3;
    [Tooltip("Total duration of the flash effect, in seconds.")]
    [Min(0f)]
    [SerializeField] private float flashDuration = 0.35f;
 
    // Cached references
    private SpriteRenderer spriteRenderer;
 
    // Tier tracking
    private enum HealthTier { Full, Cracked, BadlyCracked, Shattered }
    private HealthTier currentTier;
    private int maxHealth;
     private int currentHealth;
 
    private Coroutine flashRoutine;
 
    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
    }
 
    private void Start()
    {
        if (MasterController.Instance != null)
        {
            maxHealth = MasterController.Instance.PlayerHealth;
            currentHealth = MasterController.Instance.PlayerHealth;
            SetSpriteForHealth(MasterController.Instance.PlayerHealth, flash: false);
            MasterController.Instance.OnHealthChanged += HandleHealthChanged;
        }
        else
        {
            Debug.LogWarning("[GemHealthController] No MasterController.Instance found in scene.");
        }
    }
 
    private void OnDestroy()
    {
        if (MasterController.Instance != null)
        {
            MasterController.Instance.OnHealthChanged -= HandleHealthChanged;
        }
    }
 
    private void HandleHealthChanged(int newHealth)
    {
        bool tookDamage = newHealth < currentHealth;
        currentHealth = newHealth;
        SetSpriteForHealth(newHealth, flash: tookDamage);
    }
 
 
    private void SetSpriteForHealth(int health, bool flash)
    {
 
        float percent = Mathf.Clamp01((float)health / Mathf.Max(1, maxHealth));
        HealthTier newTier = TierFor(percent, health);

        Sprite target = SpriteFor(newTier);
        if (!flash)
        {
            currentTier = newTier;
            if (target != null) spriteRenderer.sprite = target;
            return;
        }
 
        currentTier = newTier;
        if (target != null) spriteRenderer.sprite = target;
 
        if (flashRoutine != null)
        {
            StopCoroutine(flashRoutine);
            // Reset to a clean state in case the previous flash was interrupted
            // mid-cycle (e.g. left showing the white sprite).
            if (target != null) spriteRenderer.sprite = target;
        }
        flashRoutine = StartCoroutine(FlashRoutine());
    }
 
    private HealthTier TierFor(float percent, int rawHealth)
    {
        if (percent >= 0.999f) return HealthTier.Full;
        if (percent >= 0.800) return HealthTier.Cracked;
        if (percent >= 0.501f) return HealthTier.BadlyCracked;
        return HealthTier.Shattered;
    }
 
    private Sprite SpriteFor(HealthTier tier)
    {
        switch (tier)
        {
            case HealthTier.Full:         return fullSprite;
            case HealthTier.Cracked:      return crackedSprite;
            case HealthTier.BadlyCracked: return badlyCrackedSprite;
            case HealthTier.Shattered:    return shatteredSprite;
            default:                      return fullSprite;
        }
    }
 
    private IEnumerator FlashRoutine()
    {
        // Split the total duration evenly across white/tier halves.
        float halfStep = (flashDuration / flashCount) * 0.5f;
 
        // The actual tier sprite that should be shown between flashes — captured
        // here so an interrupted flash can't lose track of it.
        Sprite tierSprite = spriteRenderer.sprite;
 
        for (int i = 0; i < flashCount; i++)
        {
            // "On" half: swap to the whited-out sprite.
            if (whiteSprite != null) spriteRenderer.sprite = whiteSprite;
            yield return new WaitForSeconds(halfStep);
 
            // "Off" half: restore the real tier sprite.
            if (tierSprite != null) spriteRenderer.sprite = tierSprite;
            yield return new WaitForSeconds(halfStep);
        }
 
        // Always end on the tier sprite.
        if (tierSprite != null) spriteRenderer.sprite = tierSprite;
        flashRoutine = null;
    }
}