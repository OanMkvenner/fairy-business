using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using Unity.Mathematics;
using Newtonsoft.Json.Linq;
using Cysharp.Threading.Tasks;
using System.Linq;
using System;
using UnityEngine.InputSystem;




#if (UNITY_EDITOR)
using UnityEditor;
#endif

public class VersionNumber
{
    public int major;
    public int minor;
    public int patch;

    public static bool operator == (VersionNumber a, VersionNumber b){
        return a.major == b.major && a.minor == b.minor && a.patch == b.patch;
    }
    public static bool operator != (VersionNumber a, VersionNumber b){
        return a.major != b.major || a.minor != b.minor || a.patch != b.patch;
    }
    public override bool Equals (object obj)
    {
        VersionNumber other = obj as VersionNumber;
        if (other == null) return false;
        return this == other;
    }
    public override int GetHashCode()
    {
        return base.GetHashCode();
    }
    public static bool operator > (VersionNumber a, VersionNumber b){
        return a.major > b.major || a.major == b.major && a.minor > b.minor || a.major == b.major && a.minor == b.minor && a.patch > b.patch;
    }
    public static bool operator < (VersionNumber a, VersionNumber b){
        return a.major < b.major || a.major == b.major && a.minor < b.minor || a.major == b.major && a.minor == b.minor && a.patch < b.patch;
    }
}
public static class Utilities
{
    public static void Invoke(this MonoBehaviour mb, Action f, float delay)
    {
        mb.StartCoroutine(InvokeRoutine(f, delay));
    }

    private static IEnumerator InvokeRoutine(System.Action f, float delay)
    {
        yield return new WaitForSeconds(delay);
        f();
    }
    [HideInInspector] public static Dictionary<string, bool> inputPolling = new Dictionary<string, bool>();
    public static PlayerInput input = null;

    static public T TryGetData<T>(this Newtonsoft.Json.Linq.JToken jtoken, string effectStat, T defaultValue) {
        var value = jtoken[effectStat];
        if (value != null) return value.ToType<T>();
        else return defaultValue;
    }
    static private void CheckStartKeyPollingSystem(){
        if (input == null)
        {
            input = UnityEngine.Object.FindFirstObjectByType<PlayerInput>();
        }
    }
    static public bool PollKey(string name){
        CheckStartKeyPollingSystem();
        if (!inputPolling.ContainsKey(name))
        {
            inputPolling.Add(name, false);
            // Add callbacks for input actions. 
            // WARNING we need to make sure that we remove these again when this system is destroyed for some reason!
            input.actions[name].performed += (_) => {
                inputPolling[name] = true;
            };
        }
        bool triggered = inputPolling[name];
        if (triggered)
        {
            inputPolling[name] = false;
        }
        return triggered;
    }
    static public bool KeyPressed(string name){
        CheckStartKeyPollingSystem();
        return Utilities.input.actions[name].ReadValue<float>() > 0;
    }


