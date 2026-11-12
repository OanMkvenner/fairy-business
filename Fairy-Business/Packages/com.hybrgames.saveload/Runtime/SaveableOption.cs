using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Sirenix.OdinInspector;

public class SaveableOption : MonoBehaviour
{
    public string optionName;
    [ClearOnReload(assignNewTypeInstance:true)] // necessary as we have domain-reload disabled
    public static Dictionary<string, object> nameHashes = new Dictionary<string, object>();
    private bool initialized = false;


    private void Awake() {
        //InitAll(); // done in AppUser during loading - in Start()
    }

    [ClearOnReload(valueToAssign:false)] // necessary as we have domain-reload disabled
    static public bool initializedAll = false;
    static public void InitAll(){
        // only init ONCE
        if (initializedAll) return;
        else initializedAll = true;
        
        // only Init non-prefab GameObjects, otherwise we might have PERMANENT changes (GetSceneObjectsNonGeneric does not return prefabs)
        var allHashedObjects = Utilities.GetSceneObjectsNonGeneric<SaveableOption>();
        foreach (var hashedObject in allHashedObjects)
        {
            // only init saveables if the "SaveableOption" component is enabled
            if (hashedObject.enabled) hashedObject.Init();
        }
    }

    public void Init(){
        if (initialized) return;
        if (optionName == ""){
            Debug.LogError($"found SaveableOption with empty optionName on GameObject {name}, please set a valid name!", gameObject);
            return;
        }
        if (nameHashes.ContainsKey(optionName)){
            Debug.LogError("SaveableOption Dictionary clash: GameObject of name '"+name+"' already added!", gameObject);
        } else {
            object value = GetOptionValue();
            if (value == null){
                Debug.LogError("Cant get implied type, returning...");
                return;
            }
            initialized = true;
            object loadedValue = AppUser.TryGetOptionOfDynamicType(optionName, value.GetType());
            if (loadedValue != null){
                value = loadedValue;
            }
            nameHashes[optionName] = value;

            int amountOfSaveableComponents = 0;
            if (CheckComponentAvailable<Slider>()){
                amountOfSaveableComponents++;
                GetComponent<Slider>().value = (float)value;
                GetComponent<Slider>().onValueChanged.AddListener(SetValue);
                GetComponent<Slider>().onValueChanged.Invoke((float)value); // making sure initial values are synchronized properly and saved at least once (in case it never was)
            }
            if (CheckComponentAvailable<CustomSlider>()){
                amountOfSaveableComponents++;
                GetComponent<CustomSlider>().value = (float)value;
                GetComponent<CustomSlider>().onValueChanged.AddListener(SetValue);
                GetComponent<CustomSlider>().onValueChanged.Invoke((float)value); // making sure initial values are synchronized properly and saved at least once (in case it never was)
            }
            if (CheckComponentAvailable<TMP_InputField>()){
                amountOfSaveableComponents++;
                GetComponent<TMP_InputField>().text = (string)value;
                GetComponent<TMP_InputField>().onValueChanged.AddListener(SetValue);
                GetComponent<TMP_InputField>().onValueChanged.Invoke((string)value); // making sure initial values are synchronized properly and saved at least once (in case it never was)
            }
            if (amountOfSaveableComponents == 0){
                Debug.LogError($"No saveable component found on SaveableOption. GameObject: {name}, please add a saveable component or implement a component to be saveable!", gameObject);
            } else if (amountOfSaveableComponents >= 2) {
                Debug.LogError($"SaveableOption found multiple components to save - this is not yet supported! GameObject: {name}, please remove some of the saveable components!", gameObject);
            }
        }
    }

    bool CheckComponentAvailable<T>(){
        return GetComponent<T>() != null;
    }

    private void OnEnable(){
        if (!initialized) Init();
    }

    void SetValue<T>(T value){
        nameHashes[optionName] = (object)value;
        AppUser.SaveOption(optionName, value);
    }

    public object GetOptionValue(){
        {
            if (CheckComponentAvailable<Slider>()){
                return GetComponent<Slider>().value;
            }
            if (CheckComponentAvailable<TMP_InputField>()){
                return GetComponent<TMP_InputField>().text;
            }
            if (CheckComponentAvailable<CustomSlider>()){
                return GetComponent<CustomSlider>().value;
            }
        }
        Debug.LogError("Tried to GetValue from a SaveableOption that has no implemented type of UI element on it. Check if yours is missing or not yet implemented!", gameObject);
        return null;
    }

    [Button]
    public void DeleteSavedOption()
    {
        AppUser.DeleteOption(optionName);
    }
    [Button]
    public void CheckSavedOption()
    {
        object value = GetOptionValue();
        if (value == null){
            Debug.LogError("Cant get implied type, returning...");
            return;
        }
        object loadedValue = AppUser.TryGetOptionOfDynamicType(optionName, value.GetType());
        Debug.LogError(loadedValue);
    }

    public static T GetOptionValue<T>(string targetName){
        object foundValue = GetOptionValue(targetName);
        if (foundValue.GetType() != typeof(T)){
            Debug.LogError($"Tried GetOptionValue with a incorrect type!{targetName} is of type {foundValue.GetType()} but you asked for {typeof(T)}");
        }
        return (T)GetOptionValue(targetName);
    }
    public static object GetOptionValue(string targetName){
        if (!nameHashes.ContainsKey(targetName))
        {
            Debug.LogError("SaveableOption Dictionary doesnt contain Transform of name: '" +targetName+"'");
            return null;
        } else {
            return nameHashes[targetName];
        }
    }
}
