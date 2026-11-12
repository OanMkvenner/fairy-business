using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

public static class UtilitiesSO {
    public static void MoveOutAsset<T>(T subAsset, string newName = "") where T : ScriptableObject {
        //[Button("MoveOutAsset")]
        //[ShowIf("CheckIsSubAsset")]
        string currentAssetPath = AssetDatabase.GetAssetPath(subAsset);

        int lastSlash = currentAssetPath.LastIndexOf('/');
        if (lastSlash == -1) return; // No folders
        string assetString = currentAssetPath.Substring(0, lastSlash);

        UnityEditor.AssetDatabase.RemoveObjectFromAsset(subAsset);
        if (newName != "") {
            subAsset.name = newName;
        }
        var finalAssetPath = assetString + "/" + subAsset.name + ".asset";
        finalAssetPath = AssetDatabase.GenerateUniqueAssetPath(finalAssetPath);
        UnityEditor.AssetDatabase.CreateAsset(subAsset, finalAssetPath);
        AssetDatabase.SaveAssets();
        AssetDatabase.ImportAsset(currentAssetPath);
        EditorGUIUtility.PingObject( subAsset );
    }
    public static void MoveInAsset<T>(Sirenix.OdinInspector.Editor.InspectorProperty inspectorProperties, ref T subAsset, string newName = "") where T : ScriptableObject {
        string currentAssetPath = AssetDatabase.GetAssetPath(inspectorProperties.SerializationRoot.ValueEntry.WeakSmartValue as UnityEngine.Object);
        var mainAsset = UnityEditor.AssetDatabase.LoadAssetAtPath(currentAssetPath, typeof(ScriptableObject)) as ScriptableObject;
        string previousAssetPath = AssetDatabase.GetAssetPath(subAsset);
        AssetDatabase.RemoveObjectFromAsset(subAsset);
        AssetDatabase.DeleteAsset(previousAssetPath);
        if (newName != "") {
            subAsset.name = newName;
        }
        //ubAsset = newScriptable;
        AssetDatabase.AddObjectToAsset(subAsset, mainAsset);
        AssetDatabase.SaveAssets();
        AssetDatabase.ImportAsset(currentAssetPath);
        AssetDatabase.ImportAsset(previousAssetPath);
        EditorGUIUtility.PingObject( subAsset );
    }
    
    public static void OnAddSaveChanges(UnityEngine.Object asset) {
        EditorUtility.SetDirty(asset);
        AssetDatabase.SaveAssets();
        AssetDatabase.ImportAsset(AssetDatabase.GetAssetPath(asset));
    }
    public static T AddCreateLocalScriptableObject<T>(
        Sirenix.OdinInspector.Editor.InspectorProperty inspectorProperties, string newAssetName = "") where T: ScriptableObject{
        int group = Undo.GetCurrentGroup();
        string currentAssetPath = AssetDatabase.GetAssetPath(inspectorProperties.SerializationRoot.ValueEntry.WeakSmartValue as UnityEngine.Object);
        ScriptableObject mainAsset = UnityEditor.AssetDatabase.LoadAssetAtPath(currentAssetPath, typeof(ScriptableObject)) as ScriptableObject;
        var target = UtilitiesSO.CreateLocalScriptableObject<T>(mainAsset, newAssetName: newAssetName);
        // assign the newly created SO to the member the inspectorProperties are pointing to (only works if inspectorProperties are pointing to the actual SO member, not a method)
        Undo.RegisterCreatedObjectUndo(mainAsset, "assigned created SO to property");
        inspectorProperties.ValueEntry.WeakSmartValue = target;
        OnAddSaveChanges(mainAsset);
        Undo.CollapseUndoOperations(group);
        
        // show in inspector
        PopUpAssetInspector.Create(target);
        return target;
    }
    public static void addCreateLocalScriptableObject_OverrideTargetAsset<T>(
        Sirenix.OdinInspector.Editor.InspectorProperty inspectorProperties, ref T target, out ScriptableObject mainAsset, string newAssetName = "") where T: ScriptableObject{
        string currentAssetPath = AssetDatabase.GetAssetPath(inspectorProperties.SerializationRoot.ValueEntry.WeakSmartValue as UnityEngine.Object);
        mainAsset = UnityEditor.AssetDatabase.LoadAssetAtPath(currentAssetPath, typeof(ScriptableObject)) as ScriptableObject;
        target = UtilitiesSO.CreateLocalScriptableObject<T>(mainAsset, newAssetName: newAssetName);
        OnAddSaveChanges(mainAsset);
        // show in inspector
        PopUpAssetInspector.Create(target);
    }
    public static T CreateLocalScriptableObject<T>(UnityEngine.Object asset, string newAssetName = "") where T: ScriptableObject{
        int group = Undo.GetCurrentGroup();
        T newlyCreatedSO = ScriptableObject.CreateInstance(typeof(T)) as T;
        
        // use the newly created one
        if (newAssetName == "") {
            newAssetName = typeof(T).ToString() + "_" + asset.name;
        }
        newlyCreatedSO.name = newAssetName;

        Undo.RegisterCompleteObjectUndo(asset, "Created local ScriptableObject");
        AssetDatabase.AddObjectToAsset(newlyCreatedSO, asset);
        EditorUtility.SetDirty(asset);
        AssetDatabase.SaveAssets();
        Undo.RegisterCreatedObjectUndo(newlyCreatedSO, "Created local ScriptableObject");
        
        string currentAssetPath = AssetDatabase.GetAssetPath(asset);
        AssetDatabase.ImportAsset(currentAssetPath);
        Undo.CollapseUndoOperations(group);
        return newlyCreatedSO;
    }

