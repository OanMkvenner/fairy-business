using UnityEngine.UI;
using UnityEngine;
using UnityEngine.EventSystems;

// Attach this script to a GameObject with a Collider, then mouse over the object to see your cursor change.
public class MousePointerChanger : MonoBehaviour, IDragHandler, IEndDragHandler, IPointerEnterHandler, IPointerExitHandler
{
    public Image highlightingTest = null;
    private Material previousMaterial = null;

    public MousePointerChangerSO mousePointerChangerSO;
    bool currentlyHighlighting = false;
    bool mouseCurrentlyInside = false;
    public void OnPointerEnter(PointerEventData eventData)
    {
        mouseCurrentlyInside = true;
        StartHighlighting();
    }
    public void UpdateCursor(){
        if (currentlyHighlighting) {
            Cursor.SetCursor(mousePointerChangerSO.cursorTexture, mousePointerChangerSO.hotSpot, mousePointerChangerSO.cursorMode);
        } else {
            // Pass 'null' to the texture parameter to use the default system cursor.
            Cursor.SetCursor(null, Vector2.zero, mousePointerChangerSO.cursorMode);
        }
    }
    public void StartHighlighting(){
        if (!currentlyHighlighting){
            currentlyHighlighting = true;
            UpdateCursor();
            if (highlightingTest != null && previousMaterial == null){
                previousMaterial = highlightingTest.material;
                Material copiedMaterial = new Material(previousMaterial);   /// same as instantiate AFAIK
                highlightingTest.material = copiedMaterial;
                copiedMaterial.SetFloat("_highlighting", 1.0f);
            }
        }
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        mouseCurrentlyInside = false;
        StopHighlighting();
    }
    public void StopHighlighting(){
        if (!currentlyDraggin){
            currentlyHighlighting = false;
            UpdateCursor();
            if (highlightingTest != null && previousMaterial != null){
                highlightingTest.material = previousMaterial;
                previousMaterial = null;
            }
        }
    }
    bool currentlyDraggin = false;
    public virtual void OnDrag(PointerEventData eventData){
        currentlyDraggin = true;
        StartHighlighting();
    }
    public virtual void OnEndDrag(PointerEventData eventData){
        currentlyDraggin = false;
        if (currentlyHighlighting && !mouseCurrentlyInside) StopHighlighting();
        UpdateCursor();
    }
}

 