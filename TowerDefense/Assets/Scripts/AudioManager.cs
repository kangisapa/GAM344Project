using System;
using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public Sound[] musicSounds, sfxSounds;
    public AudioSource musicSource, sfxSource;

    public void PlaySFX(string name)
    {
        Sound s = Array.Find(sfxSounds, x => x.name == name);

        if (s != null) 
        {

            Debug.Log("SFX Not Found");
        
        }

        else
        {
            sfxSource.PlayOneShot(s.clip);
        }



    }
    }
