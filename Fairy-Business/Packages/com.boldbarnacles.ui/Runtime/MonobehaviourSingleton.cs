using UnityEngine;

public class MonobheaviourSingletonCustom<T> : MonoBehaviour where T : MonoBehaviour  {
    private static T inst = null;
    public static T instance {
        get {
            if (!Application.isPlaying) {
                Debug.LogError($"trying to access MonobheaviourSingletonCustom {typeof(T)} outside of running game. Please make sure you are only accessing it at runtime!");
                return null;
            }
            if (!inst) {
                var instances = FindObjectsByType(typeof(T), FindObjectsInactive.Include, FindObjectsSortMode.None);
                if (instances.Length == 0) {
                    Debug.LogError($"No instance of type {typeof(T)} found in Scene.\nPlease add it somewhere into the Scene.");
                    return null;
                } else {
                    inst = (T)instances[0];
                    if (instances.Length > 1){
                        Debug.LogError($"More than 1 instance of type {typeof(T)} was found, but MonobheaviourSingletonCustom expected singleton. Choosing first found instance");
                    }
                }
            }
            return inst;
        }
    }
}
