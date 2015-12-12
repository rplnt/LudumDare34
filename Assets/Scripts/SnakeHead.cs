using UnityEngine;
using System.Collections;

public class SnakeHead : MonoBehaviour {

    public float rotationAmount;
    public float speed;

    public float initialTrailTime;

    bool paused = true;
    bool gameOver = false;

    int points = 0;

    public GameObject bodyParent;
    public GameObject global;

    Spawner spawner;

    TrailRenderer tr;

    Vector2[] runAround = new Vector2[4];

    float collSpawnDelay;
    float lastSpawn = 0.0f;


    void Awake() {
        tr = gameObject.GetComponent<TrailRenderer>();
        spawner = global.GetComponent<Spawner>();
        collSpawnDelay = 0.3f / speed;
    }


    void Start() {
        tr.time = initialTrailTime;
        TogglePause();
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


    public void RotateTowards(Vector2 pos) {
        throw new System.NotImplementedException();
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
                gameOver = true;
                break;

            case "edible":
                Eat(coll.gameObject);
                break;
        }
    }


    void Eat(GameObject edible) {
        edible.SetActive(false);
        StartCoroutine(ProlongTrail(0.5f, 1.0f));
        spawner.Respawn();
        points++;
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
        if (Time.time - lastSpawn > collSpawnDelay) {
            spawner.SpawnCollider(transform.position, collSpawnDelay, tr.time * 0.85f);
            lastSpawn = Time.time;
        }
    }

}
