using UnityEngine;
using System.Collections;

public class AudioManager : MonoBehaviour {
    private static AudioManager instance;
    AudioSource source;

    public AudioClip toink;
    public AudioClip explosion;


    void Start() {
        source = GetComponent<AudioSource>();
    }

    void Awake() {
        if (instance != null && instance != this) {
            Destroy(this.gameObject);
            return;
        } else {
            instance = this;
        }

        DontDestroyOnLoad(this.gameObject);
    }

    public static AudioManager GetInstance() {
        return instance;
    }


    public void PlayToink() {
        PlaySound(toink);
    }

    public void PlayExplosion() {
        PlaySound(explosion);
    }

    void PlaySound(AudioClip sound) {
        source.pitch = 1.0f + Random.Range(-0.1f, 0.1f);
        source.PlayOneShot(sound);
    }

}