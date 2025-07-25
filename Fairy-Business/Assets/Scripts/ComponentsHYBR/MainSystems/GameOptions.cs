using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.Audio;
using Newtonsoft.Json.Linq;
using UnityEngine.UI;
using System;

public enum Difficulty
{
    easy,
    medium,
    hard,
    extra,
}

public class GameOptions : MonoBehaviour
{
    public static GameOptions instance { get; private set; }

    [NonSerialized] public float endTurnTimer = 0.2f;

    [NonSerialized] public bool automaticAiTurns = false;
    [NonSerialized] public bool automaticTurnEnding = false;
    [NonSerialized] public float aiStartTurnTime = 10f;
    [NonSerialized] public float aiNextActionTime = 10f;
    [NonSerialized] public float aiTimeMultiplier = 1.0f; // multiplies "aiStartTurnTime" and "aiNextActionTime"

    [NonSerialized] public float aiAtlantisMinigameStart = 3f;
    [NonSerialized] public float aiAtlantisMinigameInput = 2f;
    [NonSerialized] public float aiAtlantisMinigameFinalizeInput = 5f;


    public bool useLDGSCanner { set; get; } = false; // if enabled, LDG scanner is used instead of old prototype scanner

    public bool colorAccuracyOverrideEnabled  { set; get; } = false; //if set to true, colorAccuracyOverride will be used as the required color-accuracy in all missions.
    public float colorAccuracyOverride;  // if colorAccuracyOverrideEnabled is true this value will be used as the required color-accuracy in all missions.

    public bool cameraEnabled { set; get; } = false;
    public UnityEngine.UI.Toggle cameraEnabledToggle;

    public bool moveMonstersOnArtifact { set; get; } = false; //By default, do not move them

    public UnityEngine.UI.Toggle moveMonstersOnArtifactToggle;
    
    public AudioMixer audioMixer;

    public string currentLanguageCode = "";
    public Difficulty currentDifficulty = Difficulty.medium;

    public TMP_FontAsset buttonFont;
    public TMP_FontAsset textFont;
    public TMP_FontAsset[] availableFonts;

    //rework these values into a json file
    static float difficultyAccuracyEasy = 55;
    static float difficultyAccuracyMedium = 65;
    static float difficultyAccuracyHard = 70;
    static float difficultyAccuracyExtra = 70;

    private void Awake() {
        if (instance == null) { instance = this; }
        currentLanguageCode = "";
    }
    // Start is called before the first frame update
    void Start()
    {
    }

    public void OptionsInit(){
        cameraEnabled = AppUser.GetOptionOrDefault("cameraEnabled", false);
        cameraEnabledToggle.isOn = cameraEnabled;

        moveMonstersOnArtifact = AppUser.GetOptionOrDefault("moveMonstersOnArtifact", false);
        moveMonstersOnArtifactToggle.isOn = moveMonstersOnArtifact;

        UniqueNameHash.Get("SliderMusic").GetComponent<Slider>().value = AppUser.GetOptionOrDefault<float>("musicVolume", 0.8f);
        UniqueNameHash.Get("SliderSFX").GetComponent<Slider>().value = AppUser.GetOptionOrDefault<float>("sfxVolume", 0.8f);
        UniqueNameHash.Get("SliderAiSpeed").GetComponent<Slider>().value = AppUser.GetOptionOrDefault<float>("aiReactionTime", 0.0f);
    }
    
    public static void PopulateLanguageDropDown(TMP_Dropdown dropdown, List<string> optionStrings)
    {
        dropdown.ClearOptions();
        List<TMP_Dropdown.OptionData> newOptions = new List<TMP_Dropdown.OptionData>();

        for(int i = 0; i < optionStrings.Count; i++)//Populate new Options
        {
            string newEntry = Localizer.instance.GetNativeLanguageName(optionStrings[i]);
            string flagName = "flag" + optionStrings[i];
            Sprite flagSprite = Resources.Load<Sprite>("I18n/" + flagName);
            newOptions.Add(new TMP_Dropdown.OptionData(newEntry, flagSprite, new Color()));
        }

        dropdown.ClearOptions();//Clear old options
        dropdown.AddOptions(newOptions);//Add new options
    }

    public void RerenderTexts(){
        GameObject ldGO = UniqueNameHash.Get("LanguageDropdown").gameObject;
        if (!ldGO) return;
        TMP_Dropdown languageDropdown = ldGO.GetComponent<TMP_Dropdown>();
        languageDropdown.onValueChanged.RemoveAllListeners();
        int oldValue = languageDropdown.value;
        List<string> languageList = Localizer.instance.GetEnabledLanguagesList();
        PopulateLanguageDropDown(languageDropdown, languageList);
        // set current selected dialog option to same as before
        languageDropdown.value = languageList.FindIndex(listItem => listItem == currentLanguageCode);
        languageDropdown.onValueChanged.AddListener(delegate { 
            currentLanguageCode = languageList[languageDropdown.value];
            Localizer.instance.SetLanguage(currentLanguageCode);
            AppUser.SaveOption("currentLanguageCode", currentLanguageCode);
        });
        if (languageList.Count > 3)
        {
            Debug.LogError("WARNING with more than 3 languages you need to use the 'ADVANCEDLanguageDropdown' gameobject instead! ..."
                + "Something is wrong with it when having less than 4 elements, so i replaced it with a simpler one. Also the following lines need to be commented in!");
        }
        // The following lines are needed for more than 3 languages!
        //var chooseLangLabel = languageDropdown.transform.Find("Template").Find("Label").GetComponent<TMP_Text>();
        //chooseLangLabel.text = Localizer.instance.TranslateToSpecificLanguage("chooseLanguage", currentLanguageCode);
    }

    public void UpdateCameraEnabled()
    {
        AppUser.SaveOption("cameraEnabled", cameraEnabled);
    }

    public void UpdateMoveMonstersOnArtifact()
    {
        AppUser.SaveOption("moveMonstersOnArtifact", moveMonstersOnArtifact);
    }

    public void SetDifficulty(int difficulty){
        currentDifficulty = (Difficulty)difficulty;
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

    public void SetAiReactionTime(System.Single value)
    {
        aiTimeMultiplier = 1.0f - value;
        // slider to min == no automatic turns ^^
        automaticAiTurns = (value > 0.001f);
        automaticTurnEnding = (value >= 1.0f);
        AppUser.SaveOption("aiReactionTime", value);
    }

    public Difficulty GetCurrentDifficulty(){
        return currentDifficulty;
    }
    public float GetRequiredAccuracyByDifficulty()
    {
        float requiredAccuracy = 65;
        switch (currentDifficulty)
        {
            case Difficulty.easy: requiredAccuracy = difficultyAccuracyEasy; break;
            case Difficulty.medium: requiredAccuracy = difficultyAccuracyMedium; break;
            case Difficulty.hard: requiredAccuracy = difficultyAccuracyHard; break;
            case Difficulty.extra: requiredAccuracy = difficultyAccuracyExtra; break;
        }
        if (colorAccuracyOverrideEnabled)
        {
            requiredAccuracy = colorAccuracyOverride;
        }
        return requiredAccuracy;
    }
}
