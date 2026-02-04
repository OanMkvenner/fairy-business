using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using Unity.Mathematics;
using Newtonsoft.Json.Serialization;
using System;
using System.Reflection;
using Cysharp.Threading.Tasks;
using NUnit.Framework;

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

    private bool CheckProgressObjectExists(string prefString)
    {
        // use cache if possible
        if (cachedProgressObjects.ContainsKey(prefString))
        {
            return true;
        }
        return PlayerPrefs.HasKey(prefString);
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

    private void SaveProgressObject(string prefString, JObject progressObject){
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

    public static T ParseJTokenString<T>(string jTokenString, T defaultValue)
    {
        if (jTokenString != ""){
            var jToken = JToken.Parse(jTokenString);
            try{
                return jToken.ToType<T>();
            } catch(Exception e){
                Debug.LogError(e.Message);
            }
        }
        return defaultValue;
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
    static public bool CheckSavedDataExists(string saveCategory, string dataName){
        JObject dataObject = AppUser.instance.GetProgressObject(saveCategory);
        return dataObject.ContainsKey(dataName);
    }
    // CAREFUL with float/integer types! This function takes the required type from the defaultValue 
    // unless specified explicitly. A default value of 0 is always assumed integer without warning!
    // If you need float, set it to 0.0f or state the type explicitly via GetOptionOrDefault<float>()
    static public T GetSavedDataOrDefault<T>(string saveCategory, string dataName, T defaultValue, bool compressed = false)
    {
        JObject dataObject = AppUser.instance.GetProgressObject(saveCategory);
        return dataObject.GetKeyOrDefault<T>(dataName, defaultValue, compressed: compressed);
    }
    static public object TryGetSavedDataOfDynamicType(string saveCategory, string dataName, Type type, bool compressed = false)
    {
        JObject dataObject = AppUser.instance.GetProgressObject(saveCategory);
        return dataObject.TryGetKeyByDynamicType(dataName, type, compressed: compressed);
    }
    static public string ConvertObjectToCompressedString<T>(T value){
        return LZStringCSharp.LZString.CompressToEncodedURIComponent(ConvertToJToken(value).ToString());
    }
    static public T ConvertCompressedStringToObject<T>(string valueString){
        JToken foundObj = JToken.Parse(LZStringCSharp.LZString.DecompressFromEncodedURIComponent(valueString));
        return foundObj.ToType<T>();
    }
    // same as above applies. But since input values are often taken from variables, the danger of mis-typing is smaller.
    static public void SaveData<T>(string saveCategory, string dataName, T value, bool compressed = false, bool debugPrint = false)
    {
        if (compressed) {
            SaveData(saveCategory, dataName, ConvertObjectToCompressedString(value), compressed: false);
            return;
        }
        JObject dataObject = AppUser.instance.GetProgressObject(saveCategory);
        dataObject[dataName] = ConvertToJToken(value);
        if (debugPrint) Debug.LogError(dataObject[dataName]);
        AppUser.instance.SaveProgressObject(saveCategory, dataObject);
    }
    static public void DeleteSavedData(string saveCategory, string dataName)
    {
        JObject dataObject = AppUser.instance.GetProgressObject(saveCategory);
        dataObject.Remove(dataName);
        AppUser.instance.SaveProgressObject(saveCategory, dataObject);
    }
    // note: using compression here means compression on single-element level of the array. If the whole array is compressed, this function does not work! 
    // (could implement a different one for this, but the performance overhead probably makes the "AddToSavedArray" functions intended use kinda useless)
    static public void AddToSavedArray<T>(string saveCategory, string arrayName, T value, bool compressed = false){
        JObject progressObject = AppUser.instance.GetProgressObject(saveCategory);
        JArray array = AppUser.instance.GetContentArraySafely(progressObject, arrayName);
        // array is a reference, so progressObject gets updated when adding items
        JToken valueToAdd = ConvertToJToken(value);
        if (compressed) valueToAdd = JToken.Parse(LZStringCSharp.LZString.CompressToEncodedURIComponent(valueToAdd.ToString()));
        array.Add(valueToAdd);
        // save the updated progressObject
        AppUser.instance.SaveProgressObject(saveCategory, progressObject);
    }
    // deprecated - can be done via GetSavedDataOrDefault
    //static public List<T> GetSavedDataArray<T>(string saveCategory, string arrayName){
    //    JObject optionsObject = AppUser.instance.GetProgressObject(saveCategory);
    //    JArray jArray = AppUser.instance.GetContentArraySafely(optionsObject, arrayName);
    //    return jArray.ToObject<List<T>>();
    //}


    static public bool CheckOptionExists(string optionName){
        return CheckSavedDataExists("Options", optionName);
    }
    // CAREFUL with float/integer types! This function takes the required type from the defaultValue 
    // unless specified explicitly. A default value of 0 is always assumed integer without warning!
    // If you need float, set it to 0.0f or state the type explicitly via GetOptionOrDefault<float>()
    static public T GetOptionOrDefault<T>(string optionName, T defaultValue)
    {
        return GetSavedDataOrDefault("Options", optionName, defaultValue);
    }
    static public object TryGetOptionOfDynamicType(string optionName, Type type)
    {
        return TryGetSavedDataOfDynamicType("Options", optionName, type);
    }
    // same as above applies. But since input values are often taken from variables, the danger of mis-typing is smaller.
    static public void SaveOption<T>(string optionName, T value, bool debugPrint = false)
    {
        SaveData("Options", optionName, value, debugPrint);
    }
    static public void DeleteOption(string optionName)
    {
        DeleteSavedData("Options", optionName);
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

        AppUser.instance.SaveProgressObject("Progress_Tutorial", progressObject);

        //As for testing, return true to always show the tutorial...
        return shouldAddAndShow;
    }

    static public void ResetTutorialProgress()
    {
        DeleteSavedData("Progress_Tutorial", "tutorialStrings");
    }

    static public void ShowMeTheProgress()
    {
        Debug.Log("progressTutorialObjJson: " + AppUser.instance.GetProgressObject("Progress_Tutorial").ToString());
        Debug.Log("progressSoloHighscoreObjJson: " + AppUser.instance.GetProgressObject("Progress_Solo_Highscore").ToString());
    }
}
