using UnityEngine;
using System.Collections;

public class InputController : MonoBehaviour {

    public GameObject head_go;
    SnakeHead head;

    public GameObject cursor;

    public bool handleMouseAsTouch;


    void Awake() {
        head = head_go.GetComponent<SnakeHead>();
        if (handleMouseAsTouch) {
            Debug.LogWarning("Using mouse as touch!");
        }
    }


    void Start() {
        cursor.SetActive(false);
    }


    public void DelayedStartClassic() {
        StartCoroutine(head.StartGameFromMenu(0));
    }
	

    void Update() {

        if (!head.Playing) {
            /* menus, etc */
            if (Input.GetKeyDown(KeyCode.LeftArrow)) {
                StartCoroutine(head.StartGameFromMenu(0));
            } else if (Input.GetKeyDown(KeyCode.RightArrow)) {
                StartCoroutine(head.StartGameFromMenu(1));
            } else if (Input.GetKeyDown(KeyCode.Escape)) {
                head.Escape();
            } else if (Input.GetKeyDown(KeyCode.Space)) {
                head.StartGame();
            }

            if (cursor.activeSelf) {
                cursor.SetActive(false);
            }

        } else {
            /* game*/

            bool posOK = true;
                /* touch */
            if (Input.touchCount > 0) {
                Touch touch = Input.GetTouch(0);
                Vector3 pos = Camera.main.ScreenToWorldPoint(touch.position);

                switch (touch.phase) {
                    case TouchPhase.Began:
                        cursor.SetActive(true);
                        cursor.transform.position = new Vector2(pos.x, pos.y);
                        //posOK = head.RotateTowards(pos);
                        break;
                    case TouchPhase.Moved:
                        if (cursor.activeSelf) {
                            cursor.transform.position = new Vector2(pos.x, pos.y);
                        }
                        break;
                    case TouchPhase.Ended:
                        cursor.SetActive(false);
                        break;
                    case TouchPhase.Canceled:
                        cursor.SetActive(false);
                        break;
                }

                posOK = head.RotateTowards(pos);

                if (!posOK) {
                    cursor.GetComponent<ParticleSystem>().startColor = Color.red;
                } else {
                    cursor.GetComponent<ParticleSystem>().startColor = Color.yellow;
                }

                /* mouse as touch */
            } else if (handleMouseAsTouch && Input.GetMouseButton(0)) {
                Vector3 click = Camera.main.ScreenToWorldPoint(Input.mousePosition);

                if (cursor.activeSelf) {
                    cursor.transform.position = new Vector2(click.x, click.y);
                }

                if (Input.GetMouseButtonDown(0)) {
                    cursor.transform.position = new Vector2(click.x, click.y);
                    cursor.SetActive(true);
                }

                posOK = head.RotateTowards(click);

                if (!posOK) {
                    cursor.GetComponent<ParticleSystem>().startColor = Color.red;
                } else {
                    cursor.GetComponent<ParticleSystem>().startColor = Color.yellow;
                }

            } else if (handleMouseAsTouch && Input.GetMouseButtonUp(0)) {
                cursor.SetActive(false);

                /* mouse */
            } else if (!handleMouseAsTouch && Input.GetMouseButton(0)) {
                head.RotateLeft();
            } else if (!handleMouseAsTouch && Input.GetMouseButton(1)) {
                head.RotateRight();

                /* keyboard */
            } else if (Input.GetKey(KeyCode.LeftArrow) || Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.H) || Input.GetKey(KeyCode.Keypad4)) {
                head.RotateLeft();
            } else if (Input.GetKey(KeyCode.RightArrow) || Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.L) || Input.GetKey(KeyCode.Keypad6)) {
                head.RotateRight();
            }

            if (Input.GetKeyDown(KeyCode.Escape)) {
                head.Escape();
            }

        }

        /* common */
        if (Input.GetKeyDown(KeyCode.Escape)) {
            Application.Quit();
        }
	}

    public void OpenTwitter() {
        Application.OpenURL("https://twitter.com/ozerogames");
    }

}
