using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class UIManager : MonoBehaviour {

    string bannerProvider = "http://services.ozerogames.com/spacesnake/banner";
    string defaultTarget = "http://ozerogames.com/";

    public GameObject intro;
    public GameObject over;
    public GameObject scoreboard;
    public GameObject timer;
    public GameObject loadOverlay;
    public AndroidStore store;

    [Header("Final Screen")]
    public Transform topScoresContainer;
    public Transform lastScoresContainer;
    Transform lastScoresGrapgPoints;
    public AnimationCurve graphAnim;

    Text scoreText;

    [System.Serializable]
    class BannerData {
        public string banner = null;
        public bool clickable = false;
        public string targetGoogle = null;
        public string targetAmazon = null;
        public string targetDefault = null;

        public static BannerData CreateFromJSON(string jsonString) {
            return JsonUtility.FromJson<BannerData>(jsonString);
        }
    }

    public enum AndroidStore { Google, Amazon, None };


    float timerValue;

    void Awake() {
        if (loadOverlay.activeSelf == false) {
            Debug.LogError("Load overlay is not active!");
        }
        lastScoresGrapgPoints = lastScoresContainer.Find("Points");
        //intro = GameObject.Find("Intro");
        //over = GameObject.Find("Game Over");
    }

    void Start() {
         scoreText = scoreboard.GetComponent<Text>();
         if (scoreText == null) {
             Debug.LogError("Could not find TEXT on scoreboard");
         }
         StartCoroutine(HideOverlay(0.666f));
    }

    IEnumerator HideOverlay(float duration) {
        Image overlay = loadOverlay.GetComponent<Image>();
        if (overlay == null) {
            loadOverlay.SetActive(false);
            yield break;
        }

        float elapsed = 0.0f;
        while (elapsed < duration) {
            elapsed += Time.deltaTime;

            overlay.color = new Color(0.0f, 0.0f, 0.0f, Mathf.Lerp(1.0f, 0.0f, elapsed / duration));

            yield return null;
        }

        loadOverlay.SetActive(false);
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

                    if (data.clickable) {
                        Button bannerButton = bannerGO.GetComponentInChildren<Button>();

                        string target = null;

                        if (store == AndroidStore.Google) {
                            target = data.targetGoogle;
                        } else if (store == AndroidStore.Amazon) {
                            target = data.targetAmazon;
                        } else if (store == AndroidStore.None) {
                            target = data.targetDefault;
                        } else {
                            Debug.LogError("Unknown store!");
                        }

                        if (target != null) {
                            Debug.Log(target);
                            bannerButton.onClick.AddListener(() => { Application.OpenURL(target); });
                        } else {
                            Debug.Log("Using local target");
                            bannerButton.onClick.AddListener(() => { Application.OpenURL(defaultTarget); });
                        }

                    }

                    bannerGO.SetActive(true);
                }
            }
        }

        credits.SetActive(!bannerGO.activeSelf);
    }


    public void ShowFinalScreen(IEnumerator finalScreen) {
        /* Hide everything */
        foreach (Transform child in over.transform) {
            child.gameObject.SetActive(false);
        }
        scoreboard.SetActive(false);
        timer.SetActive(false);

        /* Hide GAME OVER text */
        over.SetActive(true);
        Text text = over.GetComponent<Text>();
        text.color = new Color(1, 1, 1, 0.0f);

        /* Show either high scores or graph */
        StartCoroutine(RevealGameOver(text, finalScreen));
    }


    IEnumerator RevealGameOver(Text goText, IEnumerator showOverData, float delay = 1.3f) {
        float elapsed = 0.0f;

        /* Fade in GAME OVER text */
        while (elapsed < delay) {
            elapsed += Time.deltaTime;
            goText.color = new Color(goText.color.r, goText.color.g, goText.color.b, elapsed / delay);
            yield return null;
        }

        if (showOverData != null) {
            StartCoroutine(showOverData);
        } else {
            ShowOverButtons(false);
        }

        yield return null;
    }


    public IEnumerator ShowHighScoreList(ScoreKeeper.Record[] topList, int recordIndex, float duration = 1.5f) {
        topScoresContainer.parent.gameObject.SetActive(true);

        const int header = 1;  // TODO - shouldn't be set here
        Debug.Assert(topScoresContainer.childCount == topList.Length + header);

        /* Clean up table */
        for (int i = header; i < topScoresContainer.childCount; i++) {
            Transform row = topScoresContainer.GetChild(i);
            if (row == null) break;

            Text[] texts = row.GetComponentsInChildren<Text>();
            for (int n = 0; n < texts.Length; n++) {
                texts[n].text = "";
            }
        }
        yield return null;

        /* Roll table */
        for (int i = 0; i < topList.Length; i++) {
            Transform row = topScoresContainer.GetChild(i + 1);
            if (row == null) break;
            if (topList[i] == null || topList[i].score == 0) continue;

            /* delay */
            float elapsed = 0.0f;
            while (elapsed < (duration / topList.Length)) {
                elapsed += Time.deltaTime;
            }

            FillText(row.Find("Position"), ((i + 1)).ToString(), i == recordIndex ? Color.yellow : Color.white);
            FillText(row.Find("Timestamp"), i == recordIndex ? "NEW HIGH SCORE" : topList[i].date, i == recordIndex ? Color.yellow : Color.white);
            FillText(row.Find("Score"), topList[i].score.ToString(), i == recordIndex ? Color.yellow : Color.white);

            row.gameObject.SetActive(true);
            yield return null;
        }

        /* menu, share, repeat */
        ShowOverButtons(recordIndex >= 0);

        yield return null;
    }


    /// <summary>
    /// Show dot-graph plotting score from last games.
    /// </summary>
    /// <param name="lastList">List with score Records</param>
    /// <param name="bestScore">Current record</param>
    /// <param name="duration">Reveal duration</param>
    /// <returns></returns>
    public IEnumerator ShowLastScoreGraph(ScoreKeeper.Record[] lastList, int bestScore, float duration = 1.7f) {
        lastScoresContainer.parent.gameObject.SetActive(true);

        /* Find references */
        RectTransform xAxis = GameObject.Find("Lines/X").GetComponent<RectTransform>();
        RectTransform yAxis = GameObject.Find("Lines/Y").GetComponent<RectTransform>();
        RectTransform best = GameObject.Find("Lines/Max").GetComponent<RectTransform>();
        Text recordLabel = GameObject.Find("Graph/Record").GetComponent<Text>();
        GameObject avg = GameObject.Find("Lines/Avg");
        RectTransform xlimit = GameObject.Find("Lines/YMax").GetComponent<RectTransform>();

        /* Lines setup */
        xAxis.localScale = new Vector2(0.0f, 1.0f);
        yAxis.localScale = new Vector2(1.0f, 0.0f);
        best.localScale = new Vector2(0.0f, 1.0f);
        recordLabel.text = "";
        if (avg != null) {
            avg.GetComponent<Image>().color = new Color(1.0f, 1.0f, 1.0f, 0.0f);
        }

        /* Points setup */
        float scoreSum = 0;
        Debug.Assert(lastScoresGrapgPoints.childCount == lastList.Length);
        int pointsCount = lastScoresGrapgPoints.childCount;
        Rect graphArea = new Rect(yAxis.anchoredPosition.x, xAxis.anchoredPosition.y, xlimit.anchoredPosition.x - yAxis.anchoredPosition.x, best.anchoredPosition.y - xAxis.anchoredPosition.y);
        float horizontalStep = graphArea.width / 11.0f;
        RectTransform[] points = new RectTransform[pointsCount];

        /* Position points */
        for (int i = 0; i < pointsCount; i++) {
            points[i] = lastScoresGrapgPoints.GetChild(i).GetComponent<RectTransform>();
            if (points[i] == null) break;

            scoreSum += lastList[pointsCount - (i + 1)].score;
            points[i].localScale = Vector2.zero;
            points[i].GetComponent<Image>().color = (i + 1 == pointsCount) ? Color.yellow : Color.white;
            points[i].anchoredPosition = new Vector2(
                graphArea.xMin + (i + 1) * horizontalStep + 0.5f * yAxis.rect.width,
                graphArea.yMin + (bestScore > 0 ? ((float)lastList[pointsCount - (i + 1)].score / bestScore) : 0.0f) * graphArea.height + 0.5f * xAxis.rect.height
            );
        }
        yield return null;

        /* Average */
        if (avg != null) {
            RectTransform avgRT = avg.GetComponent<RectTransform>();
            avgRT.anchoredPosition = new Vector2(avgRT.anchoredPosition.x, graphArea.yMin + (bestScore > 0 ? ((scoreSum / pointsCount) / bestScore) : 0.0f) * graphArea.height);
        }

        /* Animate */
        float elapsed = 0.0f;
        while (elapsed < duration) {
            elapsed += Time.deltaTime;
            float baseEval = graphAnim.Evaluate(elapsed / duration);

            /* Lines */
            xAxis.localScale = new Vector2(baseEval, 1.0f);
            yAxis.localScale = new Vector2(1.0f, Mathf.Min(baseEval * 2.0f, 1.0f));
            best.localScale = new Vector2(Mathf.Max((baseEval - 0.5f) * 2.0f, 0.0f), 1.0f);  // start revealing halfway through
            if (baseEval > 0.5f && recordLabel.text == "") {
                recordLabel.text = bestScore.ToString();
            }
            if (avg != null) {
                avg.GetComponent<Image>().color = new Color(1.0f, 1.0f, 1.0f, Mathf.Max((baseEval - 0.8f) * 2.0f, 0.0f));
            }

            /* Points */
            float step = 1.0f / pointsCount;
            for (int i = 0; i < pointsCount; i++) {
                float scale = Mathf.Lerp(0.0f, 1.0f, (baseEval - i * step) / ((i + 1) * step - i * step));
                points[i].localScale = new Vector2(scale, scale);
            }

            yield return null;
        }

        ShowOverButtons(false);
        yield return null;
    }


    void ShowOverButtons(bool showShare) {
        over.transform.Find("Menu").gameObject.SetActive(true);
        over.transform.Find("Retry").gameObject.SetActive(true);

        if (showShare) {
            over.transform.Find("Share").gameObject.SetActive(true);
        }
    }


    void FillText(Transform obj, string text, Color color) {
        if (obj == null) return;
        Text txt = obj.GetComponent<Text>();
        if (txt == null) return;
        txt.text = text;
        txt.color = color;
    }


    IEnumerator FlickerText(Text text, float delay) {
        if (text == null) yield break;
        GameObject parent = text.gameObject.transform.parent.gameObject;
        string content = text.text;

        while (parent != null && parent.activeSelf && text.text != "")  {
            text.enabled = true;
            yield return new WaitForSeconds(delay);
            text.enabled = false;
            yield return new WaitForSeconds(delay);
        }
    }


    public void UpdateScore(int score) {
        scoreText.text = score.ToString();
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
