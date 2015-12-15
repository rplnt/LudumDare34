using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class BackgroundMusic : MonoBehaviour {
    AudioSource player;
    public List<AudioClip> playlist;
    AudioClip current;
    float currentLength;
    AudioManager am;

    float timer;

    void Awake() {
        player = gameObject.GetComponent<AudioSource>();
    }
        

    void Start() {
        am = AudioManager.GetInstance();
        timer = 0.0f;
        currentLength = 0.0f;
    }


    void Update() {
        if (am.muted != player.mute) {
            player.mute = am.muted;
        }

        timer += Time.deltaTime;

        if (timer > currentLength) {
            ChangeTrack();
        }
    }


    void ChangeTrack() {
        player.Stop();
        if (am.paused) {
            current = playlist[0];
        } else {
            current = playlist[Random.Range(1, playlist.Count - 1)];
        }
        currentLength = current.length;
        player.clip = current;
        timer = 0;
        player.Play();
    }



}