    // helper function for WaitTimerUnique to allow calling without setting a specific uniqueCallerString.
    // uniqueCallerString is instead created automatically by the name of the calling Function and the Script-file it is called from
    public static async UniTask<bool> WaitTimerUnique(float waitingTime, bool useUnscaledTime = false, [System.Runtime.CompilerServices.CallerFilePathAttribute] string callingFilePath = "", [System.Runtime.CompilerServices.CallerMemberName] string callingFunctionName = "")
    {
        // prune the callingFilePath somewhat, to avoid leaking System-specific filepaths to the programm
        int cutIndex = callingFilePath.IndexOf("Assets\\");
        if (cutIndex == -1) cutIndex = callingFilePath.IndexOf("Assets");
        if (cutIndex != -1) callingFilePath = callingFilePath.Substring(cutIndex + 7);
        // use filepath and functionname to create string cache. This is used to identify Calls from the same function!
        string uniqueCallingMethodString = callingFilePath + "_" + callingFunctionName;
        return await WaitTimerUnique(uniqueCallingMethodString, waitingTime, useUnscaledTime);
    }
    static Dictionary<string, float> cachedTimers = new Dictionary<string, float>();
    public static async UniTask<bool> WaitTimerUnique(string uniqueCallerString, float waitingTime, bool useUnscaledTime = false)
    {
        // check 
        float finishedTime = Time.time;
        if (useUnscaledTime) finishedTime = Time.unscaledTime;
        finishedTime += waitingTime;
        bool thisFunctionIsAlreadyWaitingToRun = cachedTimers.ContainsKey(uniqueCallerString);
        // update timer for this functioncall
        cachedTimers[uniqueCallerString] = finishedTime;

        // this function is already waiting for a timer to expire, return "false" to notify the outside of this
        if (thisFunctionIsAlreadyWaitingToRun) return false;

        // if we continue, keep awaiting until the time is up! (this await can be continued further and further by updating the target time later)
        float currentTime = Time.time;
        if (useUnscaledTime) currentTime = Time.unscaledTime;
        while (cachedTimers[uniqueCallerString] > currentTime)
        {
            await UniTask.Yield();
            currentTime = Time.time;
            if (useUnscaledTime) currentTime = Time.unscaledTime;
        }
        // we are done waiting, remove the timer as its not needed anymore
        cachedTimers.Remove(uniqueCallerString);
        // await has finished successfully, return "true" to allow calling function to continue
        return true;
    }
    public static string GetCaller([System.Runtime.CompilerServices.CallerFilePathAttribute] string callingFilePath = "", [System.Runtime.CompilerServices.CallerMemberName] string callingFunctionName = "")
    {
        return callingFilePath + "_" + callingFunctionName;
    }
    


    public static VersionNumber ConvertVersionNumber(string versionString){
        if (versionString == null)
        {
            Debug.LogWarning("ConvertVersionNumber got null string");
            return new VersionNumber{major = 0, minor = 0, patch = 0};
        }
        string[] version = versionString.Split('.');
        VersionNumber versionNumber = new VersionNumber();
        versionNumber.major = int.Parse(version[0]);
        if (version.Length > 1)
            versionNumber.minor = int.Parse(version[1]); 
        else 
            versionNumber.minor = 0;
        if (version.Length > 2)
            versionNumber.patch = int.Parse(version[2]);
        else 
            versionNumber.patch = 0;
        return versionNumber;
    }
    public static VersionNumber GetGameVersion(){
        return ConvertVersionNumber(Application.version);
    }

    // this function gets ALL scene objects of this type! no matter if active or not. 
    // It MIGHT return some other ressources as well, but i hope i got all of those excluded with the flags...
    // AVOIDS adding prefabs
    public static List<T> GetSceneObjectsNonGeneric<T>() where T : MonoBehaviour
    {
        List<T> objectsInScene = new List<T>();

        foreach (T go in Resources.FindObjectsOfTypeAll<T>())
        {
            GameObject cGO = go.gameObject as GameObject;
            bool canAdd = cGO != null && !(go.hideFlags == HideFlags.NotEditable || go.hideFlags == HideFlags.HideAndDontSave || cGO.IsPrefab());
#if (UNITY_EDITOR)
            if (canAdd)
                if (EditorUtility.IsPersistent(cGO.transform.root.gameObject)) 
                    canAdd = false;
#endif
            if (canAdd)
                objectsInScene.Add(go);
        }

        return objectsInScene;
    } 

