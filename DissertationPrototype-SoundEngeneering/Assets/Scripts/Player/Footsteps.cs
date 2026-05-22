using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Footsteps : MonoBehaviour
{
    [SerializeField] private AudioClip[] footstepSFX;
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private Rigidbody2D rig;
    [SerializeField] private float playRate;
    private float lastPlayTime;

    void Update ()
    {
        // Only play SFX if we are moving.
        // Play every "playRate" seconds.
        if(rig.linearVelocity.magnitude > 0 && Time.time - lastPlayTime > playRate)
        {
            Play();
        }
    }

    void Play ()
    {
        lastPlayTime = Time.time;

        AudioClip clipToPlay = footstepSFX[Random.Range(0, footstepSFX.Length)];
        audioSource.PlayOneShot(clipToPlay);
    }
}