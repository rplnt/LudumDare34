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


    public void PlayToink(Vector2 pos) {
        PlaySound(toink, pos.y / 50.0f);
    }

    public void PlayExplosion() {
        source.volume = 0.8f;
        PlaySound(explosion, Random.Range(-0.1f, 0.1f));
    }

    public void PlaySiren() {
        if (!sirenEnabled) {
            sirenEnabled = true;
            sirenTimer = Mathf.Infinity;
            source.volume = 0.3f;
        }

        if (sirenTimer > siren.length + 0.2f) {
            PlaySound(siren, 0.0f);
            sirenTimer = 0.0f;
        }
    }

    public void StopSiren() {
        source.volume = 1.0f;
        sirenEnabled = false;
    }

    void PlaySound(AudioClip sound, float pitchOffset) {
        source.pitch = 1.0f + pitchOffset; //(changePitch ? Random.Range(-0.1f, 0.1f) : 0.0f);
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
