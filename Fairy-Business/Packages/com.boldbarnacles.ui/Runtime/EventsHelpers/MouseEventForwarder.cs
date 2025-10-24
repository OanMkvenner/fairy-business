using UnityEngine.UI;
using UnityEngine;
using UnityEngine.EventSystems;
using Sirenix.OdinInspector;

// Attach this script to a GameObject with a raycastable image (or any component intercepting clicks/drags) and then use one of the methods to forward (copy) the events to other objects
public class MouseEventForwarder : MonoBehaviour, IPointerClickHandler,/* IPointerEnterHandler, IPointerExitHandler ,*/ IBeginDragHandler, IEndDragHandler, IDragHandler 
{
    [Tooltip("If this is set, any forwarded mouse event from a child MouseEventForwarder will be intercepted here and spread to all components (including this component itself!)")]
    public bool receiveForwarding = false;

    [Tooltip("If this is set, any mouse event goes Up the parent-tree until it hits another MouseEventForwarder with receiveForwarding activated")]
    [HideIf("@tgtGameObject != null || tgtComponent != null")]
    public bool forwardToNextAcceptingParent = false;


    [Tooltip("If we want to send it to all components of the target gameobject. if you need multiple targets, just add multiple instances of MouseEventForwarder to this Gameobject!")]
    [HideIf("@tgtComponent != null || forwardToNextAcceptingParent")]
    public GameObject tgtGameObject = null;

    [Tooltip("if we only want to send it to a specific component. if you need multiple targets, just add multiple instances of MouseEventForwarder to this Gameobject!")]
    [HideIf("@tgtGameObject != null || forwardToNextAcceptingParent")]
    public MonoBehaviour tgtComponent = null;


    public MouseEventForwarder GetNextParentEventForwarder(Transform searchParent){
        if (searchParent == null) {
            Debug.LogError("MouseEventForwarder didnt find another MouseEventForwarder in its parents!");
            return null;
        }
        var mouseForwarders = searchParent.GetComponents<MouseEventForwarder>();
        MouseEventForwarder foundForwardableMouseEventForwarder = null;
        foreach (var item in mouseForwarders)
        {
            if (item.receiveForwarding) {
                foundForwardableMouseEventForwarder = item;
                break;
            } 
        }
        if (foundForwardableMouseEventForwarder != null){
            return foundForwardableMouseEventForwarder;
        } else {
            return GetNextParentEventForwarder(searchParent.parent);
        }
    }
    bool stackOverflowExecutionAvoid = false;
    private bool AvoidStackOverflow(){
        bool alreadyExecuting = stackOverflowExecutionAvoid;
        stackOverflowExecutionAvoid = true;
        return alreadyExecuting;
    }
    private void StackDone(){
        stackOverflowExecutionAvoid = false;
    }
    public void OnPointerClick(PointerEventData eventData)
    {
        if (AvoidStackOverflow() || (eventData.pointerClick == tgtGameObject && tgtGameObject != null)) return;
        if (forwardToNextAcceptingParent){
            MouseEventForwarder parentForwarder = GetNextParentEventForwarder(transform.parent);
            if (parentForwarder != null) parentForwarder.SendMessage("OnPointerClick", eventData, SendMessageOptions.DontRequireReceiver);//parentForwarder.OnPointerClick(eventData);
        } else if (tgtComponent != null){
            ((IPointerClickHandler)tgtComponent)?.OnPointerClick(eventData);
        } else if (tgtGameObject != null){
            tgtGameObject.SendMessage("OnPointerClick", eventData, SendMessageOptions.DontRequireReceiver);
        }
        StackDone();
    }
    /*
    public void OnPointerEnter(PointerEventData eventData)
    {
        if (tgtComponent != null){
            ((IPointerEnterHandler)tgtComponent)?.OnPointerEnter(eventData);
        } else if (tgtGameObject != null){
            tgtGameObject.SendMessage("OnPointerEnter", eventData, SendMessageOptions.DontRequireReceiver);
        }
    }
    public void OnPointerExit(PointerEventData eventData)
    {
        if (tgtComponent != null){
            ((IPointerExitHandler)tgtComponent)?.OnPointerExit(eventData);
        } else if (tgtGameObject != null){
            tgtGameObject.SendMessage("OnPointerExit", eventData, SendMessageOptions.DontRequireReceiver);
        }
    }
    */
    public void OnBeginDrag(PointerEventData eventData)
    {
        if (AvoidStackOverflow() || (eventData.pointerClick == tgtGameObject && tgtGameObject != null)) return;
        if (forwardToNextAcceptingParent){
            MouseEventForwarder parentForwarder = GetNextParentEventForwarder(transform.parent);
            if (parentForwarder != null) parentForwarder.SendMessage("OnBeginDrag", eventData, SendMessageOptions.DontRequireReceiver);//parentForwarder.OnBeginDrag(eventData);
        } else if (tgtComponent != null){
            ((IBeginDragHandler)tgtComponent)?.OnBeginDrag(eventData);
        } else if (tgtGameObject != null){
            tgtGameObject.SendMessage("OnBeginDrag", eventData, SendMessageOptions.DontRequireReceiver);
        }
        StackDone();
    }
    public virtual void OnDrag(PointerEventData eventData){
        if (AvoidStackOverflow()) return;
        if (forwardToNextAcceptingParent){
            MouseEventForwarder parentForwarder = GetNextParentEventForwarder(transform.parent);
            if (parentForwarder != null) parentForwarder.SendMessage("OnDrag", eventData, SendMessageOptions.DontRequireReceiver);//parentForwarder.OnDrag(eventData);
        } else if (tgtComponent != null){
            ((IDragHandler)tgtComponent)?.OnDrag(eventData);
        } else if (tgtGameObject != null){
            tgtGameObject.SendMessage("OnDrag", eventData, SendMessageOptions.DontRequireReceiver);
        }
        StackDone();
    }
    public virtual void OnEndDrag(PointerEventData eventData){
        if (AvoidStackOverflow()) return;
        if (forwardToNextAcceptingParent){
            MouseEventForwarder parentForwarder = GetNextParentEventForwarder(transform.parent);
            if (parentForwarder != null) parentForwarder.SendMessage("OnEndDrag", eventData, SendMessageOptions.DontRequireReceiver);//parentForwarder.OnEndDrag(eventData);
        } else if (tgtComponent != null){
            ((IEndDragHandler)tgtComponent)?.OnEndDrag(eventData);
        } else if (tgtGameObject != null){
            tgtGameObject.SendMessage("OnEndDrag", eventData, SendMessageOptions.DontRequireReceiver);
        }
        StackDone();
    }
}

 