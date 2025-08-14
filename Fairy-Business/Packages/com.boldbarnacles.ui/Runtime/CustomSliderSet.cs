using System;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using UnityEngine.InputSystem;
using Sirenix.OdinInspector;
using Unity.Mathematics;
using DG.Tweening;

public class CustomSliderSet : MonoBehaviour {
    public CustomSlider mainSlider;
    [Tooltip("Should be interleaved with the main slider, but drawn below the actual slider fillage. Can be used to highlight potentially added values")]
    public CustomSlider additionHighlightSlider;
    [Tooltip("Should be interleaved with the main slider, but drawn above the actual slider fillage. Can be used to highlight potentially subtracted values")]
    public CustomSlider subtractionHighlightSlider;
    [Tooltip("Should be interleaved with the main slider, but drawn above the actual slider fillage. Can be used to highlight potentially change of values in either direction")]
    public CustomSlider newChangeHighlightSlider;

    void Start()
    {
        subtractionHighlightSlider.useOffset = true;
        newChangeHighlightSlider.useOffset = true;
        ResetAdditionHighlightSlider();
        ResetSubtractionHighlightSlider();
        ResetChangeHighlightSlider();
    }
    void ResetAdditionHighlightSlider(){
        additionHighlightSlider.normalizedValue = 0;
    }
    void ResetSubtractionHighlightSlider(){
        subtractionHighlightSlider.normalizedValue = 0;
        subtractionHighlightSlider.normalizedOffsetValue = 1;
    }
    void ResetChangeHighlightSlider(){
        newChangeHighlightSlider.normalizedValue = 0;
        newChangeHighlightSlider.normalizedOffsetValue = 1;
    }

    public void SetChangeTargetNormalized(float newChangeTarget)
    {
        newChangeTarget = math.clamp(newChangeTarget, 0, 1.0f);
        var mainNormalizedValue = mainSlider.normalizedValue;
        if (newChangeTarget < mainNormalizedValue){
            // reset other highlighters
            ResetChangeHighlightSlider();

            additionHighlightSlider.normalizedValue = newChangeTarget;
            subtractionHighlightSlider.normalizedOffsetValue = newChangeTarget;
            subtractionHighlightSlider.normalizedValue = mainNormalizedValue;
        } else {
            additionHighlightSlider.normalizedValue = newChangeTarget;
            
            ResetSubtractionHighlightSlider();
            //subtractionHighlightSlider.normalizedOffsetValue = mainNormalizedValue;
            //subtractionHighlightSlider.normalizedValue = mainNormalizedValue;
        }
    }

    public void SetMainValueNormalized(float newMainVal)
    {
        newMainVal = math.clamp(newMainVal, 0, 1.0f);
        mainSlider.value = newMainVal;
    }
}