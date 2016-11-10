using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class UIManager : MonoBehaviour {

    string bannerProvider = "http://services.ozerogames.com/spacesnake/banner";

    public GameObject intro;
    public GameObject over;
    public GameObject scoreboard;
    public GameObject timer;

    [System.Serializable]
    class BannerData {
        public string banner = null;
        public string target = null;

        public static BannerData CreateFromJSON(string jsonString) {
            return JsonUtility.FromJson<BannerData>(jsonString);
        }
    }

    float timerValue;

    void Awake() {
        //intro = GameObject.Find("Intro");
        //over = GameObject.Find("Game Over");
    }

    public void DisableMenus() {
        intro.SetActive(false);
        over.SetActive(false);
        scoreboard.SetActive(true);
        timer.SetActive(false);
    }


    public void ShowMenu() {
        StartCoroutine(LoadBanner());
        DisableMenus();
        scoreboard.SetActive(false);
        intro.SetActive(true);
    }


    IEnumerator LoadBanner() {
        GameObject credits = intro.transform.Find("Credits").gameObject;
        GameObject bannerGO = intro.transform.Find("Banner").gameObject;
        bannerGO.SetActive(false);

        WWW response = new WWW(bannerProvider);
        yield return response;

        if (response.isDone && response.error == null) {
            BannerData data = BannerData.CreateFromJSON(response.text);

            if (data.banner != null) {
                response = new WWW(data.banner);
                yield return response;

                if (response.isDone && response.error == null) {
                    
                    UnityEngine.UI.Image bannerImg = bannerGO.GetComponentInChildren<UnityEngine.UI.Image>();
                    bannerImg.sprite = Sprite.Create(response.texture, new Rect(0.0f, 0.0f, response.texture.width, response.texture.height), Vector2.zero);

                    if (data.target != null) {
                        Button bannerButton = bannerGO.GetComponentInChildren<Button>();
                        bannerButton.onClick.AddListener(() => { Application.OpenURL(data.target); });
                    }

                    bannerGO.SetActive(true);
                }
            }
        }

        credits.SetActive(!bannerGO.activeSelf);
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
            bool activate = true;
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

                case "Share":
                    if (!(score > best) || (score < 10)) {
                        activate = false;
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

            child.gameObject.SetActive(activate);
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
