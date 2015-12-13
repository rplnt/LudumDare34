using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class BackgroundMusic : MonoBehaviour {
    AudioSource player;
    public List<AudioClip> playlist;
    AudioClip current;

    float timer;

    void Awake() {
        player = gameObject.GetComponent<AudioSource>();
    }
        

    void Start() {
        ChangeTrack();
    }

    void Update() {
        timer += Time.deltaTime;

        if (timer > current.length) {
            ChangeTrack();
        }
    }

    void ChangeTrack() {
        player.Stop();
        current = playlist[Random.Range(0, playlist.Count - 1)];
        player.clip = current;
        timer = 0;
        player.Play();
    }



}