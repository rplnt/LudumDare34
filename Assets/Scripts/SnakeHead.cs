using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.Analytics;

public class SnakeHead : MonoBehaviour {

    public float defaultRotationAmount;
    public float defaultSpeed;
    float speed;
    float rotationAmount;

    float startTime;

    public float initialTrailTime;

    bool paused = true;
    bool gameOver = false;

    Mode mode;
    public int Score { get; protected set; }

    public GameObject bodyParent;
    public GameObject global;

    [Header("Leaderboard")]
    public string dreamloUrl;
    public string dreamloCode;

    Spawner spawner;

    AudioManager am;
    TrailRenderer tr;
    SpriteRenderer sr;
    UIManager ui;
    PulseBorders pb;

    Vector3 screenPosition;
    bool warning = false;
    float outsideTimer = 3.0f;

    float collSpawnDelay;
    float lastSpawn = 0.0f;

    string playerId;


    public enum Mode {
        CLASSIC, INSANE
    };


    void Awake() {
        tr = gameObject.GetComponent<TrailRenderer>();
        sr = gameObject.GetComponent<SpriteRenderer>();
        ui = global.GetComponent<UIManager>();
        pb = Camera.main.gameObject.GetComponent<PulseBorders>();
        spawner = global.GetComponent<Spawner>();
        playerId = SystemInfo.deviceUniqueIdentifier.Substring(0, Mathf.Min(16, SystemInfo.deviceUniqueIdentifier.Length));

        BackupOldBestScore();
    }


    void BackupOldBestScore() {
        int legacy_best = PlayerPrefs.GetInt("BEST_" + ScoreKey);
        if (legacy_best == 0) return;
        int sk_best = ScoreKeeper.BestScore(ScoreKey);
        if (sk_best > 0) return;

        ScoreKeeper.Record[] lastList, bestList;
        ScoreKeeper.NewScore(ScoreKey, legacy_best, out lastList, out bestList);
        StartCoroutine(PostHighScore(legacy_best, 0.0f));
        Debug.Log("Loaded legacy record " + legacy_best + " into score keeper.");
    }


    float CalculateHeadPosition() {
        int height = Camera.main.pixelHeight;

        return Camera.main.ScreenToWorldPoint(new Vector3(0, (float)height * 0.1f)).y;
    }


    void Start() {
        am = AudioManager.GetInstance();
        ui.ShowMenu();
        ResetPosition();
    }


    string ScoreKey {
        get {
            return System.Enum.GetName(typeof(Mode), mode);
        }
    }


    public bool Playing {
        get {
            return (!paused && !gameOver);
        }
    }

    void Reset() {
        ResetPosition();
        tr.time = initialTrailTime;
        tr.enabled = true;
        speed = defaultSpeed + (mode == Mode.INSANE ? 1.0f : 0.0f);
        rotationAmount = defaultRotationAmount + (mode == Mode.INSANE ? 10.0f : 0.0f);
        ui.DisableMenus();        
        gameOver = false;
        Score = 0;
        ui.UpdateScore(0);
        //best = PlayerPrefs.GetInt(ScoreKey);
        pb.StopPulser();
        collSpawnDelay = 0.4f / defaultSpeed;
        startTime = Time.time;
    }


    void ResetPosition() {
        transform.position = new Vector3(0.0f, CalculateHeadPosition());
        transform.rotation = Quaternion.identity;
        sr.enabled = true;
    }


    public void Escape() {
        //if (!gameOver) return;
        ResetPosition();
        pb.StopPulser();
        ui.DisableMenus();
        gameOver = false;
        TogglePause(true);
        ui.ShowMenu();
        spawner.RespawnStar(true);
    }


    public IEnumerator StartGameFromMenu(int mode) {

        yield return new WaitForSeconds(0.1f);
        
        if (!Playing) {
            StartGame(mode);
        }
    }


    public void StartGame() {
        if (!gameOver) return;
        StartGame((int)mode);
    }


    public void StartGame(int modeCode) {
        this.mode = (Mode)modeCode;
        Reset();
        TogglePause(false);
    }


    public void TogglePause() {
        if (gameOver) return;
        paused = !paused;
        am.paused = paused;
    }


    public void TogglePause(bool status) {
        if (gameOver) return;
        if (!status) {
            ui.DisableMenus();
        }
        paused = status;
        am.paused = status;
    }


    public void RotateLeft() {
        Rotate(rotationAmount);
    }


    public void RotateRight() {
        Rotate(-rotationAmount);
    }


    void Rotate(float rotation) {
        if (paused || gameOver) return;
        transform.Rotate(0.0f, 0.0f, rotation * Time.deltaTime);
    }


