using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class UIManager : MonoBehaviour {

    public GameObject intro;
    public GameObject over;

    void Awake() {
        //intro = GameObject.Find("Intro");
        //over = GameObject.Find("Game Over");
    }

    public void DisableMenus() {
        intro.SetActive(false);
        over.SetActive(false);
    }


    public void ShowGameOver() {
        over.SetActive(true);
    }
}