    public static void OnValueChangeDestroyUnreferencedLocalAssets(
        Sirenix.OdinInspector.Editor.InspectorProperty inspectorProperties) {
        // try to get the selected asset. InspectorProperty serializationRoot doesnt necessarily go up to the actual root object though! 
        // it might stop at a ScriptableObject that is a child of the actual root object. "CheckDestroyUnreferencedLocalAssets" deals with that gracefully though!
        // alternative: Selection.activeObject --- but im unsure how safe it is to use. Maybe you can somehow have something else selected while this happens?
        var targetMainOrSubAsset = inspectorProperties.SerializationRoot.ValueEntry.WeakSmartValue as UnityEngine.Object;
        UtilitiesSO.CheckDestroyUnreferencedLocalAssets(targetMainOrSubAsset);
    }
    public static void CheckDestroyUnreferencedLocalAssets(UnityEngine.Object subOrMainAsset) {
        string currentAssetPath = AssetDatabase.GetAssetPath(subOrMainAsset);
        var mainAsset = UnityEditor.AssetDatabase.LoadAssetAtPath(currentAssetPath, typeof(ScriptableObject)) as object;
        Dictionary<object, bool> allSubAssets = UnityEditor.AssetDatabase.LoadAllAssetsAtPath(currentAssetPath).ToDictionary(x => (object)x, y => false);
        //List<KeyValuePair<Object, bool>> allSubAssets;
        CheckLocalSubAssetsStillReferenced(mainAsset, allSubAssets, new());
        bool destroyedAtLeastOne = false;
        foreach (var subAsset in allSubAssets) {
            if (subAsset.Key != mainAsset){
                if (!subAsset.Value) {
                    Debug.LogError($"destroying unreferenced object:{subAsset.Key}");
                    destroyedAtLeastOne = true;
                    //AssetDatabase.RemoveObjectFromAsset((UnityEngine.Object)subAsset.Key);
                    Undo.DestroyObjectImmediate((UnityEngine.Object)subAsset.Key);
                }
            }
        }
        if (destroyedAtLeastOne){
            AssetDatabase.SaveAssets();
            AssetDatabase.ImportAsset(currentAssetPath);
        }
    }
    // we iteratively go through references contained by "assetToCheck", and check if every SubAsset is at least referenced once somewhere in there
    private static void CheckLocalSubAssetsStillReferenced(object assetToCheck, Dictionary<object, bool> allSubAssets, List<object> checkedObjects) {
        if (assetToCheck == null) return;
        // avoid cyclic checks
        if (checkedObjects.Contains(assetToCheck)) return;
        else checkedObjects.Add(assetToCheck);
        
        var type = assetToCheck.GetType();
        if (type.IsPrimitive || type.IsEnum || type == typeof(string) || type == typeof(decimal) || type == typeof(DateTime) || type == typeof(TimeSpan)) return;
        
        List<Type> referenceTypes = new() {
            typeof(ScriptableObject),
            typeof(Sprite),
            typeof(GameObject),
        };

        bool assigneableType = false;
        foreach (var referenceType in referenceTypes) {
            assigneableType = assigneableType || referenceType.IsAssignableFrom(type);
        } 
        if (assigneableType) {
            // check against list of local assets
            if (allSubAssets.ContainsKey(assetToCheck)) {
                // if it IS a local asset, tick that asset off as "yo you are referenced, nice!"
                allSubAssets[assetToCheck] = true;
                // but still go deeper into that asset and continue checking. It might have more referenced local assets inside
            } else {
                // if it is NOT a local asset, we STOP here! do NOT go deeper, this isnt our problem anymore!
                return;
            }
        }
        
        FieldInfo[] fis = assetToCheck.GetType().GetFields();
        for (int i = 0; i < fis.Length; i++)
        {
            FieldInfo fi =  fis[i];
            if (fi.FieldType.IsGenericList())
            { 
                //Type listType = fi.FieldType.GetGenericArguments()[0];
                //if (!checkedObjects.Contains(listType))
                //{
                    var list = fi.GetValue(assetToCheck) as IList;
                    if (list == null) continue;
                    for (int j = 0; j < list.Count; j++)
                    {
                        CheckLocalSubAssetsStillReferenced(list[j], allSubAssets, checkedObjects);
                    }
                //}
            }
            else
            {
                //if (!checkedObjects.Contains(fi.FieldType))
                //{
                CheckLocalSubAssetsStillReferenced(fi.GetValue(assetToCheck), allSubAssets, checkedObjects);
                //}
            }
        }
    }

