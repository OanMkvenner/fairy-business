using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json.Linq;
using System.IO;
using System.Linq;
using TMPro;
using Unity.Mathematics;
using DG.Tweening;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using UnityEngine.Events;

public class LanguageSetting
{
    public string languageCode = "en";
    public string unitySystemLanguage = "English";
    public string translatedLanguageName = "English";
    public bool enabled = false;
    public string buttonFont = "BDCartoonShout";
    public string textFieldFont = "Arvo-Bold";
}

public class Localizer : MonoBehaviour
{
    public static Localizer instance { get; private set; }

    private List<LocalizedText> localizableStrings;
    private List<LocalizedGameobject> localizableGameObjects;
    private JObject locJson;
    private JObject availableLocalizations;
    private string currentlySetLanguageInternal;
    
    [Tooltip("Reference all available TMP fonts in here, otherwise they wont be included in a build!")]
    public TMP_FontAsset[] availableFonts;
    public GameObject emergencyMessageField0;
    public GameObject emergencyMessageField1;
    [Tooltip("This List should reference all objects on the initial Main Screen that should not show before being translated!")]
    public List<GameObject> initialStringElementsToPopup;

    bool mainScreenElementsInitialized = false;
    
    [HideInInspector] public List<LanguageSetting> languageSettings = new List<LanguageSetting>();
    [HideInInspector] public string currentLanguageCode = "";
    public TMP_FontAsset buttonFont;
    public TMP_FontAsset textFont;

    private void Awake() {
        instance = this;
    }
    async void Start() {
        LoadingManager.AddRemoteConfigFinishedCallback(ApplyAppConfigsAfterLoad);
        await AsyncInitialization();
    }
    bool localizerInitialized = false;
    async UniTask AsyncInitialization(){
        LoadingManager.AddExpectedLoadValue(0.5f, "InitLocalizer");
        await UniTask.Yield();
        // get all Localizations
        this.localizableStrings = Utilities.GetSceneObjectsNonGeneric<LocalizedText>();
        this.localizableGameObjects = Utilities.GetSceneObjectsNonGeneric<LocalizedGameobject>();
        // some setup-checks
        foreach (var item in this.localizableStrings)
        {
            if (item.localizationString == "textNotSetYet") {
                Debug.LogError("found a 'textNotSetYet' translation field! this is probably a mistake. It was found on GameObject: " + item.name);
            }
        }
        // init rest
        InitializePopupsAndMessages();
        localizerInitialized = true;
        // check if a config has been loaded already while initializing. If yes, apply it
        if (lastAppConfigs != null)
        {
            await ApplyAppConfigs(lastAppConfigs);
        }
    }

    public string GetCurrentlySetLanguage(){
        return currentlySetLanguageInternal;
    }
    
    void InitializePopupsAndMessages()
    {
        availableLocalizations = new JObject();

        foreach (var item in initialStringElementsToPopup)
        {
            if (item.GetComponent<TweenAnimator>()) {
                item.GetComponent<TweenAnimator>().HideElementOverride();
            } else {
                Debug.LogError("Localizer expects TweenAnimator on all initialStringElementsToPopup");
            }
        }
        
        if (emergencyMessageField1) emergencyMessageField1.SetActive(false);
        if (emergencyMessageField0) emergencyMessageField0.SetActive(false);
        //GameController.popupManager.PopupManagerInitialize();
    }


    public async void ApplyAppConfigsAfterLoad(){
        await ApplyAppConfigs(LoadingManager.instance.appRemoteConfigsForLoadingCallback);
    }

    JObject lastAppConfigs = null;
    public async UniTask ApplyAppConfigs(JObject appRemoteConfigs){
        LoadingManager.AddLoadedValue(0.0f, "InitLocalizer", "ApplyAppConfigs");
        await UniTask.Yield();
        lastAppConfigs = appRemoteConfigs;
        if (localizerInitialized == true)
        {
            ClearLoadedLocalizations();
            CheckLanguageSettings(appRemoteConfigs);
            SetInitialLanguage();
            LoadingManager.AddLoadedValue(0.1f, "InitLocalizer", "SetInitialLanguage");
        await UniTask.Yield();
            RegisterAllLocalizations(appRemoteConfigs);
            LoadingManager.AddLoadedValue(0.1f, "InitLocalizer", "RegisterAllLocalizations");
        await UniTask.Yield();
            SetLanguage(currentLanguageCode);
            LoadingManager.AddLoadedValue(0.3f, "InitLocalizer", "SetLanguage");
        await UniTask.Yield();
            InitializeStringElementsPoppingUp();
            LoadingManager.AddLoadedValue(0.1f, "InitializeRemoteConfig", "ApplyAppConfigs finished", true); // only has effect when InitializeRemoteConfig has started
        }
    }

    public void InitializeStringElementsPoppingUp(){
        if (!mainScreenElementsInitialized)
        {
            mainScreenElementsInitialized = true;
            
            int i = 0;
            foreach (var item in initialStringElementsToPopup)
            {
                item.GetComponent<TweenAnimator>().ShowElementOverride(TweenAnimator.TweenMode.DefaultShowByScalingIn);
                i++;
            }
        }
    }

