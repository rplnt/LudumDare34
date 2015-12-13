using UnityEngine;
using System.Collections;

public class ScreenShake : MonoBehaviour {

    public void CameraShake() {
        StartCoroutine(Shake(0.3f, 0.1f));
    }

    IEnumerator Shake(float duration, float magnitude) {
        float elapsed = 0.0f;
        Vector3 offset;

        while (elapsed < duration) {
            elapsed += Time.deltaTime;

            offset = new Vector3(Random.Range(-magnitude, magnitude), Random.Range(-magnitude, magnitude));

            Camera.main.transform.position += offset;
            yield return null;
            Camera.main.transform.position -= offset;

        }

    }
}