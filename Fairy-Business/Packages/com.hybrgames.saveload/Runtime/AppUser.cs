using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using Unity.Mathematics;
using Newtonsoft.Json.Serialization;
using System;
using System.Reflection;
using Cysharp.Threading.Tasks;

public class EnounteredItem
{
    public string itemName = "";
    public int encounteredCount = 0;
}

public class AppUser : MonoBehaviour
{
    public static AppUser instance { get; private set; }

    Dictionary<string, JObject> cachedProgressObjects = new Dictionary<string, JObject>();

    void Awake() {
        if (instance == null) { instance = this; }
        cachedProgressObjects = new Dictionary<string, JObject>();
    }

    async void Start() {
        LoadingManager.AddExpectedLoadValue(0.2f, "InitializeAppUser");
        await UniTask.Yield();
        var _ = InitializeSaveableOptions();
    }

    public async UniTask InitializeSaveableOptions()
    {
        await UniTask.Yield();
        SaveableOption.InitAll();
        LoadingManager.AddLoadedValue(0.2f, "InitializeAppUser", "InitializedSaveableOptions");
    }


    public AppUser()
    {
    }

    private JObject GetProgressObject(string prefString)
    {
        // use cache if possible
        if (cachedProgressObjects.ContainsKey(prefString))
        {
            return cachedProgressObjects[prefString];
        }
        // otherwise get from saved data
        string progressObjJson = PlayerPrefs.GetString(prefString, "{}");
        JObject progressObj = JObject.Parse(progressObjJson);
        cachedProgressObjects[prefString] = progressObj;
        return progressObj;
    }

    private void SaveData(string prefString, JObject progressObject){
        // update cache
        cachedProgressObjects[prefString] = progressObject;
        // update PlayerPrefs
        PlayerPrefs.SetString(prefString, progressObject.ToString());
    }
    
    private JArray GetContentArraySafely(JObject progressObject, string arrayName)
    {
        if (progressObject.ContainsKey(arrayName) == false)
        {
            progressObject.Add(new JProperty(arrayName, new JArray()));
        }
        return (JArray)progressObject.GetValue(arrayName);
    }

    // required to serialize things like int2 or other vectors with sizzled properties (or any properties that cant 
    // be "get" and "set", because it wont be possible to deserialize them anyway)
    internal sealed class JsonStructFieldsContractResolver : DefaultContractResolver {
        private static Predicate<object> NoShouldNotSerialize = (instance) => { return false; };
        protected override JsonProperty CreateProperty(MemberInfo member, MemberSerialization memberSerialization) {
            var property = base.CreateProperty(member, memberSerialization);
            if (member.MemberType != MemberTypes.Field && !member.IsDefined(typeof(JsonPropertyAttribute), false))
                property.ShouldSerialize = NoShouldNotSerialize;
            return property;
        }
    }

    static public JToken ConvertToJToken<T>(T value){
        if (value == null){
            return JValue.CreateNull();
        } else {
            JsonSerializer defaultSerializer = new();
            defaultSerializer.ReferenceLoopHandling = ReferenceLoopHandling.Ignore;
            defaultSerializer.ContractResolver = new JsonStructFieldsContractResolver();
            return JToken.FromObject(value, defaultSerializer);
        }
    }

    // ************************************** Static Public Functions *************************************
    
    static public void RemoveProgressContent(string progressName, string contentName){
        JObject progressObject = AppUser.instance.GetProgressObject(progressName);
        progressObject.Remove(contentName);
        AppUser.instance.SaveData(progressName, progressObject);
    }

    static public T GetProgressOrDefault<T>(string progressName, string contentName, T defaultValue){
        JObject optionsObject = AppUser.instance.GetProgressObject(progressName);
        return optionsObject.GetKeyOrDefault<T>(contentName, defaultValue);
    }
    static public void SaveProgress<T>(string progressName, string contentName, T value){
        JObject optionsObject = AppUser.instance.GetProgressObject(progressName);
        optionsObject[contentName] = ConvertToJToken(value);
        AppUser.instance.SaveData(progressName, optionsObject);
    }

