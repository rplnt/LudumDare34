using UnityEngine;
using System.Collections;

public class SnakeHead : MonoBehaviour {

    public float rotationAmount;
    public float speed;

    public float initialTrailTime;

    bool paused = true;
    bool gameOver = false;

    int score = 0;
    int best = 0;

    public GameObject bodyParent;
    public GameObject global;

    Spawner spawner;

    TrailRenderer tr;
    SpriteRenderer sr;
    UIManager ui;

    Vector3 screenPosition;
    bool warning = false;
    float outsideTimer = 3.0f;

    float collSpawnDelay;
    float lastSpawn = 0.0f;

    AudioManager am;


    void Awake() {
        tr = gameObject.GetComponent<TrailRenderer>();
        sr = gameObject.GetComponent<SpriteRenderer>();
        ui = global.GetComponent<UIManager>();
        spawner = global.GetComponent<Spawner>();
        collSpawnDelay = 0.3f / speed;
    }


    void Start() {
        am = AudioManager.GetInstance();
        Reset();
    }


    void Reset() {
        tr.time = initialTrailTime;
        tr.enabled = true;
        sr.enabled = true;
        score = 0;
        transform.position = new Vector2(0.0f, -4.0f);
        transform.rotation = Quaternion.identity;
        gameOver = false;
        best = PlayerPrefs.GetInt("best_score");
        TogglePause(true);
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


    void ExtendSnake() {
    }

    public void RotateLeft() {
        Rotate(rotationAmount);
    }


    public void RotateRight() {
        Rotate(-rotationAmount);
    }


    public void RotateTowards(Vector3 pos) {
        if (paused || gameOver) return;
        Debug.DrawRay(transform.position, transform.up);
        Debug.DrawRay(transform.position, pos - gameObject.transform.position);
        Vector2 direction = (pos - gameObject.transform.position);
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg - 90.0f;
        Quaternion rotation = Quaternion.AngleAxis(angle, Vector3.forward);
        transform.rotation = Quaternion.Slerp(transform.rotation, rotation, Time.deltaTime * rotationAmount);
    }


    void Rotate(float rotation) {
        if (paused || gameOver) return;
        transform.Rotate(0.0f, 0.0f, rotation * Time.deltaTime);
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
            }
            outsideTimer -= Time.deltaTime;

            if (outsideTimer < 0.0f) {
                Die();
            }

        } else if (warning) {
            warning = false;
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

        if (score > best) {
            PlayerPrefs.SetInt("best_score", score);
        }
        am.PlayExplosion();

        ui.ShowGameOver();

        Invoke("Reset", 2.0f);
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
