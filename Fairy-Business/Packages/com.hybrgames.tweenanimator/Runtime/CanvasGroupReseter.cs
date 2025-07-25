using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(CanvasGroup))]
public class CanvasGroupReseter : MonoBehaviour
{
    float grpalpha;
    bool grpblocksRaycasts;
    bool grpInteractable;
    bool grpignoreParentGroups;
    bool initialized = false;

    CanvasGroup cachedGroup = null;
    void Awake(){
    }

    // Start is called before the first frame update
    void Start()
    {
        if (!initialized) {
            cachedGroup = GetComponent<CanvasGroup>();
            grpalpha = cachedGroup.alpha;
            grpblocksRaycasts = cachedGroup.blocksRaycasts;
            grpInteractable = cachedGroup.interactable;
            grpignoreParentGroups = cachedGroup.ignoreParentGroups;
        }
        initialized = true;
    }

    public void CheckInitialized(){
        if (!initialized){
            Start();
        }
    }

    public void ResetCanvasGroup(){
        CheckInitialized();
        if (cachedGroup == null)
        {
            cachedGroup = GetComponent<CanvasGroup>();
        }
        cachedGroup.alpha = grpalpha;
        cachedGroup.blocksRaycasts = grpblocksRaycasts;
        cachedGroup.interactable = grpInteractable;
        cachedGroup.ignoreParentGroups = grpignoreParentGroups;
    }

    public void DisableCanvasGroup(){
        CheckInitialized();
        cachedGroup.alpha = 0;
        cachedGroup.blocksRaycasts = false;
        cachedGroup.interactable = false;
    }
    public void DisableInteractions(){
        CheckInitialized();
        cachedGroup.blocksRaycasts = false;
        cachedGroup.interactable = false;
    }
}
