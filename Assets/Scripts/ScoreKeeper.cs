using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


/// <summary>
/// Class used for Saving and Reading scores from PlayerPrefs.
/// 
/// Keeps record of top scores and last scores as well.
/// </summary>
public static class ScoreKeeper {

    [Serializable]
    public class Record {
        public int score = 0;
        public string date = null;

        public Record(int score, DateTime date) {
            this.score = score;
            this.date = date.ToString("d", Locale.ci);
        }
    }

    const int STORE_SIZE = 10;
    const string STORE_PREFIX_LAST = "scores_last";
    const string STORE_PREFIX_TOP = "scores_top";
    const string STORE_PREFIX_COUNT = "scores_count";

    static string LastKey(string key) {
        return string.Format("{0}_{1}", STORE_PREFIX_LAST, key);
    }

    static string TopKey(string key) {
        return string.Format("{0}_{1}", STORE_PREFIX_TOP, key);
    }

    static string CountKey(string key) {
        return string.Format("{0}_{1}", STORE_PREFIX_COUNT, key);
    }

    //static void FillStorageKeys(string key, out string lastKey, out string topKey, out string countKey) {
    //    lastKey = string.Format("{0}_{1}", STORE_PREFIX_LAST, key);
    //    topKey = string.Format("{0}_{1}", STORE_PREFIX_TOP, key);
    //    countKey = string.Format("{0}_{1}", STORE_PREFIX_COUNT, key);
    //}


    /// <summary>
    /// Submit new score to Keeper
    /// </summary>
    /// <param name="key">Key under which records are to be kept</param>
    /// <param name="score"></param>
    /// <param name="lastList"></param>
    /// <param name="topList"></param>
    /// <returns>Returns index of new record, -1 if not in top X</returns>
    public static int NewScore(string key, int score, out Record[] lastList, out Record[] topList) {
        IncreaseCount(CountKey(key));
        SaveLast(LastKey(key), score, out lastList);
        return SaveTop(TopKey(key), score, out topList);
    }


    static void IncreaseCount(string fullKey) {
        int current = PlayerPrefs.GetInt(fullKey, 0);
        PlayerPrefs.SetInt(fullKey, current + 1);
    }


    public static int GetGameCount(string key) {
        return PlayerPrefs.GetInt(TopKey(key), 0);
    }


    public static int BestScore(string key) {
        Record[] records;
        LoadList(TopKey(key), out records);
        return records[0] == null ? 0 : records[0].score;
    }


    static void LoadList(string fullKey, out Record[] recordList) {
        if (PlayerPrefs.HasKey(fullKey)) {
            string data = PlayerPrefs.GetString(fullKey);
            recordList = JsonHelper.FromJson<Record>(data);
        } else {
            recordList = new Record[STORE_SIZE];
        }
    }


    static void SaveList(string fullKey, Record[] recordList) {
        string records = JsonHelper.ToJson<Record>(recordList);
        PlayerPrefs.SetString(fullKey, records);
    }


    // sorted from newest to oldest
    static void SaveLast(string lastKey, int score, out Record[] lastList) {
        LoadList(lastKey, out lastList);
        ShiftRecordsRight(lastList);
        lastList[0] = new Record(score, DateTime.Now);
        SaveList(lastKey, lastList);
    }


    // sorted from top
    static int SaveTop(string topKey, int score, out Record[] topList) {
        LoadList(topKey, out topList);
        int i;
        for (i = 0; i < topList.Length; i++) {
            if (topList[i] == null || topList[i].score < score) break;
        }
        if (i == topList.Length) return -1;

        ShiftRecordsRight(topList, stop: i);
        topList[i] = new Record(score, DateTime.Now);
        SaveList(topKey, topList);

        return i;
    }


    /// <summary>
    /// Shift array from "right", ending at "stop"
    /// </summary>
    /// <param name="recordList">Array to shift</param>
    /// <param name="to">Stop at</param>
    /// [0, 1, 2, 3, null, null]
    static void ShiftRecordsRight(Record[] recordList, int stop = 0) {
        for (int i = recordList.Length - 1; i > stop; i--) {
            if (recordList[i] == null) continue;
            recordList[i] = recordList[i - 1];
        }
    }
}


// https://stackoverflow.com/questions/36239705/serialize-and-deserialize-json-and-json-array-in-unity/36244111#36244111
public static class JsonHelper {
    public static T[] FromJson<T>(string json) {
        Wrapper<T> wrapper = UnityEngine.JsonUtility.FromJson<Wrapper<T>>(json);
        return wrapper.Records;
    }

    public static string ToJson<T>(T[] array, bool prettyPrint = false) {
        Wrapper<T> wrapper = new Wrapper<T>();
        wrapper.Records = array;
        return UnityEngine.JsonUtility.ToJson(wrapper, prettyPrint);
    }

    [System.Serializable]
    private class Wrapper<T> {
        public T[] Records;
    }
}