    public List<string> GetEnabledLanguagesList(){
        List<string> languageList = new List<string>();
        foreach (var item in languageSettings)
        {
            if (item.enabled)
            {
                languageList.Add(item.languageCode);
            }
        }
        return languageList;
    }

    public void SetInitialLanguage(){
        string unitySystemLanguage = Application.systemLanguage.ToString();
        string savedLanguage = AppUser.GetOptionOrDefault("currentLanguageCode", "");
        string initialLanguageCode = "";
        // use saved language
        if (savedLanguage != ""){
            LanguageSetting savedLanguageSetting = GetSettingByLanguageCode(savedLanguage);
            if (savedLanguageSetting != null)
            {
                if (savedLanguageSetting.enabled)
                {
                    initialLanguageCode = savedLanguage;
                }
            }
        }
        // fallback to systemLanguage
        if (initialLanguageCode == "")
        {
            LanguageSetting initialLanguageSetting = GetSettingByUnitySystemLanguage(unitySystemLanguage);
            if (initialLanguageSetting != null)
            {
                if (initialLanguageSetting.enabled)
                {
                    initialLanguageCode = initialLanguageSetting.languageCode;
                }
            }
        }
        // fallback to english
        if (initialLanguageCode == "")
        {
            initialLanguageCode = "en";
        }

        currentLanguageCode = initialLanguageCode;
    }

    public void ClearLoadedLocalizations()
    {
        availableLocalizations = new JObject();
    }


    public void CheckLanguageSettings(JObject appRemoteConfigs)
    {
        // find ALL language settings within
        List<LanguageSetting> settingsList = new List<LanguageSetting>();
        var languageSettingsJObj = Utilities.GetFromRemoteConfigs(appRemoteConfigs, "languages");
        foreach (KeyValuePair<string, JToken> keyValuePair in languageSettingsJObj.Value<JObject>()){
            JToken settingsJObj = keyValuePair.Value;
            LanguageSetting setting = new LanguageSetting();
            setting.languageCode = (string)settingsJObj["languageCode"];
            setting.unitySystemLanguage = (string)settingsJObj["unitySystemLanguage"];
            setting.translatedLanguageName = (string)settingsJObj["translatedLanguageName"];
            setting.enabled = "yes".Equals((string)settingsJObj["enabled"]);
            setting.buttonFont = (string)settingsJObj["buttonFont"];
            setting.textFieldFont = (string)settingsJObj["textFieldFont"];
            settingsList.Add(setting);
        }

        languageSettings = settingsList;
    }
    public void RegisterAllLocalizations(JObject appConfigs)
    {
        foreach (var langSetting in languageSettings)
        {
            string languageString = "translation_" + langSetting.languageCode;
            if (!appConfigs.ContainsKey(languageString)) {
                Debug.LogError($"RemoteConfig: no rc file with content '{languageString}' was found. Did you forget to push it to production/development?");
                continue;
            }

            JObject languageTranslations = Utilities.GetFromRemoteConfigs(appConfigs, languageString).Value<JObject>();
            availableLocalizations[langSetting.languageCode] = languageTranslations;
        }
    }

    public LanguageSetting GetSettingByLanguageCode(string languageCode){
        return languageSettings.Find(setting => setting.languageCode == languageCode);
    }
    public LanguageSetting GetSettingByUnitySystemLanguage(string unitySystemLanguage){
        return languageSettings.Find(setting => setting.unitySystemLanguage == unitySystemLanguage);
    }
    public TMP_FontAsset GetFontForButtonsByLanguageCode(string languageCode){
        var settings = GetSettingByLanguageCode(languageCode);
        foreach (var font in availableFonts)
        {
            if (font == null) {
                Debug.LogError("found null-font in available fonts List, ignoring");
                continue;
            }
            if (font.name == settings.buttonFont)
            {
                return font;
            }
        }
        Debug.LogError($"Buttonfont of name {settings.buttonFont} not found in availableFonts");
        return null;
    }
    public TMP_FontAsset GetFontForTextByLanguageCode(string languageCode){
        var settings = GetSettingByLanguageCode(languageCode);
        foreach (var font in availableFonts)
        {
            if (font == null) {
                Debug.LogError("found null-font in available fonts List, ignoring");
                continue;
            }
            if (font.name == settings.textFieldFont)
            {
                return font;
            }
        }
        Debug.LogError($"Textfont of name {settings.textFieldFont} not found in availableFonts");
        return null;
    }
    public bool SetLanguage(string languageCode)
    {
        currentlySetLanguageInternal = languageCode;

        locJson = null;

        var settings = GetSettingByLanguageCode(languageCode);
        var newButtonFont = GetFontForButtonsByLanguageCode(languageCode);
        if (newButtonFont != null) {
            buttonFont.fallbackFontAssetTable.Clear();
            buttonFont.fallbackFontAssetTable.Add(newButtonFont);
            buttonFont.ReadFontAssetDefinition();
        }

        var newTextFont = GetFontForTextByLanguageCode(languageCode);
        if (newTextFont != null) {
            textFont.fallbackFontAssetTable.Clear();
            textFont.fallbackFontAssetTable.Add(newTextFont);
            textFont.ReadFontAssetDefinition();
        }

        //Look for this loc and set locJson
        if (!availableLocalizations.ContainsKey(languageCode)) {
            Debug.LogError($"RemoteConfig: no rc file with content '{languageCode}' was found. Did you forget to push it to production/development?");
            return false;
        }
        locJson = availableLocalizations[languageCode].Value<JObject>();
        //availableLocalizations.ForEach(locJsonI =>
        //{
        //    Debug.Log(locJsonI.ToString());
        //    if (((string)(locJsonI["languageCode"])).Equals(languageCode.ToString()))
        //    {
        //        locJson = locJsonI;
        //    }
        //});

        if (locJson == null)
        {
            return false;
        }

        RerenderAllLocalizables();
        return true;
    }

