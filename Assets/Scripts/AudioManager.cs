using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioManager : MonoBehaviour {

    AudioSource source;

    public void PlayAudio(AudioClip clip) {
        source.clip = clip;
        source.time = 0;
        source.Play();
    }

    // Update is called once per frame
    void Update() {

    }
}
