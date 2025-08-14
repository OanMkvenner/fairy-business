using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.Serialization;
using UnityEngine;
using UnityEngine.UI;
using System;
using NaughtyAttributes;
using System.Collections.Generic;
using Unity.Mathematics;

[AddComponentMenu("Custom UI/Custom Gauge", 30)]
[RequireComponent(typeof(RectTransform))]
public class CustomGauge : UIBehaviour, IDataReceiver
{
    public GameObject gaugeNeedle; // only shown while the mouse is held down on the button. If not existing: switches to enabled graphic immediately
    public float startRotation = 0;
    public float endRotation = 360;
    public float minValue = 0;
    public float maxValue = 1.0f;

    public void TakeNewData(float newValue){
        if (gaugeNeedle){
            var percent = (  newValue - minValue) / (maxValue - minValue);
            percent = Mathf.Clamp(percent, 0, 1);
            var finalRotation = (endRotation - startRotation) * percent + startRotation;
            var angles = gaugeNeedle.transform.localRotation.eulerAngles;
            angles.z = finalRotation;
            gaugeNeedle.transform.localRotation = Quaternion.Euler(angles);
        }
    }

    protected CustomGauge(){}
    public virtual void LayoutComplete(){}
    public virtual void GraphicUpdateComplete(){}
    override protected void Reset(){} // similar to "awake" but it also executes even if disabled (i think)
    public virtual void Rebuild(CanvasUpdate executing)
    {
#if UNITY_EDITOR
#endif
    }
}