    public static void CopyArray<T>(T[,] srcArray, ref T[,] tgtArray){
        if (srcArray == null){
            tgtArray = null;
            return;
        }
        int assumedDim = srcArray.GetLength(0);
        tgtArray = new T[assumedDim, assumedDim];
        System.Array.Copy(srcArray, tgtArray, srcArray.Length);
    }
    public static void CopyArray<T>(T[] srcArray, ref T[] tgtArray){
        if (srcArray == null){
            tgtArray = null;
            return;
        }
        tgtArray = new T[srcArray.Count()];
        System.Array.Copy(srcArray, tgtArray, srcArray.Count());
    }
    public static void CopyList<T>(List<T> srcList, ref List<T> tgtList){
        if (srcList == null){
            tgtList = null;
            return;
        }
        tgtList = new List<T>(srcList);
    }
    public static List<T> CopyList<T>(List<T> srcList){
        if (srcList == null){
            return null;
        }
        return new List<T>(srcList);
    }
    public static void CopyListDeep<T>(List<T> srcList, ref List<T> tgtList){
        if (srcList == null){
            tgtList = null;
            return;
        }

        //tgtList = new List<T>(srcList);
        tgtList = new List<T>();
        for (int i = 0; i < srcList.Count; i++)
        {
            tgtList.Add((T)System.Activator.CreateInstance(typeof(T), srcList[i]));
        }
    }

    
    public static object ToDynamicType(this JToken jtoken, Type type){
        object value = jtoken.ToObject(type);// Value<T>();
        return value;
    }
    public static T ToType<T>(this JToken jtoken){
        T value = jtoken.ToObject<T>();// Value<T>();
        return value;
    }
    public static T GetKeyOrDefault<T>(this JObject jObject, string keyName, T defaultValue)
    {
        if (jObject.ContainsKey(keyName))
        {
            try{
                return jObject[keyName].ToType<T>();
            } catch(Exception e){
                Debug.LogError(e.Message);
            }
        }
        return defaultValue;
    }
    public static object TryGetKeyByDynamicType(this JObject jObject, string keyName, Type type)
    {
        if (jObject.ContainsKey(keyName))
        {
            try{
                return jObject[keyName].ToDynamicType(type);
            } catch(Exception e){
                Debug.LogError(e.Message);
            }
        }
        return null;
    }
    public static bool IsPrefab(this Transform This)
    {
        return This.gameObject.scene.buildIndex < 0;
    }
    public static bool IsPrefab(this GameObject This)
    {
        return This.scene.buildIndex < 0;
    }
    public static Transform FindOneDeep(Transform t, string name) {
        for (int i = 0; i < t.childCount; i++) {
            Transform child = t.GetChild(i);
            if (name == child.name)
            {
                return child;
            }
        }
        Debug.LogError("didnt find '" + name + "' inside transform '"+t.name+"'!");
        return null;
    }
    public static Transform FindFullDepth(Transform t, string name, bool reportFailedResult = true) {
        for (int i = 0; i < t.childCount; i++) {
            Transform child = t.GetChild(i);
            if (name == child.name)
            {
                return child;
            } else {
                var foundChild = FindFullDepth(child, name, false);
                if (foundChild != null) return foundChild;
            }
        }
        Debug.LogError("didnt find '" + name + "' inside transform '"+t.name+"' at any depth!");
        return null;
    }
    public static List<Transform> ChildTransformsSorted(Transform t) {
        List<Transform> sorted = new List<Transform>();
        for (int i = 0; i < t.childCount; i++) {
            Transform child = t.GetChild(i);
            sorted.Add( child );
        }
        return sorted;
    }
    public static List<Transform> ChildTransformsSortedFullDepth(Transform t) {
        List<Transform> sorted = new List<Transform>();
        for (int i = 0; i < t.childCount; i++) {
            Transform child = t.GetChild(i);
            sorted.Add( child );
            sorted.AddRange( ChildTransformsSorted(child) );
        }
        return sorted;
    }
    public static List<T> GetComponentsInChildrenIncludingDisabled<T>(Transform t) where T : MonoBehaviour
    {
        List<T> sorted = new List<T>();
        for (int i = 0; i < t.childCount; i++) {
            Transform child = t.GetChild(i);
            var component = child.GetComponent<T>();
            if (component != null)
            {
                sorted.Add( component );
            }
            sorted.AddRange( GetComponentsInChildrenIncludingDisabled<T>(child) );
        }
        return sorted;
    }
    // "Fisher–Yates shuffle"
    public static void ShuffleList<T>(List<T> list)
    {
        int n = list.Count;
        while (n > 1)
        {
            n--;
            int k = RandomizerStatic.RandomInt(n);
            T value = list[k];
            list[k] = list[n];
            list[n] = value;
        }
    }
    public static T GetRandomElementOfList<T>(List<T> list){
        int k = RandomizerStatic.RandomInt(list.Count - 1);
        return list[k];
    }