    public bool RotateTowards(Vector3 pos) {
        if (paused || gameOver) return false;

        Vector3 direction = (pos - gameObject.transform.position);
        if (((Vector2)direction).magnitude < 3.5f && Vector2.Angle(direction, transform.up) > 120 && !warning) return false;

        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg - 90.0f;
        Quaternion rotation = Quaternion.AngleAxis(angle, Vector3.forward);

        transform.rotation  = Quaternion.RotateTowards(transform.rotation, rotation, Time.deltaTime * rotationAmount);

        return true;
    }


    void Update() {
        if (paused || gameOver) return;
        
        /* move */
        transform.position = transform.position + (transform.up * Time.deltaTime * speed);

        /* test outside borders */
        screenPosition = Camera.main.WorldToScreenPoint(transform.position);
        if (screenPosition.x < 0 || screenPosition.y < 0 || screenPosition.x > Camera.main.pixelWidth || screenPosition.y > Camera.main.pixelHeight) {
            if (!warning) {
                warning = true;
                outsideTimer = 1.0f;
                ui.StartTimer(outsideTimer * 60.0f);
                pb.StartPulser();
            }
            outsideTimer -= Time.deltaTime;

            am.PlaySiren();
            ui.UpdateTimer(outsideTimer * 60.0f);

            if (outsideTimer < 0.0f) {
                Die();
            }

        } else if (warning) {
            warning = false;
            am.StopSiren();
            ui.StopTimer();
            pb.StopPulser();
        }

        /* spawn colliders */
        lastSpawn += Time.deltaTime;
        if (lastSpawn > collSpawnDelay) {
            spawner.SpawnCollider(transform.position, collSpawnDelay, tr.time * 0.85f);
            lastSpawn = 0.0f;
        }
    }


    void OnTriggerEnter2D(Collider2D coll) {
        switch (coll.gameObject.tag) {
            case "body":
                if (gameOver) {
                    break;
                }
                //Debug.Break();
                Die();
                break;

            case "edible":
                Eat(coll.gameObject);
                break;
        }
    }

    void DisableTrail() {
        tr.enabled = false;
    }


    void Eat(GameObject edible) {
        am.PlayToink(edible.transform.position);
        edible.SetActive(false);
        StartCoroutine(ProlongTrail(0.5f, 1.0f));
        spawner.RespawnStar();
        Score++;
        ui.UpdateScore(Score);

        if (mode == Mode.INSANE) {
            speed += 0.25f;
            collSpawnDelay = 0.3f / speed;
            rotationAmount += 3.5f;

        } else {
            speed += 0.025f;
            collSpawnDelay = 0.3f / speed;
            rotationAmount += 0.4f;
        }
    }


    void Die() {
        spawner.ExplodeColliders();
        Invoke("DisableTrail", 0.1f);
        gameOver = true;
        sr.enabled = false;
        ScreenShake ss = Camera.main.GetComponent<ScreenShake>();
        if (ss != null) {
            ss.CameraShake();
        }

        ScoreKeeper.Record[] lastList, topList;
        int recordIndex = ScoreKeeper.NewScore(ScoreKey, Score, out lastList, out topList);

        IEnumerator finalScreen = recordIndex >= 0 || (topList[topList.Length - 1] == null || topList[topList.Length - 1].score == 0) ? 
                                  ui.ShowHighScoreList(topList, recordIndex) : 
                                  ui.ShowLastScoreGraph(lastList, topList[0].score);
        ui.ShowFinalScreen(finalScreen);

        if (mode == Mode.CLASSIC && recordIndex == 0) {
            StartCoroutine(PostHighScore(Score, Time.time - startTime));
        }
        Analytics.CustomEvent("GameOver", new Dictionary<string, object>
          {
            {"Score", Score},
            {"Best", (recordIndex >= 0) },
            {"Time", Time.time - startTime},
            {"GameNumber", ScoreKeeper.GetGameCount(ScoreKey) }
          });

        am.PlayExplosion();
        am.paused = true;
        pb.StartPulser();
    }


    IEnumerator ProlongTrail(float amount, float time) {
        float elapsed = 0.0f;
        float initial = tr.time;

        while (elapsed < time) {
            elapsed += Time.deltaTime;

            tr.time = Mathf.Lerp(initial, initial + amount, elapsed / time);

            yield return null;
        }

        tr.time = initial + amount;
    }


    IEnumerator PostHighScore(int score, float elapsedTime) {
        
        WWW response = new WWW(string.Format("{0}/{1}/add/{2}/{3}/{4}", dreamloUrl, dreamloCode, WWW.EscapeURL(playerId), score, Mathf.RoundToInt(elapsedTime)));
        yield return response;

        if (response.isDone && response.error == null) {
            Debug.Log("Posted best score");
        } else {
            Debug.LogError("Failed to post best score of " + score);
        }

        yield return null;
    }

}
