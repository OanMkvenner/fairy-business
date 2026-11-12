using System;
using System.Collections.Generic;
using Freya;
using Unity.Burst;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;
using static UnityEngine.InputSystem.InputAction;

[BurstCompile]
public class ControlsSingleton : MonobehaviourSingletonCustom<PlayerInput> {
    // instance in this case will return the (first?) instance of PlayerInput
    static public Vector2 GetMouseScreenPosition(){
        return ControlsSingleton.instance.actions["Point"].ReadValue<Vector2>();
    }
    // get mouse position within the given rect (returns Vector2 instead of Vector3)
    static public Vector2 GetMousePosLocal2D(RectTransform rect, Camera overrideCam = null)
    {
        return GetMousePosLocal(rect, overrideCam).XY();
    }
    // get mouse position within the given rect
    static public Vector3 GetMousePosLocal(RectTransform rect, Camera overrideCam = null)
    {
        return GetLocalPosFromScreenPos(rect, GetMouseScreenPosition(), overrideCam);
    }
    
    // get local position within the given rect from given screen position
    // screenPos axis: BottomRight of screen is (0,0) - TopRight of screen is (Screen.width, Screen.height)
    static public Vector3 GetLocalPosFromScreenPos(RectTransform rect, Vector2 screenPos, Camera camera = null){
        camera ??= Camera.main; // use default cam if not specified otherwise
        RectTransformUtility.ScreenPointToLocalPointInRectangle(rect, screenPos, camera, out Vector2 result);
        return new Vector3(result.x, result.y, 0);
    }
    // get screen position from given local position of rectTransform
    static public Vector2 GetScreenPosFromLocalPos(RectTransform rect, Vector3 localPos, Camera camera = null){
        camera ??= Camera.main; // use default cam if not specified otherwise
        return RectTransformUtility.WorldToScreenPoint(camera, rect.TransformPoint(localPos));
    }
    // move a screenspace position along localVec within local space of rect. Value is returned in screenspace again.
    static public Vector2 MoveScreenPosAlongLocalVec(RectTransform rect, Vector2 screenPos, Vector3 localVec, Camera camera = null){
        camera ??= Camera.main; // use default cam if not specified otherwise
        var localPos = ControlsSingleton.GetLocalPosFromScreenPos(rect, screenPos, camera);
        localPos += localVec;
        return GetScreenPosFromLocalPos(rect, localPos, camera);
    }

    // takes a localposition of a source recttransform and transforms it into the localPos of the plane of the target Transform (height = 0),
    // but not based on its world position but on its position on the screen!
    static public Vector3 TransformLocalPosToLocalPosByScreenPos(RectTransform srcRect, RectTransform tgtRect, Vector3 srcLocalPos, Camera camera = null) {
        var screenPos = GetScreenPosFromLocalPos(srcRect, srcLocalPos, camera);
        return GetLocalPosFromScreenPos(tgtRect, screenPos, camera);
    }

    //[BurstCompile]
    static public float3 GetMousePositionExact3D(){
        return GetMousePositionExact3D(out var _);
    }
    static public Ray GetMouseRay(){
        var mouseScreenPosition = GetMouseScreenPosition();
        return Camera.main.ScreenPointToRay(mouseScreenPosition);
    }
    //[BurstCompile]
    static public float3 GetMousePositionExact3D(out Ray mouseRay, float zDepth = 0f, Plane? overridePlane = null){
        //var camSystem = GameController.ClientWorld.GetExistingSystemManaged<CameraSystem>();
        mouseRay = GetMouseRay();
        var mouseWorldPosition = Utilities.GetWorldPosFromScreenRayAtZDepth(mouseRay, zDepth, overridePlane);
        return mouseWorldPosition;
    }
    //[BurstCompile]
    static public float2 GetMousePositionExact(){
        return GetMousePositionExact3D(out var _).xy;
    }
    //[BurstCompile]
    static public int2 GetMouseBlockPos(){
        var mousePos = GetMousePositionExact();
        return new int2((int)(mousePos.x + 0.5f), (int)(mousePos.y + 0.5f));
    }
    static public float2 GetMousePositionExact(out Ray mouseRay){
        return GetMousePositionExact3D(out mouseRay).xy;
    }

