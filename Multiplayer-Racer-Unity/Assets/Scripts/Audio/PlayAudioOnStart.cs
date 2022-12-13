using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class PlayAudioOnStart : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        var clip = GetComponent<AudioSource>();
        clip.Play();
    }
}
