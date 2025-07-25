using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UniqueNameHash : MonoBehaviour
{
    public static Dictionary<string, Transform> nameHashes = new Dictionary<string, Transform>();

    static bool initialized = false;
    static public void InitAll(){
        // only init ONCE
        if (initialized) return;
        else initialized = true;

        // only Init non-prefab GameObjects, otherwise we might have PERMANENT changes (GetSceneObjectsNonGeneric does not return prefabs)
        var allHashedObjects = Utilities.GetSceneObjectsNonGeneric<UniqueNameHash>();
        foreach (var hashedObject in allHashedObjects)
        {
            hashedObject.Init();
        }
    }

    private void Awake() {
        InitAll();
    }

    public void Init(){

        if (nameHashes.ContainsKey(name)){
            Debug.LogError("UniqueNameHash Dictionary clash: GameObject of name '"+name+"' already added!");
        } else {
            nameHashes[name] = transform;
        }
    }

    public static Transform Get(string targetName){
        if (!nameHashes.ContainsKey(targetName))
        {
            Debug.LogError("UniqueNameHash Dictionary doesnt contain Transform of name: '" +targetName+"'");
            return null;
        } else {
            return nameHashes[targetName];
        }
    }
}
