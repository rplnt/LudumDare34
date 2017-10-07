using System.Collections;
using System;
using System.Collections.Generic;
using System.Globalization;
using UnityEngine;


public class Locale : MonoBehaviour {

    public SystemLanguage defaultLanguage;
    public bool overrideDefaultLanguage;
    public SystemLanguage language;

    public static SystemLanguage Language { get; private set; }
    public static CultureInfo ci;


    static Dictionary<SystemLanguage, string> localeCodes = new Dictionary<SystemLanguage, string>() {
        {SystemLanguage.English, "en-US"},
        {SystemLanguage.Czech, "cs-CZ"},
        {SystemLanguage.Chinese, "zh-CN"},
        {SystemLanguage.French, "fr-FR"},
        {SystemLanguage.German, "de-DE"},
        {SystemLanguage.Italian, "it-IT"},
        {SystemLanguage.Japanese, "ja-JP"},
        {SystemLanguage.Polish, "pl-PL" },
        {SystemLanguage.Portuguese, "pt-PT"},
        {SystemLanguage.Russian, "ru-RU" },
        {SystemLanguage.Slovak, "sk-SK"},
        {SystemLanguage.Spanish, "es-ES"},
        // TODO indonesia
    };


    private void Awake() {
        if (Application.systemLanguage == defaultLanguage) return;
        if (overrideDefaultLanguage && localeCodes.ContainsKey(language)) {
            ChangeLanguage(language);
        } else if (localeCodes.ContainsKey(Application.systemLanguage)) {
            ChangeLanguage(Application.systemLanguage);
        } else {
            ChangeLanguage(defaultLanguage);
        }
    }


    public static void ChangeLanguage(SystemLanguage newLanguage) {
        if (localeCodes.ContainsKey(newLanguage)) {
            Debug.Log("Changed language to " + newLanguage);
            Language = newLanguage;
            UpdateLocale();
        }
    }


    static void UpdateLocale() {
        try {
            ci = new CultureInfo(localeCodes[Language]);
        } catch (ArgumentException) {
            Debug.LogError(string.Format("Could not update locale for {0}", Language));
        }
    }
}