    static Dictionary<string, DragDropManager> dragDropManagers = new();
    static public DragDropManager GetDragDropManager(string actionName){
        dragDropManagers.Clear();
        if (dragDropManagers.ContainsKey(actionName)) return dragDropManagers[actionName];
        else {
            if (!Application.isPlaying) {
                Debug.LogError("only allowed while game is running!");
                return null;
            }
            var newDragDropManager = DragDropManager.Create(actionName);
            dragDropManagers[actionName] = newDragDropManager;
            return newDragDropManager;
        }
    }

}

public class DragDropManager
{
    public Vector2 mouseDragStartedPos = new(0, 0);
    public Vector2 mouseCurrentPos = new(0, 0);
    public Vector2 mousePreviousPos = new(0,0);

    bool tryStartDragging = false;
    bool isDragging = false;
    string actionName = "";
    public static DragDropManager Create(string actionName){
        var newManager = new DragDropManager();
        newManager.actionName = actionName;
        ControlsSingleton.instance.actions[actionName].Enable();
        if (ControlsSingleton.instance.actions[actionName].type == InputActionType.PassThrough){
            ControlsSingleton.instance.actions[actionName].performed += (ctx) => newManager.PassthroughCheck(ctx);
        } else {
            ControlsSingleton.instance.actions[actionName].started += (ctx) => newManager.TryStartDrag(ctx);
            ControlsSingleton.instance.actions[actionName].canceled += (ctx) => newManager.CancelDrag(ctx);
        }
        ControlsSingleton.instance.actions["Point"].performed += (ctx) => newManager.UpdateDragging(ctx);
        
        return newManager;
    }

    bool wasPressed = false;
    private void PassthroughCheck(CallbackContext ctx)
    {
        bool isPressed = ctx.ReadValueAsButton();
        if (!wasPressed && isPressed){
            wasPressed = true;
            TryStartDrag(ctx);
        }
        if (wasPressed && !isPressed){
            wasPressed = false;
            CancelDrag(ctx);
        }
    }

    private void TryStartDrag(CallbackContext ctx){
        tryStartDragging = true;
        mouseDragStartedPos = ControlsSingleton.GetMouseScreenPosition();
        mouseCurrentPos = mouseDragStartedPos;
    }
    static float dragMoveDelta = 10.5f;
    private void UpdateDragging(CallbackContext ctx){
        if (tryStartDragging){
            if (!isDragging){
                if ((mouseDragStartedPos - ControlsSingleton.GetMouseScreenPosition()).magnitude > dragMoveDelta){
                    isDragging = true;
                    OnStartDrag?.Invoke(ctx);
                }
            }
            if (isDragging){
                mousePreviousPos = mouseCurrentPos;
                mouseCurrentPos = ControlsSingleton.GetMouseScreenPosition();
                OnDrag?.Invoke(ctx);
            }
        }
    }
    private void CancelDrag(CallbackContext ctx){
        if (isDragging){
            mousePreviousPos = mouseCurrentPos;
            mouseCurrentPos = ControlsSingleton.GetMouseScreenPosition();
            OnDrag?.Invoke(ctx);
            OnEndDrag?.Invoke(ctx);
        }
        isDragging = false;
        tryStartDragging = false;
    }
    
    public UnityAction<CallbackContext> OnStartDrag = null;
    public UnityAction<CallbackContext> OnDrag = null;
    public UnityAction<CallbackContext> OnEndDrag = null;

    //private void StartDrag(CallbackContext ctx){}
    //private void OnDrag(CallbackContext ctx){}
    //private void EndDrag(CallbackContext ctx){}
}