    private void RerenderAllLocalizables()
    {
        RerenderTexts();
        RerenderGameObjects();
        RerenderEmergencyMessages();
        RerenderRest();
        LoadingManager.RunLocalizerFinishedCallbacks(); //indirectly calls GameUpdater.CheckVersion(); if Gameupdater is present and registered itself to LoadingManager
    }
    private void RerenderEmergencyMessages()
    {
        if (emergencyMessageField0 != null) {
            if (emergencyMessageField0.transform.Find("Text (TMP)").GetComponent<TMP_Text>().text == "")
            {
                emergencyMessageField0.SetActive(false);
            } else {
                emergencyMessageField0.SetActive(true);
            };
        }
        if (emergencyMessageField1 != null) {
            if (emergencyMessageField1.transform.Find("Text (TMP)").GetComponent<TMP_Text>().text == "")
            {
                emergencyMessageField1.SetActive(false);
            } else {
                emergencyMessageField1.SetActive(true);
            };
        }
    }
    private void RerenderTexts()
    {
        RerenderSpecificTexts(localizableStrings);
    }
    public void RerenderSpecificTexts(List<LocalizedText> localizableTextObjects){
        foreach (LocalizedText gameObject in localizableTextObjects)
        {
            if (gameObject.localizationString == "") continue;

            TMPro.TMP_Text gameObjectText = gameObject.GetComponent<TMPro.TMP_Text>();
            if (locJson.ContainsKey(gameObject.localizationString))
            {
                gameObjectText.SetText(""); // used to force update of font even if text is the same
                gameObjectText.SetText((string)locJson[gameObject.localizationString]);
            }
        }
    }
    private void RerenderGameObjects()
    {
        foreach (LocalizedGameobject gameObject in localizableGameObjects)
        {
            bool setAtLeastOneToTrue = false;
            foreach (Transform item in gameObject.transform)
            {
                if (item.name == currentlySetLanguageInternal) {
                    item.gameObject.SetActive(true);
                    setAtLeastOneToTrue = true;
                }
                else
                {
                    item.gameObject.SetActive(false);
                }
            }
            // if no special gameobject was set for this language, use english instead.
            if (!setAtLeastOneToTrue)
            {
                Transform englishObj = gameObject.transform.Find("en");
                if (englishObj)
                {
                    englishObj.gameObject.SetActive(true);
                }
            }
        }
    }
    
    
    UnityEvent rerenderRest = new();
    // WARNING! Callbacks are only ever ADDED and never removed. This is because we only add these in "Awake"s of systems to setup refernces and not at runtime.
    static public void AddLoadingFinishedCallback(UnityAction uEvent){
        instance.rerenderRest.RemoveListener(uEvent); //done as a precaution, to avoid adding the same listener twice
        instance.rerenderRest.AddListener(uEvent);
    }
    private void RerenderRest()
    {
        // add things here that cant be handled by localizables "Loc" or "LocObj" tag
        rerenderRest.Invoke();
        //GameController.gameOptions.RerenderTexts();
        //GameController.instance.GenerateCharacterButtons();
    }

    public string Translate(string str)
    {
        if (locJson != null) {
            if (locJson.ContainsKey(str))
            {
                return (string)locJson[str];
            }
            else return str;
        }
        else return str;
    }

    public string TranslateToSpecificLanguage(string str, string languageCode)
    {
        if (availableLocalizations[languageCode].Value<JObject>().ContainsKey(str))
        {
            return (string)availableLocalizations[languageCode][str];
        }
        return str;
    }

    public string GetNativeLanguageName(string languageCode)
    {
        return GetSettingByLanguageCode(languageCode).translatedLanguageName;
    }

    public string Translate(string str, string[] replacements)
    {
        string strL = Translate(str);

        for (int i = 0; i < replacements.Length; i++)
        {
            strL = strL.Replace("{" + (i + 1) + "}", replacements[i]);
        }

        return strL;
    }
}
