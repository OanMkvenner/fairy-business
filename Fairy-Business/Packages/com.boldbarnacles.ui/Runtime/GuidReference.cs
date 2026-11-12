using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using MemoryPack;
using Sirenix.OdinInspector;
using Sirenix.Serialization;
using UnityEngine;
using UnityEditor;


#if UNITY_EDITOR && UNITY_2021_1_OR_NEWER

    internal sealed class AssetGUIDAssetPostProcessor : AssetPostprocessor
    {
#pragma warning disable UNT0033 // Incorrect message case
        private static void OnPostprocessAllAssets(string[] importedAssets, string[] deletedAssets, string[] movedAssets, string[] movedFromAssetPaths, bool didDomainReload)
#pragma warning restore UNT0033 // Incorrect message case
        {
            if (didDomainReload){
                return;
            }

            int importedCount = importedAssets.Length;
            List<Guid> guids = new List<Guid>();
            List<UnityEngine.Object> assets = new List<UnityEngine.Object>();
            if (importedCount > 0) {
                
                var guidReferencedAssets = Resources.LoadAll<SerializedScriptableObjectWithGuid>("AllScriptableObjects/");
                
                foreach (var item in guidReferencedAssets) {
                    guids.Add(item.GetGuid());
                    assets.Add(item);
                }
            }
            foreach (var assetPath in importedAssets) {
                // load asset and all sub-assets at path
                UnityEngine.Object[] assetsAtPath = AssetDatabase.LoadAllAssetsAtPath(assetPath);
                foreach (var asset in assetsAtPath) {
                    if (asset != null && asset is IGuidReferenced assetGUID)
                    {
                        for (int j = 0; j < guids.Count; j++) {
                            var guid = guids[j];
                            // check if any of them have the same guid as any already existing (excluding itself of course)
                            if (guid == assetGUID.GetGuid() && (UnityEngine.Object)assetGUID != assets[j]) {
                                Debug.LogWarning("Asset GUID: Created Asset has same GUID as other assets. Recreating GUID", asset);
                                assetGUID.ResetGuid();
                                EditorUtility.SetDirty(asset);
                            }
                        }
                    }
                }
            }
            AssetDatabase.SaveAssets();
        }
    }
#endif

public interface IGuidReferenced
{
    public System.Guid GetGuid();
    public void ResetGuid();
}

// similar to GuidRef<T> but untyped, to hold one big dict containing all guid-referenced objects - for finding an object of unknown type
public static class GuidRef {
    public static Dictionary<System.Guid, UnityEngine.Object> dict = null;

    public static UnityEngine.Object GetFromId(Guid id) {
        dict.TryGetValue(id, out var value);
        return value;
    }
    public static T GetFromId<T>(Guid id) where T : UnityEngine.Object, IGuidReferenced {
        return GuidRef<T>.GetFromId(id);
    }
    
    // initialize GuidRef<T>.dict for each T that implements IGuidReferenced
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void UpdateTypesThatHaveScriptableReferences() {
        GuidRef.dict = new();
        
        var types = AppDomain.CurrentDomain.GetAssemblies()
            .SelectMany(s => s.GetTypes())
            .Where(p => typeof(IGuidReferenced).IsAssignableFrom(p));
        Type myClass = typeof(GuidRef<>);
        foreach (var type in types) {
            if (type == typeof(IGuidReferenced)) continue;
            Type genMyClass = myClass.MakeGenericType(type);
            genMyClass.InvokeMember("InitDefault", BindingFlags.InvokeMethod | BindingFlags.Public | BindingFlags.Static, null, null, null);
        }
    }
}

public class SerializedScriptableObjectWithGuid : SerializedScriptableObject, IGuidReferenced {
    [OdinSerialize] private Guid globalId = Guid.NewGuid(); 
    public Guid GetGuid() { return globalId; }

    public void ResetGuid() {
        Debug.LogWarning($"Reseting Guid for {name}", this);
        globalId = Guid.NewGuid();
    }
}
// 
[MemoryPackable]
public partial class GuidRef<T> where T : UnityEngine.Object, IGuidReferenced {
    public System.Guid id = Guid.Empty;
    [MemoryPackIgnore]
    [ShowInInspector]
    private T cache = null;
    [MemoryPackIgnore]
    public static Dictionary<System.Guid, T> dict = null; // this creates a separate dict for each <T> type in use!
    // returns the UnityEngine.Object (usually ScriptableObject) thats referenced by the Guid id
    public T Get() {
        if (dict == null) {
            Debug.LogError($"Guid-Dictionary not initialized for type {typeof(T)}. Have you called GuidRef.Init<T>(...) for this type yet?");
            return null;
        }
        if (id == Guid.Empty) {
            Debug.LogError($"Trying to reference object of type {typeof(T)} using an empty guid. Did you forget setting it?");
            return null;
        }
        if (cache != null) return cache;
        dict.TryGetValue(id, out cache);
        if (cache == null) {
            Debug.LogError($"Trying to reference object of type {typeof(T)} but could not be found in dictionary! Missing Guid: {id}");
            return null;
        }
        return cache;
    }
    public void Set(System.Guid newId) {
        id = newId;
        cache = null;
        cache = Get();
    }
    
    public static T GetFromId(Guid id) {
        dict.TryGetValue(id, out T value);
        return value;
    }
    public static void InitDefault() {
        var loadedItems = Resources.LoadAll<T>("AllScriptableObjects/");
        dict = new();
        foreach (var item in loadedItems) {
            var guid = item.GetGuid();
            if (dict.ContainsKey(guid)) {
                Debug.LogError($"Found Guid clash! {item} and {dict[guid]} have the same guid: {guid}");
            }
            dict[guid] = item;
            GuidRef.dict[guid] = item;
        }
    }
}
