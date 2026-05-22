using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioManager : MonoBehaviour
{
    [SerializeField] private AudioSource playerAudioSource;

    public static AudioManager Instance;

    void Awake ()
    {
        if(Instance != null && Instance != this)
            Destroy(gameObject);
        else
            Instance = this;
    }

    public void PlayPlayerSound (AudioClip clip)
    {
        PlaySound(playerAudioSource, clip);
    }

    public void PlaySound (AudioSource source, AudioClip clip)
    {
        source.PlayOneShot(clip);
    }
}