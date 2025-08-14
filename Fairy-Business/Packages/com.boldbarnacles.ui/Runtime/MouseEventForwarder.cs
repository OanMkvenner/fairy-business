using UnityEngine.UI;
using UnityEngine;
using UnityEngine.EventSystems;
using Sirenix.OdinInspector;

// Attach this script to a GameObject with a Collider, then mouse over the object to see your cursor change.
public class MouseEventForwarder : MonoBehaviour, IPointerClickHandler,/* IPointerEnterHandler, IPointerExitHandler ,*/ IBeginDragHandler, IEndDragHandler, IDragHandler 
{
    [Tooltip("If we want to send it to all components of the target gameobject")]
    [HideIf("tgtComponent", null)]
    public GameObject tgtGameObject = null;
    [Tooltip("if we only want to send it to a specific component")]
    [HideIf("tgtGameObject", null)]
    public MonoBehaviour tgtComponent = null;
    
    public void OnPointerClick(PointerEventData eventData)
    {
        if (tgtComponent != null){
            ((IPointerClickHandler)tgtComponent)?.OnPointerClick(eventData);
        } else if (tgtGameObject != null){
            tgtGameObject.SendMessage("OnPointerClick", eventData, SendMessageOptions.DontRequireReceiver);
        }
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
        if (tgtComponent != null){
            ((IBeginDragHandler)tgtComponent)?.OnBeginDrag(eventData);
        } else if (tgtGameObject != null){
            tgtGameObject.SendMessage("OnBeginDrag", eventData, SendMessageOptions.DontRequireReceiver);
        }
    }
    public virtual void OnDrag(PointerEventData eventData){
        if (tgtComponent != null){
            ((IDragHandler)tgtComponent)?.OnDrag(eventData);
        } else if (tgtGameObject != null){
            tgtGameObject.SendMessage("OnDrag", eventData, SendMessageOptions.DontRequireReceiver);
        }
    }
    public virtual void OnEndDrag(PointerEventData eventData){
        if (tgtComponent != null){
            ((IEndDragHandler)tgtComponent)?.OnEndDrag(eventData);
        } else if (tgtGameObject != null){
            tgtGameObject.SendMessage("OnEndDrag", eventData, SendMessageOptions.DontRequireReceiver);
        }
    }
}

 