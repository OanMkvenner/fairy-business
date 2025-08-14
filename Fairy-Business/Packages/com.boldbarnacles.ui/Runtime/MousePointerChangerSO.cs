using UnityEngine;
using UnityEngine.EventSystems;

[CreateAssetMenu(fileName = "MousePointerChangerSO", menuName = "MousePointerChangerSO", order = 0)]
public class MousePointerChangerSO : ScriptableObject {
    public Texture2D cursorTexture;
    public CursorMode cursorMode = CursorMode.Auto;
    public Vector2 hotSpot = Vector2.zero;
}

 