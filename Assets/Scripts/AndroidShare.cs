using UnityEngine;
using System.Collections;
using UnityEngine.Analytics;

/*
 * Credits: http://forum.unity3d.com/threads/creating-a-share-button-intent-for-android-in-unity-that-forces-the-chooser.335751/
 */

public class AndroidShare : MonoBehaviour {

    string url = "https://goo.gl/cmbI1K";
    public GameObject head;
    SnakeHead headScript;

    void Start() {
        headScript = head.GetComponent<SnakeHead>();
    }

    public void shareBestScore() {
        //execute the below lines if being run on a Android device
#if UNITY_ANDROID

        Analytics.CustomEvent("AndroidShare");

        //Reference of AndroidJavaClass class for intent
        AndroidJavaClass intentClass = new AndroidJavaClass("android.content.Intent");
        //Reference of AndroidJavaObject class for intent
        AndroidJavaObject intentObject = new AndroidJavaObject("android.content.Intent");
        //call setAction method of the Intent object created
        intentObject.Call<AndroidJavaObject>("setAction", intentClass.GetStatic<string>("ACTION_SEND"));
        //set the type of sharing that is happening
        intentObject.Call<AndroidJavaObject>("setType", "text/plain");

        //add data to be passed to the other activity i.e., the data to be sent
        intentObject.Call<AndroidJavaObject>("putExtra", intentClass.GetStatic<string>("EXTRA_SUBJECT"), "SpaceSnake - New Record! " + headScript.score + " points");
        string shareText = "I just scored " + headScript.score + " points in SpaceSnake, can you beat me?\nGet SpaceSnake for Android here: " + url + "";
        intentObject.Call<AndroidJavaObject>("putExtra", intentClass.GetStatic<string>("EXTRA_TEXT"), shareText);

        //get the current activity
        AndroidJavaClass unity = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
        AndroidJavaObject currentActivity = unity.GetStatic<AndroidJavaObject>("currentActivity");
        //start the activity by sending the intent data
        AndroidJavaObject jChooser = intentClass.CallStatic<AndroidJavaObject>("createChooser", intentObject, "Share Via");
        currentActivity.Call("startActivity", jChooser);
#endif

    }

}
