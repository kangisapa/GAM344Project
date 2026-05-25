using UnityEngine;

public class AudioManager : MonoBehaviour
{
    [Header("Sub-Audio Players")]
    [SerializeField] private AudioSource musicSource; 
    [SerializeField] private AudioSource sfxSource;  


    [Header("SFX Library")]
    [SerializeField] public AudioClip startWaveSFX;
    [SerializeField] public AudioClip endWaveSFX;
    [SerializeField] public AudioClip playerDamageSFX;
    [SerializeField] public AudioClip playerDamageSFX2;
    [SerializeField] public AudioClip playerDamageSFX3;
    [SerializeField] public AudioClip basicAttackSFX;

    [SerializeField] public AudioClip basicCreepHitSFX;
    [SerializeField] public AudioClip basicCreepDeathSFX;
    [SerializeField] public AudioClip basicCreepDeathSFX2;
    [SerializeField] public AudioClip basicCreepDeathSFX3;
    [SerializeField] public AudioClip basicCreepDeathSFX4;
    [SerializeField] public AudioClip basicCreepDeathSFX5;
    [SerializeField] public AudioClip creepMovement;

    [SerializeField] public AudioClip panicButton;
    [SerializeField] public AudioClip shockEffect;
    
    [SerializeField] public AudioClip playerDeathShatter;
    [SerializeField] public AudioClip tsunamiPanicButton;

    public static AudioManager Instance { get; private set; }

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }


    public void PlaySFX(AudioClip clip)
    {
        if (clip == null) return;

        sfxSource.PlayOneShot(clip);
    }

    public void SetAudioSourcePitches(float pitch)
    {
        musicSource.pitch = pitch;
        sfxSource.pitch = pitch;
    }
}