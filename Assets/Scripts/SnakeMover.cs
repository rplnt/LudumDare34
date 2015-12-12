using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class SnakeMover {
    GameObject head;
    List<GameObject> body;

    float speed;


    public SnakeMover(GameObject head, float speed) {
        body = new List<GameObject>();
        body.Add(head);
        this.head = head;
        this.speed = speed;

    }
    

    public void ExtendTail(GameObject tail) {
        body.Add(tail);
    }


    //public void UpdateBody() {
    //    for (int i = body.Count - 1; i > 0; i--) {
    //        Vector3 dir = body[i - 1].transform.position - body[i].transform.position;
    //        float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg - 90;
    //        body[i].transform.rotation = Quaternion.Lerp(body[i].transform.rotation, Quaternion.AngleAxis(angle, Vector3.forward), 180 * Time.deltaTime);

    //        body[i].transform.position = body[i].transform.position + (body[i].transform.up * Time.deltaTime * speed);
    //    }
    //}



    public void UpdateBody() {
        for (int i = body.Count - 1; i > 0; i--) {
            body[i].transform.position = Vector3.Lerp(body[i].transform.position, body[i - 1].transform.position, 0.15f);
        }
    }


    public GameObject Tail {
        get {
            if (body.Count > 0) {
                return body[body.Count - 1];
            } else {
                return head;
            }
        }
    }


    //public void AddSnapshot() {
    //    Snapshot snap;
    //    snap.position = head.transform.position;
    //    snap.rotation = head.transform.rotation;

    //    frame++;
        
    //    snapshots.Add(snap);
    //}


    //public void PlaySnapshot() {
        
    //    for (int i = 1; i < body.Count; i++) {
    //        body[i].transform.position = snapshots[frame%10000].position;
    //        body[i].transform.rotation = snapshots[frame%10000].rotation;
    //    }

    //    if (snapshots.Count > 10000 + body.Count) {
    //        snapshots.RemoveAt(0);
    //    }
    //}


    public int Length {
        get {
            return body.Count;
        }
    }

}