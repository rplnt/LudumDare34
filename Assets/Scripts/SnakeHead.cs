using UnityEngine;
using System.Collections;

public class SnakeHead : MonoBehaviour {

    public float rotationAmount;
    public float speed;

    public bool paused = true;
    bool gameOver = false;

    public GameObject link;
    public GameObject bodyParent;

    SnakeMover snake;


    void Start() {
        snake = new SnakeMover(gameObject, speed);

        for (int i = 0; i < 10; i++) {
            snake.ExtendTail(AddPart());
        }
    }


    GameObject AddPart() {
        GameObject newTail = Instantiate(link);
        newTail.transform.position = snake.Tail.transform.position + snake.Tail.transform.up * -0.15f;
        newTail.transform.SetParent(bodyParent.transform);
        newTail.GetComponent<SpriteRenderer>().sortingOrder = 500 - snake.Length;
        newTail.name = "Unit " + (snake.Length);

        StartCoroutine(GrowTail(newTail.transform, 0.75f, true));
        if (snake.Length > 1) {
            StartCoroutine(GrowTail(snake.Tail.transform, 1.0f, false));
        }

        return newTail;
    }


    IEnumerator GrowTail(Transform tail, float target, bool fromZero = true, float duration = 0.8f) {
        float elapsed = 0.0f;
        if (fromZero) {
            tail.localScale = Vector2.zero;
        }
        Vector2 targetVector = new Vector2(target, target);

        while (elapsed < duration) {
            elapsed += Time.deltaTime;

            tail.localScale = Vector2.Lerp(tail.localScale, targetVector, elapsed / duration);

            yield return null;
        }

        transform.localScale = targetVector;
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

        

        Debug.DrawLine(transform.position, transform.position + transform.up);
        transform.position = transform.position + (transform.up * Time.deltaTime * speed);
        snake.UpdateBody();
        
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
        snake.ExtendTail(AddPart());
        Destroy(edible);
    }

}
