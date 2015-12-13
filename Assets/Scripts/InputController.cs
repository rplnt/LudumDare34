using UnityEngine;
using System.Collections;

public class InputController : MonoBehaviour {

    public GameObject head_go;
    SnakeHead head;

    void Awake() {
        head = head_go.GetComponent<SnakeHead>();
    }
	
    void Update() {
        if (Input.touchCount > 0) {
            // pass
        }

        //if (Input.GetMouseButton(0)) {
        //    Vector3 click = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        //    head.RotateTowards(click);
        //}

        if (Input.GetKey(KeyCode.LeftArrow) || Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.H)) {
            head.RotateLeft();
        } else if (Input.GetKey(KeyCode.RightArrow) || Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.L)) {
            head.RotateRight();
        } else if (Input.GetKeyDown(KeyCode.P)) {
            head.TogglePause();
        }
	}
}
