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

    float collSpawnDelay;
    float lastSpawn = 0.0f;

    AudioManager am;


    void Awake() {
        tr = gameObject.GetComponent<TrailRenderer>();
        sr = gameObject.GetComponent<SpriteRenderer>();
        spawner = global.GetComponent<Spawner>();
        collSpawnDelay = 0.3f / speed;
        am = AudioManager.GetInstance();

        best = PlayerPrefs.GetInt("best_score");
    }


    void Start() {
        tr.time = initialTrailTime;
    }


    void Reset() {
        tr.time = initialTrailTime;
        tr.enabled = true;
        sr.enabled = true;
        score = 0;
        transform.position = new Vector2(0.0f, -4.0f);
        transform.rotation = Quaternion.identity;
        gameOver = false;
        paused = true;
    }


    public void TogglePause() {
        paused = !paused;
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
        if (paused) return;
        Debug.DrawRay(transform.position, transform.up);
        Debug.DrawRay(transform.position, pos - gameObject.transform.position);
        Vector2 direction = (pos - gameObject.transform.position);
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg - 90.0f;
        Quaternion rotation = Quaternion.AngleAxis(angle, Vector3.forward);
        transform.rotation = Quaternion.Slerp(transform.rotation, rotation, Time.deltaTime * rotationAmount);
    }


    void Rotate(float rotation) {
        transform.Rotate(0.0f, 0.0f, rotation * Time.deltaTime);
    }


    void Update() {
        if (paused || gameOver) return;
        
        transform.position = transform.position + (transform.up * Time.deltaTime * speed);

    }


    void OnTriggerEnter2D(Collider2D coll) {
        switch (coll.gameObject.tag) {
            case "body":
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


    void FixedUpdate() {
        if (paused || gameOver) return;

        if (Time.time - lastSpawn > collSpawnDelay) {
            spawner.SpawnCollider(transform.position, collSpawnDelay, tr.time * 0.85f);
            lastSpawn = Time.time;
        }
    }

}
