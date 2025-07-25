using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TransformReseter : MonoBehaviour
{
    Vector3 originalPosition;
    Quaternion originalRotation;
    Vector3 originalLocalScale;
    bool initialized = false;

    Transform cachedTransform;
    void Awake(){
    }

    // Start is called before the first frame update
    void Start()
    {
        if (!initialized) {
            cachedTransform = GetComponent<Transform>();
            originalPosition = cachedTransform.localPosition;
            originalRotation = cachedTransform.localRotation;
            originalLocalScale = cachedTransform.localScale;
        }
        initialized = true;
    }
    public void CheckInitialized(){
        if (!initialized){
            Start();
        }
    }
    public void ResetTransform(){
        CheckInitialized();
        cachedTransform.localPosition = originalPosition;
        cachedTransform.localRotation = originalRotation;
        cachedTransform.localScale = originalLocalScale;
    }
}
