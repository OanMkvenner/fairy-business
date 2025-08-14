using UnityEngine;

public class ScriptableSingletonCustom<T> : ScriptableObject where T : ScriptableObject  {
    private static T inst = null;
    public static T instance {
        get {
            if (!inst) {
                var instances = Resources.LoadAll<T>("SOSingletons/");
                if (instances.Length == 0) {
                    Debug.LogError($"No instance of type {typeof(T)} found in SOSingletons/.\nDid you forget to put it inside the Resources/SOSingletons/ folder?");
                    return null;
                } else {
                    inst = instances[0];
                }
            }
            return inst;
        }
    }
}
