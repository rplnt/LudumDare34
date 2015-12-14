using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class UIManager : MonoBehaviour {

    public GameObject intro;
    public GameObject over;
    public GameObject scoreboard;
    public GameObject timer;

    float timerValue;

    void Awake() {
        //intro = GameObject.Find("Intro");
        //over = GameObject.Find("Game Over");
    }

    public void DisableMenus() {
        intro.SetActive(false);
        over.SetActive(false);
        scoreboard.SetActive(true);
    }


    public void ShowMenu() {
        DisableMenus();
        intro.SetActive(true);
    }


    public void ShowGameOver(int score, int best) {
        Text text = over.GetComponent<Text>();
        text.color = new Color(1, 1, 1, 0.0f);
        foreach (Transform child in over.transform) {
            child.gameObject.SetActive(false);
        }
        over.SetActive(true);
        scoreboard.SetActive(false);
        timer.SetActive(false);

        StartCoroutine(RevealGameOver(text, score, best));
    }


    IEnumerator RevealGameOver(Text goText, int score, int best, float delay = 1.3f) {
        float elapsed = 0.0f;

        while (elapsed < delay) {
            elapsed += Time.deltaTime;
            goText.color = new Color(goText.color.r, goText.color.g, goText.color.b, elapsed / delay);
            yield return null;
        }

        foreach (Transform child in over.transform) {
            switch (child.name) {
                case "new_record":
                    Text record = child.GetComponent<Text>();
                    if ((score > best) || best == 0) {
                        record.text = "NEW RECORD!";
                        StartCoroutine(FlickerText(record, 0.8f));                        
                    } else {
                        record.text = "Best score: " + best;
                    }
                    break;

                case "score":
                    Text scoreTxt = child.GetComponent<Text>();
                    if (scoreTxt != null) {
                        scoreTxt.text = "Score: " + score;
                    }
                    break;
            }

            //yield return new WaitForSeconds(0.2f);

            child.gameObject.SetActive(true);
        }
    }

    IEnumerator FlickerText(Text text, float delay) {
        if (text == null) yield break;
        GameObject parent = text.gameObject.transform.parent.gameObject;
        string content = text.text;

        while (parent != null && parent.activeSelf) {
            text.text = content;
            yield return new WaitForSeconds(delay);
            text.text = "";
            yield return new WaitForSeconds(delay);
        }
    }


    public void UpdateScore(int score) {
        Text text = scoreboard.GetComponent<Text>();
        text.text = "Score: " + score;
    }


    public void StartTimer(float time) {
        SetTimer(time);
        timer.SetActive(true);        
    }

    public void StopTimer() {
        timer.SetActive(false);
    }

    void SetTimer(float time) {
        timerValue = time;
        Text text = timer.GetComponent<Text>();
        text.text = Mathf.FloorToInt(time / 60) + ":" + Mathf.RoundToInt(time % 60).ToString().PadLeft(2, '0');
    }

    public void UpdateTimer(float time) {
        if (timerValue - time > 0.1f) {
            SetTimer(time);
        }
    }
}
