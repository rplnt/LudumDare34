using UnityEngine;
using System.Collections;
using UnityStandardAssets.ImageEffects;

public class PulseBorders : MonoBehaviour {

    public bool active;
    ScreenOverlay so;

    public float diff;
    bool pulsing = false;
    bool up = true;

    void Start() {
        so = gameObject.GetComponent<ScreenOverlay>();
        if (so == null) {
            Debug.LogError("Could not find ScreenOverlay");
            active = false;
        } else {
            so.intensity = 0.0f;
        }
    }


    public void StartPulser() {
        pulsing = true;
        up = true;
    }

    public void StopPulser() {
        pulsing = false;
        up = true;
    }


    void Update() {
        if (!active) return;

        if (!pulsing) {
            if (so.intensity > 0.0f) {
                so.intensity -= diff * Time.deltaTime * 2.0f;
            }
            return;
        }


        if (up && so.intensity < 1.3f) {
            so.intensity += diff * Time.deltaTime * (so.intensity < 0.8f ? 2.0f : 1.0f);
        } else if (up) {
            up = false;
        } else if (!up && so.intensity > 0.8f) {
            so.intensity -= diff * Time.deltaTime;
        } else { //!up
            up = true;
        }

    }

}

