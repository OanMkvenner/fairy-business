using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SaveableOption : MonoBehaviour
{
    public string optionName;
    public static Dictionary<string, object> nameHashes = new Dictionary<string, object>();


    private void Awake() {
        //InitAll(); // done in AppUser during loading - in Start()
    }

    static bool initialized = false;
    static public void InitAll(){
        // only init ONCE
        if (initialized) return;
        else initialized = true;

        // only Init non-prefab GameObjects, otherwise we might have PERMANENT changes (GetSceneObjectsNonGeneric does not return prefabs)
        var allHashedObjects = Utilities.GetSceneObjectsNonGeneric<SaveableOption>();
        foreach (var hashedObject in allHashedObjects)
        {
            hashedObject.Init();
        }
    }

    public void Init(){
        if (optionName == ""){
            Debug.LogError($"found SaveableOption with empty optionName on GameObject {name}, please set a valid name!");
            return;
        }
        if (nameHashes.ContainsKey(optionName)){
            Debug.LogError("SaveableOption Dictionary clash: GameObject of name '"+name+"' already added!");
        } else {
            object value = GetOptionValue();
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
                SetValue((float)value); // this sets the value from the slider as initial saved value, in case no value was loaded
            }
            if (CheckComponentAvailable<TMP_InputField>()){
                amountOfSaveableComponents++;
                GetComponent<TMP_InputField>().text = (string)value;
                GetComponent<TMP_InputField>().onValueChanged.AddListener(SetValue);
                SetValue((string)value); // this sets the value from the slider as initial saved value, in case no value was loaded
            }
            if (amountOfSaveableComponents == 0){
                Debug.LogError($"No saveable component found on SaveableOption. GameObject: {name}, please add a saveable component or implement a component to be saveable!");
            } else if (amountOfSaveableComponents >= 2) {
                Debug.LogError($"SaveableOption found multiple components to save - this is not yet supported! GameObject: {name}, please remove some of the saveable components!");
            }
        }
    }

    bool CheckComponentAvailable<T>(){
        return GetComponent<T>() != null;
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
        }
        Debug.LogError("Tried to GetValue from a SaveableOption that has no implemented type of UI element on it. Check if yours is missing or not yet implemented!");
        return null;
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
