using UnityEngine;
using System.Collections;

public class InputController : MonoBehaviour {

    public GameObject head_go;
    SnakeHead head;
    AudioManager am;


    void Awake() {
        head = head_go.GetComponent<SnakeHead>();
    }


    void Start() {
        am = AudioManager.GetInstance();
    }
	

    void Update() {
        //if (Input.GetMouseButton(0)) {
        //    Vector3 click = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        //    head.RotateTowards(click);
        //}

        if (Input.GetKeyDown(KeyCode.LeftArrow)) {
            StartCoroutine(head.StartGameFromMenu(0));
        } else if (Input.GetKeyDown(KeyCode.RightArrow)) {
            StartCoroutine(head.StartGameFromMenu(1));
        } else if (Input.GetKeyDown(KeyCode.Escape)) {
            head.Escape();
        }

        if (Input.GetKey(KeyCode.LeftArrow) || Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.H) || Input.GetKey(KeyCode.Keypad4)) {
            head.RotateLeft();
        } else if (Input.GetKey(KeyCode.RightArrow) || Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.L) || Input.GetKey(KeyCode.Keypad6)) {
            head.RotateRight();
        } else if (Input.GetKeyDown(KeyCode.M)) {
            am.ToggleMute();
        } else if (Input.GetKeyDown(KeyCode.Space)) {
            head.StartGame();
        }
	}


    public void OpenTwitter() {
#if UNITY_STANDALONE
        Application.OpenURL("https://twitter.com/rplnt");
#endif
    }
}
