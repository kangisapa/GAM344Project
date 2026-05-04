using UnityEngine;

public class AudioManager : MonoBehaviour
{
    [Header("Sub-Audio Players")]
    [SerializeField] private AudioSource musicSource; 
    [SerializeField] private AudioSource sfxSource;  


    [Header("SFX Library")]
    [SerializeField] public AudioClip startWaveSFX;
    [SerializeField] public AudioClip endWaveSFX;
    [SerializeField] public AudioClip playerDamgeSFX;

    [SerializeField] public AudioClip basicAttackSFX;

    [SerializeField] public AudioClip basicCreepDeathSFX;
    [SerializeField] public AudioClip basicCreepHitSFX;

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
}