    public static JToken GetFromRemoteConfigs(JToken remoteConfig, string keyName){
        return GetFromRemoteConfigs((JObject)remoteConfig, keyName);
    }
    public static JToken GetFromRemoteConfigs(JObject remoteConfig, string keyName){
        if (remoteConfig.ContainsKey(keyName)){
            return remoteConfig.GetValue(keyName);
        } else {
            Debug.LogError($"{keyName} not found in RemoteConfig");
            return null;
        }
    }

    
    public static float3 EaseInSquared(float3 difference, float slowdown_range, float easing_speed)
    {
        var tgt_vec_length = math.length(difference);
        if (!(tgt_vec_length == 0))
        {
            var slowdownMult = tgt_vec_length / slowdown_range;
            if (slowdownMult > 1.0f)
                slowdownMult = 1.0f;
            var negated_distance = 1.0f - slowdownMult;

            //var tgt_vec = easing_speed * difference / tgt_vec_length;
            //return tgt_vec * (1.0f - (negated_distance * negated_distance));
            var movePercent = easing_speed / tgt_vec_length; 
            var finalMovePercent = movePercent * (1.0f - (negated_distance * negated_distance));
            finalMovePercent = Math.Clamp(finalMovePercent, 0.0f, 1.0f); // clamp to 1.0f of original difference
            var tgt_vec = finalMovePercent * difference;
            return tgt_vec; 
        }
        else
        {
            return difference;
        }
    }
    public static float2 EaseInSquared(float2 difference, float slowdown_range, float easing_speed)
    {
        var tgt_vec_length = math.length(difference);
        if (!(tgt_vec_length == 0))
        {
            var tgt_vec = easing_speed * difference / tgt_vec_length;
            tgt_vec_length /= slowdown_range;
            if (tgt_vec_length > 1.0f)
                tgt_vec_length = 1.0f;
            var negated_distance = 1.0f - tgt_vec_length;

            return tgt_vec * (1.0f - (negated_distance * negated_distance));
        }
        else
        {
            return difference;
        }
    }
    public static float EaseInSquared(float difference, float slowdown_range, float easing_speed)
    {
        var tgt_length = math.abs(difference);
        var direction_mult = 1.0f - 2.0f * (difference < 0.0f? 1: 0);
        tgt_length /= slowdown_range;
        if (tgt_length > 1.0f)
            tgt_length = 1.0f;
        var negated_distance = 1.0f - tgt_length;

        return direction_mult * easing_speed * (1.0f - (negated_distance * negated_distance));
    }

    public static T ParseEnum<T>(string parseString) where T : Enum {
        return (T)System.Enum.Parse( typeof(T), parseString ); 
    }
    
    static public Vector3 GetWorldPosFromScreenRayAtZDepth(Ray ray, float zDepth = 0f, Plane? overridePlane = null){
        Plane playingFieldPlane = new Plane(new Vector3(0,0,-1), new Vector3(0,0,zDepth));
        if (overridePlane != null){
            playingFieldPlane = overridePlane.Value;
        }
        Vector3 worldPos = new(0,0,zDepth);
        if(playingFieldPlane.Raycast(ray, out var enter))
        {
            worldPos = ray.GetPoint(enter);
        }
        return worldPos;
    }
    static public Vector3 GetWorldPosFromScreenPos(Vector2 screenPos){
        Ray ray = Camera.main.ScreenPointToRay(screenPos);
        Plane playingFieldPlane = new Plane(new Vector3(0,0,-1), new Vector3(0,0,0));
        Vector3 worldPos = new(0,0,0);
        if(playingFieldPlane.Raycast(ray, out var enter))
        {
            worldPos = ray.GetPoint(enter);
        }
        return worldPos;
    }
    static public List<T> FilterListByType<T>(List<T> list, Type contentType){
        List<T> filteredList = new();
        foreach (var item in list)
        {
            if (item.GetType() == contentType || item.GetType().IsSubclassOf(contentType)){
                filteredList.Add(item);
            }
        }
        return filteredList;
    }
}