    private static List<Type> CrawlTypesCheckReferences(object target, ref List<Type> typesWithReferences, ref List<Type> alreadyCheckedTypes) {
        List<Type> referenceTypes = new() {
            typeof(ScriptableObject),
            typeof(Sprite),
            typeof(GameObject),
        };
        var type = target.GetType();
        FieldInfo[] fis = type.GetFields();
        for (int i = 0; i < fis.Length; i++) {
            FieldInfo fi =  fis[i];
            Type relevantType = fi.FieldType;
            
            if (relevantType.IsGenericList()) { 
                relevantType = relevantType.GetGenericArguments()[0];
            }
            if (referenceTypes.Contains(relevantType)) {
                typesWithReferences.AddUnique(type);
            } else {
                if (alreadyCheckedTypes.Contains(relevantType)) {
                    if (typesWithReferences.Contains(relevantType)) {
                        // subtype has already been checked, but it does contain references, so remember current type as having references as wel
                        typesWithReferences.AddUnique(type);
                    }
                    continue;
                }
                alreadyCheckedTypes.AddUnique(relevantType);
                var newEntries = CrawlTypesCheckReferences(relevantType, ref typesWithReferences, ref alreadyCheckedTypes);
                if (newEntries.Count > 0) {
                    typesWithReferences.AddUnique(type);
                    typesWithReferences.AddRangeUnion(newEntries);
                }
            }
        }
        return typesWithReferences;
    }

    /*
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void UpdateTypesThatHaveScriptableReferences() {
        typesThatHaveScriptableReferences = new();
        // crawl as deep as possible, if any reference-type is found, remember ALL types
        // ABOVE it and add them to the list (so we know where to crawl deeper in the future)
        var allInheritedTypes = AppDomain.CurrentDomain.GetAssemblies()
            .SelectMany(assembly => assembly.GetTypes())
            .Where(type => type.IsSubclassOf(typeof(TempQualityData)));

        var alreadyCheckedTypes = new List<Type>();
        foreach (var baseType in allInheritedTypes) {
            CrawlTypesCheckReferences(baseType, ref typesThatHaveScriptableReferences, ref alreadyCheckedTypes);
        }
    }
*/
}