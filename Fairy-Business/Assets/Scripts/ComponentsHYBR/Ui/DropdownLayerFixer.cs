using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DropdownLayerFixer : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnEnable()
    {
        StartCoroutine (CustomCoroutines.OneFrameDelay(FixLayerOrder));
    }

    private void FixLayerOrder(){
        // check for closest parent Canvas and take its sortingLayer override, if applicable
        Canvas parentCanvas = null;
        Transform parentTransform = transform.parent;
        while (parentTransform != null)
        {
            parentCanvas = parentTransform.GetComponent<Canvas>();
            if (parentCanvas != null)
                break;
            parentTransform = parentTransform.parent;
        }

        Canvas canvas = GetComponent<Canvas>();
        if(canvas && parentCanvas != null)
        {
            canvas.sortingLayerID = parentCanvas.sortingLayerID;
            canvas.sortingOrder = parentCanvas.sortingOrder + 1; // instead of 30000 hardcoded value, lets just go "one higher" than the parent order
        }
    }

}