/* // need to go into Localizer!
public class UiUtilities {
    //You can populate any dropdown with any enum with this method
    public static void PopulateDropDownWithEnum(TMP_Dropdown dropdown, System.Enum targetEnum)
    {
        dropdown.ClearOptions();
        System.Type enumType = targetEnum.GetType();//Type of enum(FormatPresetType in my example)
        List<TMP_Dropdown.OptionData> newOptions = new List<TMP_Dropdown.OptionData>();

        for(int i = 0; i < System.Enum.GetNames(enumType).Length; i++)//Populate new Options
        {
            string newEntry = GameController.localizer.Translate(System.Enum.GetName(enumType, i));
            newOptions.Add(new TMP_Dropdown.OptionData(newEntry));
        }

        dropdown.ClearOptions();//Clear old options
        dropdown.AddOptions(newOptions);//Add new options
    }
    public static void PopulateDropDownWithList(TMP_Dropdown dropdown, List<string> optionStrings)
    {
        dropdown.ClearOptions();
        List<TMP_Dropdown.OptionData> newOptions = new List<TMP_Dropdown.OptionData>();

        for(int i = 0; i < optionStrings.Count; i++)//Populate new Options
        {
            string newEntry = GameController.localizer.Translate(optionStrings[i]);
            newOptions.Add(new TMP_Dropdown.OptionData(newEntry));
        }

        dropdown.ClearOptions();//Clear old options
        dropdown.AddOptions(newOptions);//Add new options
    }
}
*/

public static class CustomCoroutines
{
    public static IEnumerator OneFrameDelay(System.Action dangerous_callback)
    {
        yield return null;
        if (dangerous_callback != null)
            dangerous_callback.Invoke();
    }
    public static IEnumerator OneFrameDelayWithParameter<T>(System.Action<T> dangerous_callback, T obj)
    {
        yield return null;
        if (dangerous_callback != null)
            dangerous_callback.Invoke(obj);
    }
    public static IEnumerator WaitForSeconds(System.Action dangerous_callback, float seconds)
    {
        yield return new WaitForSeconds(seconds);
        if (dangerous_callback != null)
            dangerous_callback.Invoke();
    }
}
public static class RectTransformExtensions
{
    // all of these are "distance from border" (when stretch mode is active?)
    public static void SetLeft(this RectTransform rt, float left)
    {
        rt.offsetMin = new Vector2(left, rt.offsetMin.y);
    }

    public static void SetRight(this RectTransform rt, float right)
    {
        rt.offsetMax = new Vector2(-right, rt.offsetMax.y);
    }

    public static void SetTop(this RectTransform rt, float top)
    {
        rt.offsetMax = new Vector2(rt.offsetMax.x, -top);
    }

    public static void SetBottom(this RectTransform rt, float bottom)
    {
        rt.offsetMin = new Vector2(rt.offsetMin.x, bottom);
    }
    
    public static void SetPivotWithoutMoving(RectTransform target, Vector2 pivot)
    {
        if (!target) return;
        var offset=pivot - target.pivot;
        offset.Scale(target.rect.size);
        var wordlPos= target.position + target.TransformVector(offset);
        target.pivot = pivot;
        target.position = wordlPos;
    }
}
public abstract class UnitySerializedDictionary<TKey, TValue> : Dictionary<TKey, TValue>, ISerializationCallbackReceiver
{
	[SerializeField, HideInInspector]
	private List<TKey> keyData = new List<TKey>();
	
	[SerializeField, HideInInspector]
	private List<TValue> valueData = new List<TValue>();

    void ISerializationCallbackReceiver.OnAfterDeserialize()
    {
		this.Clear();
		for (int i = 0; i < this.keyData.Count && i < this.valueData.Count; i++)
		{
			this[this.keyData[i]] = this.valueData[i];
		}
    }

    void ISerializationCallbackReceiver.OnBeforeSerialize()
    {
		this.keyData.Clear();
		this.valueData.Clear();

		foreach (var item in this)
		{
			this.keyData.Add(item.Key);
			this.valueData.Add(item.Value);
		}
    }
}
