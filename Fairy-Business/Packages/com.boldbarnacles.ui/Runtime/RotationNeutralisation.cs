using UnityEngine;
using Unity.Mathematics;

// counters the rotation of the parent transform. Allows you to keep original rotation while moving along the parent rotation position
public class RotationNeutralisation : MonoBehaviour {
    private quaternion cachedOriginalRotation = new quaternion();
    private void Awake() {
        cachedOriginalRotation = transform.localRotation;
    }
    bool initialized = false;
    private void Start() {
        NeutralizeRotation();
        initialized = true;
    }
    private quaternion cachedParentRotation = new quaternion();
    private void Update() {
    }

    public void NeutralizeRotation(){
        if (!initialized) return;
        var parentRot = transform.parent.localRotation;
        if (!parentRot.Equals(cachedParentRotation)){
            cachedParentRotation = parentRot;
            transform.localRotation = cachedOriginalRotation * Quaternion.Inverse(parentRot);
        }
    }
}