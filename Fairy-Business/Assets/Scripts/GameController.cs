using UnityEngine;
using System.Collections.Generic;
using System.Threading.Tasks;
using System;
using DG.Tweening;

public class GameController : MonoBehaviour
{

    public static GameController instance { get; private set; }

    private void Awake() {
        if (instance == null) { instance = this; }
    }

    private void Start(){
        // keep screens online
        Screen.sleepTimeout = SleepTimeout.NeverSleep;

        Invoke("LateStart", 0.0f);
    }
    
    /*
    public async void LateStart()
    {
        //var task = GameLoader.instance.StartLoading();
        //await task;

        // just start scanning automatically for prototyping
        //GameLoader.finishedLoading.AddListener(delegate{
        //    CameraOpencvLib.instance.StartScanning();
        //});

        //GetComponent<GameSession>().NewRound();
    }
    */
    public void SwitchLanguage(){
        if (Localizer.instance.GetCurrentlySetLanguage() == "fr") {
            Localizer.instance.SetLanguage("en");
        } else {
            Localizer.instance.SetLanguage("fr");
        }
    }
}