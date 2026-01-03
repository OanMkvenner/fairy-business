using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.Audio;
using Newtonsoft.Json.Linq;
using UnityEngine.UI;
using System;

public class GameOptions : MonoBehaviour
{
    public static GameOptions instance { get; private set; }

    public bool moveMonstersOnArtifact { set; get; } = false; //By default, do not move them

    public AudioMixer audioMixer;

    private void Awake() {
        if (instance == null) { instance = this; }
    }
    // Start is called before the first frame update
    void Start()
    {
    }

    public void OptionsInit(){
        if (UniqueNameHash.HasKey("SliderMusic")){
            UniqueNameHash.Get("SliderMusic").GetComponent<Slider>().onValueChanged.AddListener(SetAudioMusic);
            UniqueNameHash.Get("SliderMusic").GetComponent<Slider>().value = AppUser.GetOptionOrDefault<float>("musicVolume", 0.8f);
        }
        if (UniqueNameHash.HasKey("SliderSFX")){
            UniqueNameHash.Get("SliderSFX").GetComponent<Slider>().onValueChanged.AddListener(SetAudioSFX);
            UniqueNameHash.Get("SliderSFX").GetComponent<Slider>().value = AppUser.GetOptionOrDefault<float>("sfxVolume", 0.8f);
        }
    }

    public void SetAudioSFX(System.Single value)
    {
        audioMixer.SetFloat("SFXVol", Mathf.Log(value) * 20);
        AppUser.SaveOption("sfxVolume", value);
    }

    public void SetAudioMusic(System.Single value)
    {
        audioMixer.SetFloat("MusicVol", Mathf.Log(value) * 20);
        AppUser.SaveOption("musicVolume", value);
    }
}
