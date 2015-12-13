using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class RandomStars : MonoBehaviour {

    public GameObject star;
    public int max;

    int count = 0;
    Vector2 vMin, vMax;
    GameObject[] stars;


    void Awake() {
        stars = new GameObject[max];
    }


    void Start() {
        vMin = Camera.main.ScreenToWorldPoint(Vector2.zero);
        vMax = Camera.main.ScreenToWorldPoint(new Vector2(Camera.main.pixelWidth, Camera.main.pixelHeight));
    }


    void Update() {
        if (Random.Range(0, 100) < 1) {
            GameObject go;
            if (count < max) {
                go = Instantiate(star);
                go.transform.SetParent(Camera.main.transform);
                stars[count] = go;
                count++;
            } else {
                go = stars[Random.Range(0, max)];
            }

            go.transform.position = new Vector2(Random.Range(vMin.x, vMax.x), Random.Range(vMin.y, vMax.y));
        }
    }


}

