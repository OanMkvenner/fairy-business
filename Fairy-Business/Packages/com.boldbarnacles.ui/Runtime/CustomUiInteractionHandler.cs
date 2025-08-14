using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.Serialization;
using UnityEngine;
using UnityEngine.UI;
using System;

[AddComponentMenu("Custom UI/Custom Ui Click Handler", 30)]
[RequireComponent(typeof(RectTransform))]
public class CustomUiInteractionHandler : Selectable, IDragHandler, IEndDragHandler, IInitializePotentialDragHandler, ISubmitHandler, ICanvasElement
{
    public bool draggingAllowed = false;

    public UnityEvent<PointerEventData> onPointerDownEvent = new UnityEvent<PointerEventData>();
    public UnityEvent<PointerEventData> onPointerUpEvent = new UnityEvent<PointerEventData>();
    public UnityEvent<PointerEventData> onOnInitializePotentialDragEvent = new UnityEvent<PointerEventData>();
    public UnityEvent<PointerEventData> onDragEvent = new UnityEvent<PointerEventData>();
    public UnityEvent<PointerEventData> onEndDragEvent = new UnityEvent<PointerEventData>();
    public UnityEvent<PointerEventData> onPointerEnter = new UnityEvent<PointerEventData>();
    public UnityEvent<PointerEventData> onPointerExit = new UnityEvent<PointerEventData>();
    

    public override void OnPointerDown(PointerEventData eventData){
        if (eventData.button != PointerEventData.InputButton.Left)
            return;
        onPointerDownEvent.Invoke(eventData);
    }
    public override void OnPointerUp(PointerEventData eventData){
        if (eventData.button != PointerEventData.InputButton.Left)
            return;
        onPointerUpEvent.Invoke(eventData);
    }
    public override void OnPointerEnter(PointerEventData eventData){
        onPointerEnter.Invoke(eventData);
    }
    public override void OnPointerExit(PointerEventData eventData){
        onPointerExit.Invoke(eventData);
    }
    public virtual void OnSubmit(BaseEventData eventData){
        onPointerDownEvent.Invoke(eventData as PointerEventData);
        onPointerUpEvent.Invoke(eventData as PointerEventData);
    }

    public virtual void OnInitializePotentialDrag(PointerEventData eventData){
        onOnInitializePotentialDragEvent.Invoke(eventData);
    }
    public virtual void OnDrag(PointerEventData eventData){
        onDragEvent.Invoke(eventData);
    }
    public virtual void OnEndDrag(PointerEventData eventData){
        onEndDragEvent.Invoke(eventData);
    }

    
#if UNITY_EDITOR
    protected override void OnValidate()
    {
        base.OnValidate();

        if (!UnityEditor.PrefabUtility.IsPartOfPrefabAsset(this) && !Application.isPlaying)
            CanvasUpdateRegistry.RegisterCanvasElementForLayoutRebuild(this);
    }

#endif // if UNITY_EDITOR

    public virtual void Rebuild(CanvasUpdate executing)
    {
#if UNITY_EDITOR
#endif
    }
    public virtual void LayoutComplete()
    {}

    public virtual void GraphicUpdateComplete()
    {}

}