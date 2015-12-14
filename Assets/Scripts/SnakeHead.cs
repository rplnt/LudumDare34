using UnityEngine;
using System.Collections;

public class SnakeHead : MonoBehaviour {

    public float defaultRotationAmount;
    public float defaultSpeed;
    float speed;
    float rotationAmount;

    public float initialTrailTime;

    bool paused = true;
    bool gameOver = false;

    Mode mode;
    int score = 0;
    int best = 0;

    public GameObject bodyParent;
    public GameObject global;

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


    public enum Mode {
        CLASSIC, INSANE
    };


    void Awake() {
        tr = gameObject.GetComponent<TrailRenderer>();
        sr = gameObject.GetComponent<SpriteRenderer>();
        ui = global.GetComponent<UIManager>();
        pb = Camera.main.gameObject.GetComponent<PulseBorders>();
        spawner = global.GetComponent<Spawner>();
    }


    void Start() {
        am = AudioManager.GetInstance();
        ui.ShowMenu();
    }


    string ScoreKey {
        get {
            return "BEST_" + System.Enum.GetName(typeof(Mode), mode);
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
        score = 0;
        ui.UpdateScore(0);
        best = PlayerPrefs.GetInt(ScoreKey);
        pb.StopPulser();
        collSpawnDelay = 0.3f / defaultSpeed;
    }


    void ResetPosition() {
        transform.position = new Vector2(0.0f, -3.5f);
        transform.rotation = Quaternion.identity;
        sr.enabled = true;
    }


    public void Escape() {
        if (!gameOver) return;
        ResetPosition();
        pb.StopPulser();
        gameOver = false;
        TogglePause(true);
        ui.ShowMenu();
        spawner.Respawn(true);
    }


    public IEnumerator StartGameFromMenu(int mode) {

        yield return new WaitForSeconds(0.1f);
        
        if (paused && !gameOver) {
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


    //public void RotateTowards(Vector3 pos) {
    //    /* TODO fix */
    //    if (paused || gameOver) return;
    //    Debug.DrawRay(transform.position, transform.up);
    //    Debug.DrawRay(transform.position, pos - gameObject.transform.position);
    //    Vector2 direction = (pos - gameObject.transform.position);
    //    float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg - 90.0f;
    //    Quaternion rotation = Quaternion.AngleAxis(angle, Vector3.forward);
    //    transform.rotation = Quaternion.Slerp(transform.rotation, rotation, Time.deltaTime * rotationAmount);
    //}


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
        am.PlayToink();
        edible.SetActive(false);
        StartCoroutine(ProlongTrail(0.5f, 1.0f));
        spawner.Respawn();
        score++;
        ui.UpdateScore(score);

        if (mode == Mode.INSANE) {
            speed += 0.25f;
            collSpawnDelay = 0.3f / speed;
            rotationAmount += 3.5f;

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

        ui.ShowGameOver(score, best);
        if (score > best) {
            PlayerPrefs.SetInt(ScoreKey, score);
        }

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

}
