using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using System.Collections.Generic;
using UnityEngine.Events;
using Namotion.Reflection;
using Newtonsoft.Json.Linq;

public class LoadingManager : MonoBehaviour
{
    public static LoadingManager instance { get; private set; }
    public Image loadingImage = null;
    public bool fadeImageWhenDone = true;


    public UnityEvent loadingFinished = new();
    public UnityEvent loadingRemoteConfigFinished = new();
    public UnityEvent loadingLocalizerFinished = new();
    
    void Awake()
    {
        if (instance == null) { instance = this; }
    }

    private void OnApplicationQuit() {
        if (!Application.isEditor) {
            System.Diagnostics.Process.GetCurrentProcess().Kill();
        }
    }
    
    float timeStart = 0;
    bool finishedLoadingBar = false;
    
    Dictionary<string, float> expectedPercents = new();
    Dictionary<string, float> currentPercents = new();
    static public void AddLoadingFinishedCallback(UnityAction uEvent){
        instance.loadingFinished.AddListener(uEvent); // AddListener() already takes care of uniqueness
    }

    static public void AddRemoteConfigFinishedCallback(UnityAction uEvent){
        instance.loadingRemoteConfigFinished.AddListener(uEvent); // AddListener() already takes care of uniqueness
    }
    public JObject appRemoteConfigsForLoadingCallback;
    static public void RunRemoteConfigFinishedCallbacks(JObject appRemoteConfigs){
        instance.appRemoteConfigsForLoadingCallback = appRemoteConfigs;
        instance.loadingRemoteConfigFinished.Invoke();
    }

    static public void AddLocalizerFinishedCallback(UnityAction uEvent){
        instance.loadingLocalizerFinished.AddListener(uEvent); // AddListener() already takes care of uniqueness
    }
    static public void RunLocalizerFinishedCallbacks(){
        instance.loadingLocalizerFinished.Invoke();
    }
    static public void AddExpectedLoadValue(float addPercent, string reason) { instance._AddExpectedLoadValue(addPercent, reason); }
    private void _AddExpectedLoadValue(float addExpectedPercent, string reason){
        expectedPercents[reason] = addExpectedPercent;
        currentPercents[reason] = 0;
        Debug.Log($"Add expected Load Progress {reason}: 0/{addExpectedPercent} Time: {Time.realtimeSinceStartup - timeStart}");
    }

    static public void AddLoadedValue(float addPercent, string reason, string secondaryReason = "", bool ignoreWarning = false) { instance._AddLoadedValue(addPercent, reason, secondaryReason, ignoreWarning); }
    private void _AddLoadedValue(float addPercent, string reason, string secondaryReason = "", bool ignoreWarning = false){
        if (timeStart == 0) timeStart = Time.realtimeSinceStartup;
        if (!currentPercents.ContainsKey(reason)){
            if (!ignoreWarning){
                Debug.LogError($"LoadingManager: cant find reason {reason}. Did you forget to call AddExpectedLoadValue() first "
                + "or did you want to put reason as secondaryReason instead and forgot to put the main reason in the AddLoadedValue() call?");
            }
            return;
        };
        float currentVal = currentPercents[reason];
        currentVal += addPercent;
        currentPercents[reason] = currentVal;

        float overallExpected = GetExpectedSum();
        float overallCurrent = GetCurrentSum();

        float fillPercent = overallCurrent / overallExpected;
        if (loadingImage != null) loadingImage.fillAmount = fillPercent;
        
        string secondaryReasonString = secondaryReason;
        if (secondaryReason != ""){
            secondaryReasonString = " - " + secondaryReasonString;
        }
        Debug.Log($"Load Progress {reason}{secondaryReasonString}: {currentVal}/{expectedPercents[reason]} Overall: {overallCurrent}/{overallExpected} Time: {Time.realtimeSinceStartup - timeStart}");
        if (currentPercents[reason] > expectedPercents[reason]){
            Debug.LogError($"{reason} has more progress than expected, please adjust the value given in AddExpectedLoadValue accordingly. Current Value: {currentPercents[reason]}/{expectedPercents[reason]}");
        }
        if (fillPercent > 0.99f && !finishedLoadingBar)
        {
            Debug.Log($"Loading Finished!");
            finishedLoadingBar = true;
            if (loadingImage != null && fadeImageWhenDone) loadingImage.DOFade(0, 0.5f);
            loadingFinished.Invoke();
        }
    }

    static public bool CheckLoadingFinished() { 
        return instance.finishedLoadingBar;
    }

    private float GetCurrentSum(){
        float overallCurrent = 0;
        foreach (var item in currentPercents)
        {
            overallCurrent += item.Value;
        }
        return overallCurrent;
    }
    private float GetExpectedSum(){
        float overallExpected = 0;
        foreach (var item in expectedPercents)
        {
            overallExpected += item.Value;
        }
        return overallExpected;
    }
    static public float GetCurrentLoadProgress(){
        float overallExpected = instance.GetExpectedSum();
        float overallCurrent = instance.GetCurrentSum();
        return overallCurrent / overallExpected;
    }
}