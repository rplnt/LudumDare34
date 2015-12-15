using UnityEngine;
using System.Collections;

public class AudioManager : MonoBehaviour {
    private static AudioManager instance;
    AudioSource source;

    public AudioClip toink;
    public AudioClip explosion;
    public AudioClip siren;

    public bool paused;
    public bool muted;

    float sirenTimer;
    bool sirenEnabled = false;


    void Start() {
        paused = true;
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

    public void PlaySiren() {
        if (!sirenEnabled) {
            sirenEnabled = true;
            sirenTimer = Mathf.Infinity;
        }

        if (sirenTimer > siren.length + 0.2f) {
            PlaySound(siren, false);
            sirenTimer = 0.0f;
        }
    }

    public void StopSiren() {
        sirenEnabled = false;
    }

    void PlaySound(AudioClip sound, bool changePitch = true) {
        source.pitch = 1.0f + (changePitch ? Random.Range(-0.1f, 0.1f) : 0.0f);
        source.PlayOneShot(sound);
    }

    public void ToggleMute() {
        muted = !muted;
        source.mute = muted;
    }

    void Update() {
        if (sirenEnabled) {
            sirenTimer += Time.deltaTime;
        }
    }

}
