using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Spawner : MonoBehaviour {
    public GameObject itemPrefab;

    GameObject item;
    public GameObject bodyColliderPrefab;

    List<GameObject> colliderPool;

    GameObject env;

    Vector2 pos;


    void Awake() {
        env = GameObject.Find("Environment");
        if (env == null) {
            env = gameObject;
        }
    }


    void Start() {
        item = Instantiate(itemPrefab);
        item.transform.position = Vector2.zero;
        colliderPool = new List<GameObject>();
    }


    Vector2 GetFreePosition() {
        return new Vector2(Random.Range(-3f, 3f), Random.Range(-3f, 3f));
    }


    public void Respawn() {
        item.transform.position = GetFreePosition();
        item.SetActive(true);
    }


    public void SpawnCollider(Vector3 pos, float delay, float lifetime) {
        if (lifetime < delay) {
            return;
        }

        GameObject coll;
        if (colliderPool.Count > 0) {
            coll = colliderPool[colliderPool.Count - 1];
            colliderPool.RemoveAt(colliderPool.Count - 1);
        } else {
            coll = Instantiate(bodyColliderPrefab);
            coll.transform.parent = env.transform;
        }

        coll.transform.position = pos;
        coll.SetActive(false);

        StartCoroutine(EnableAfter(coll, delay));
        StartCoroutine(DestroyAfter(coll, lifetime));
    }


    IEnumerator EnableAfter(GameObject go, float delay) {
        yield return new WaitForSeconds(delay);
        go.SetActive(true);        
    }


    IEnumerator DestroyAfter(GameObject go, float delay) {
        yield return new WaitForSeconds(delay);
        go.SetActive(false);
        colliderPool.Add(go);
    }

}