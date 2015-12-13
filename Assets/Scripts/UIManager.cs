using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class UIManager : MonoBehaviour {

    GameObject intro;

    void Awake() {
        intro = GameObject.Find("Intro");
    }

    public void DisableIntro() {
        intro.SetActive(false);
    }
}