    static public JArray GetProgressArray(string progressName, string arrayName){
        JObject optionsObject = AppUser.instance.GetProgressObject(progressName);
        return AppUser.instance.GetContentArraySafely(optionsObject, arrayName);
    }
    static public void AddToProgressArray<T>(string progressName, string arrayName, T value){
        JObject progressObject = AppUser.instance.GetProgressObject(progressName);
        JArray array = AppUser.instance.GetContentArraySafely(progressObject, arrayName);
        // array is a reference, so progressObject gets updated when adding items
        array.Add(ConvertToJToken(value));
        // save the updated progressObject
        AppUser.instance.SaveData(progressName, progressObject);
    }

    // CAREFUL with float/integer types! This function takes the required type from the defaultValue 
    // unless specified explicitly. A default value of 0 is always assumed integer without warning!
    // If you need float, set it to 0.0f or state the type explicitly via GetOptionOrDefault<float>()
    static public T GetOptionOrDefault<T>(string optionName, T defaultValue)
    {
        JObject optionsObject = AppUser.instance.GetProgressObject("Options");
        return optionsObject.GetKeyOrDefault<T>(optionName, defaultValue);
    }

    static public object TryGetOptionOfDynamicType(string optionName, Type type)
    {
        JObject optionsObject = AppUser.instance.GetProgressObject("Options");
        return optionsObject.TryGetKeyByDynamicType(optionName, type);
    }
    // same as above applies. But since input values are often taken from variables, the danger of mis-typing is smaller.
    static public void SaveOption<T>(string optionName, T value, bool debugPrint = false)
    {
        JObject optionsObject = AppUser.instance.GetProgressObject("Options");
        optionsObject[optionName] = ConvertToJToken(value);
        if (debugPrint) Debug.LogError(optionsObject[optionName]);
        AppUser.instance.SaveData("Options", optionsObject);
    }
    static public void DeleteOption(string optionName)
    {
        JObject optionsObject = AppUser.instance.GetProgressObject("Options");
        optionsObject.Remove(optionName);
        AppUser.instance.SaveData("Options", optionsObject);
    }

    //Return true, if is supposed to show tutorial
    static public bool GetAndSetTutorialSeen(string stringId, int times = 1)
    {
        JObject progressObject = AppUser.instance.GetProgressObject("Progress_Tutorial");
        JArray tutorialStrings = AppUser.instance.GetContentArraySafely(progressObject, "tutorialStrings");

        int foundNumberOfTimes = 0;

        for (int i = 0; i < tutorialStrings.Count; i++)
        {
            if (tutorialStrings[i].ToString().Equals(stringId))
            {
                foundNumberOfTimes++;
            }
        }

        bool shouldAddAndShow = foundNumberOfTimes < times;

        if (shouldAddAndShow)
        {
            tutorialStrings.Add(new JValue(stringId));
        }

        progressObject["tutorialStrings"] = tutorialStrings;

        AppUser.instance.SaveData("Progress_Tutorial", progressObject);

        //As for testing, return true to always show the tutorial...
        return shouldAddAndShow;
    }

    static public void ResetTutorialProgress()
    {
        RemoveProgressContent("Progress_Tutorial", "tutorialStrings");
    }

    static public void StoreSoloHighscore(int numberOfRounds)
    {
        AddToProgressArray("Progress_Solo_Highscore", "scores", numberOfRounds);
    }
    
    static public List<int> GetSoloHighscores()
    {
        JArray scores = GetProgressArray("Progress_Solo_Highscore", "scores");
        return scores.ToObject<List<int>>();
    }

    static public void ShowMeTheProgress()
    {
        Debug.Log("progressTutorialObjJson: " + AppUser.instance.GetProgressObject("Progress_Tutorial").ToString());
        Debug.Log("progressSoloHighscoreObjJson: " + AppUser.instance.GetProgressObject("Progress_Solo_Highscore").ToString());
    }
}
