using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Spawner : MonoBehaviour {
    public GameObject itemPrefab;

    GameObject item;
    public GameObject bodyColliderPrefab;

    GameObject env;

    Vector2 vMin, vMax;
    float buffer = 0.5f;

    public bool forceClean;

    public Transform PICIKOKOTKURVA() {
        return item.transform;
    }
 


    void Awake() {
        env = GameObject.Find("Environment");
        if (env == null) {
            env = gameObject;
        }
    }


    void Start() {
        item = Instantiate(itemPrefab);
        item.transform.position = Vector2.zero;
        vMin = Camera.main.ScreenToWorldPoint(Vector2.zero);
        vMax = Camera.main.ScreenToWorldPoint(new Vector2(Camera.main.pixelWidth, Camera.main.pixelHeight));

        bodyColliderPrefab.CreatePool();
    }


    Vector2 GetFreePosition() {
        Vector2 candidate = Vector2.zero;
        for (int i = 0; i < 5; i++) {
            candidate = new Vector2(Random.Range(vMin.x + buffer, vMax.x - buffer), Random.Range(vMin.y + buffer, vMax.y - buffer));

            if (Physics2D.OverlapCircle(candidate, 1.0f) == null) break;

        }

        return candidate;
    }


    public void RespawnStar(bool reset = false) {
        if (reset) {
            item.transform.position = Vector2.zero;
        } else {
            item.transform.position = GetFreePosition();
        }

        item.SetActive(true);
    }


    public void SpawnCollider(Vector3 pos, float delay, float lifetime) {
        if (lifetime <= delay) return;
        GameObject coll = bodyColliderPrefab.Spawn(env.transform, pos);
        coll.SetActive(false);

        StartCoroutine(EnableAfter(coll, delay));
        StartCoroutine(RecycleAfter(coll, lifetime));
    }


    IEnumerator EnableAfter(GameObject go, float delay) {
        yield return new WaitForSeconds(delay);
        go.SetActive(true);
        //BoxCollider2D collider = go.GetComponent<BoxCollider2D>();
        //collider.enabled = true;
    }


    IEnumerator RecycleAfter(GameObject go, float delay) {
        yield return new WaitForSeconds(delay);

        if (go == null) yield break;

        //BoxCollider2D collider = go.GetComponent<BoxCollider2D>();
        //collider.enabled = false;

        go.Recycle();
    }


    void CleanColliders() {
        bodyColliderPrefab.RecycleAll();
    }


    public void ExplodeColliders() {

        foreach (GameObject go in bodyColliderPrefab.GetSpawned()) {
            ParticleSystem ps = go.GetComponent<ParticleSystem>();
            if (!ps.isPlaying) {
                ps.Play();
            }

            //BoxCollider2D collider = go.GetComponent<BoxCollider2D>();
            //collider.enabled = false;
        }

        Invoke("CleanColliders", 2.0f);

    }

}
