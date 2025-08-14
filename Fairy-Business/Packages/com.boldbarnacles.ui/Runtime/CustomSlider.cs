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


[AddComponentMenu("Custom UI/Custom Slider", 30)]
[RequireComponent(typeof(RectTransform), typeof(UiElementSounds))]
/// <summary>
/// A standard slider that can be moved between a minimum and maximum value.
/// </summary>
/// <remarks>
/// The slider component is a Selectable that controls a fill, a handle, or both. The fill, when used, spans from the minimum value to the current value while the handle, when used, follow the current value.
/// The anchors of the fill and handle RectTransforms are driven by the Slider. The fill and handle can be direct children of the GameObject with the Slider, or intermediary RectTransforms can be placed in between for additional control.
/// When a change to the slider value occurs, a callback is sent to any registered listeners of UI.Slider.onValueChanged.
/// </remarks>
public class CustomSlider : UIBehaviour, ICanvasElement, IDataReceiver
{
    [Serializable]
    /// <summary>
    /// Event type used by the UI.Slider.
    /// </summary>
    public class SliderEvent : UnityEvent<float> {}

    /// <summary>
    /// Graphic the toggle should be working with.
    /// </summary>
	[FoldoutGroup("Setup")]
    [OnValueChanged("UpdateChangedParemeters")]
    public RectTransform interactionHandle; // REQUIRED! can be the same as one of the images, can be itself, OR can be any different RectTransform gameobject
	[FoldoutGroup("Setup")]
    public RectTransform fillingRect;
	[FoldoutGroup("Setup")]
    public bool rotateableLever = false;
	[FoldoutGroup("Setup")]
    [ShowIf("rotateableLever")]
    public GameObject leverArm;  // optional, if existant it can be rotated
	[FoldoutGroup("Setup")]
    [ShowIf("rotateableLever")]
    public float3 minRotation = new float3 (0, 30, 0);
	[FoldoutGroup("Setup")]
    [ShowIf("rotateableLever")]
    public float3 maxRotation = new float3 (0, 150, 0);

	[FoldoutGroup("Setup")]
    [Tooltip("The higher this is, the harder it is to move the lever in general. default value: 200")]
    [Range(10, 2000)]
    public float inputDampener = 200f; //  default: 200f


    [SerializeField]
    [ShowIf("@fillingRect?.GetComponent<UnityEngine.UI.Image>() != null")]
    [OnValueChanged("ChangeBarColor")]
    public Color barColor;

    [SerializeField]
    [OnValueChanged("UpdateVisuals")]
    protected float m_Value;
    public virtual float value
    {
        get
        {
            return m_Value;
        }
        set
        {
            Set(value);
        }
    }
    [HorizontalGroup(LabelWidth = 100)]
    public bool useOffset = false;
    [SerializeField]
    [OnValueChanged("UpdateVisuals")]
    [ShowIf("useOffset")]
    [HorizontalGroup]
    protected float m_OffsetValue;
    public virtual float offsetValue
    {
        get
        {
            return m_OffsetValue;
        }
        set
        {
            SetOffset(value);
        }
    }


    [SerializeField]
    private float m_MinValue = 0;
    public float minValue { get { return m_MinValue; } set { if (UtilitiesUi.SetPropertyUtility.SetStruct(ref m_MinValue, value)) { Set(m_Value); UpdateVisuals(); } } }

    [SerializeField]
    private float m_MaxValue = 1;
    public float maxValue { get { return m_MaxValue; } set { if (UtilitiesUi.SetPropertyUtility.SetStruct(ref m_MaxValue, value)) { Set(m_Value); UpdateVisuals(); } } }



    public void UpdateChangedParemeters(){
        UpdateCachedReferences();
        Set(m_Value);
        UpdateVisuals();
    }

    public virtual void SetValueWithoutNotify(float input)
    {
        Set(input, false);
    }
    public float normalizedValue
    {
        get
        {
            if (Mathf.Approximately(minValue, maxValue))
                return 0;
            return Mathf.InverseLerp(minValue, maxValue, value);
        }
        set
        {
            this.value = Mathf.Lerp(minValue, maxValue, value);
        }
    }
    public float normalizedOffsetValue
    {
        get
        {
            if (Mathf.Approximately(minValue, maxValue))
                return 0;
            return Mathf.InverseLerp(minValue, maxValue, offsetValue);
        }
        set
        {
            this.offsetValue = Mathf.Lerp(minValue, maxValue, value);
        }
    }

    [Space]

    [SerializeField]
	[FoldoutGroup("Events")]
    public SliderEvent m_OnValueChanged = new SliderEvent();

    private Transform m_HandleTransform;
    private RectTransform m_HandleContainerRect;

    // The offset from handle position to mouse down position
    private Vector2 m_Offset = Vector2.zero;

    // field is never assigned warning
    #pragma warning disable 649
    private DrivenRectTransformTracker m_Tracker;
    #pragma warning restore 649

    // This "delayed" mechanism is required for case 1037681.
    private bool m_DelayedUpdateVisuals = false;

    // Size of each step.
    float stepSize { get { return (maxValue - minValue) * 0.1f; } }

    protected CustomSlider()
    {}

#if UNITY_EDITOR
    protected override void OnValidate()
    {
        base.OnValidate();

        //Onvalidate is called before OnEnabled. We need to make sure not to touch any other objects before OnEnable is run.
        if (IsActive())
        {
            UpdateCachedReferences();
            // Update rects in next update since other things might affect them even if value didn't change.
            m_DelayedUpdateVisuals = true;
        }

        if (!UnityEditor.PrefabUtility.IsPartOfPrefabAsset(this) && !Application.isPlaying)
            CanvasUpdateRegistry.RegisterCanvasElementForLayoutRebuild(this);
    }

#endif // if UNITY_EDITOR

    public virtual void Rebuild(CanvasUpdate executing)
    {
    }

    /// <summary>
    /// See ICanvasElement.LayoutComplete
    /// </summary>
    public virtual void LayoutComplete()
    {}

    /// <summary>
    /// See ICanvasElement.GraphicUpdateComplete
    /// </summary>
    public virtual void GraphicUpdateComplete()
    {}

    protected override void OnEnable()
    {
        base.OnEnable();
        UpdateCachedReferences();
        Set(m_Value, false);
        // Update rects since they need to be initialized correctly.
        UpdateVisuals();
    }

    protected override void OnDisable()
    {
        m_Tracker.Clear();
        base.OnDisable();
    }
    private UiElementSounds sounds;
    protected override void Start()
    {
        ChangeBarColor();
        if (interactionHandle == null) {
            Debug.LogError("interactionSurface is null on this CustomButton. This means this button can never be called! please assign one", this);
            return;
        }
        sounds = GetComponent<UiElementSounds>();
        var interactionComponent = interactionHandle.GetComponent<CustomUiInteractionHandler>();
        if (interactionComponent == null)
        {
            interactionHandle.gameObject.AddComponent<CustomUiInteractionHandler>();
        }
        interactionComponent = interactionHandle.GetComponent<CustomUiInteractionHandler>();
        interactionComponent.onPointerDownEvent.AddListener(PointerDown);
        interactionComponent.onPointerUpEvent.AddListener(PointerUp);
        interactionComponent.onOnInitializePotentialDragEvent.AddListener(OnInitializePotentialDrag);
        interactionComponent.onDragEvent.AddListener(OnDrag);
        interactionComponent.onEndDragEvent.AddListener(OnEndDrag);
    }

    private void ChangeBarColor()
    {
        var fillRectImage = fillingRect?.GetComponent<Image>();
        if (fillRectImage) fillRectImage.color = barColor;
    }

    public void TakeNewData(float newValue){
        Set(newValue, false);
    }
    public void SendNewData(float newValue){
        var dataConnector = GetComponent<IDataConnector>();
        dataConnector?.TakeNewData(newValue);
    }

    /// <summary>
    /// Update the rect based on the delayed update visuals.
    /// Got around issue of calling sendMessage from onValidate.
    /// </summary>
    protected virtual void Update()
    {
        if (m_DelayedUpdateVisuals)
        {
            m_DelayedUpdateVisuals = false;
            Set(m_Value, false);
            UpdateVisuals();
        }
        UpdateNotDraggin();
    }

    protected override void OnDidApplyAnimationProperties()
    {
        // Has value changed? Various elements of the slider have the old normalisedValue assigned, we can use this to perform a comparison.
        // We also need to ensure the value stays within min/max.
        m_Value = ClampValue(m_Value);
        float oldNormalizedValue = normalizedValue;
        if (m_HandleContainerRect != null)
            oldNormalizedValue = interactionHandle.anchorMin[0];

        UpdateVisuals();

        if (oldNormalizedValue != normalizedValue)
        {
            UISystemProfilerApi.AddMarker("Slider.value", this);
            m_OnValueChanged.Invoke(m_Value);
        }
        base.OnDidApplyAnimationProperties();
    }

    void UpdateCachedReferences()
    {

        if (interactionHandle && interactionHandle != (RectTransform)transform)
        {
            m_HandleTransform = interactionHandle.transform;
            if (m_HandleTransform.parent != null)
                m_HandleContainerRect = m_HandleTransform.parent.GetComponent<RectTransform>();
        }
        else
        {
            interactionHandle = null;
            m_HandleContainerRect = null;
        }
    }

    float ClampValue(float input)
    {
        float newValue = Mathf.Clamp(input, minValue, maxValue);
        return newValue;
    }

    /// <summary>
    /// Set the value of the slider.
    /// </summary>
    /// <param name="input">The new value for the slider.</param>
    /// <param name="sendCallback">If the OnValueChanged callback should be invoked.</param>
    /// <remarks>
    /// Process the input to ensure the value is between min and max value. If the input is different set the value and send the callback is required.
    /// </remarks>
    protected virtual void Set(float input, bool sendCallback = true, bool forceUpdate = false)
    {
        // Clamp the input
        float newValue = ClampValue(input);

        // If the stepped value doesn't match the last one, it's time to update
        if (m_Value == newValue && !forceUpdate)
            return;

        m_Value = newValue;
        UpdateVisuals();
        if (sendCallback)
        {
            UISystemProfilerApi.AddMarker("Slider.value", this);
            m_OnValueChanged.Invoke(newValue);
            SendNewData(newValue);
        }
    }

    /// <summary>
    /// Set the value of the slider.
    /// </summary>
    /// <param name="input">The new value for the slider.</param>
    /// <param name="sendCallback">If the OnValueChanged callback should be invoked.</param>
    /// <remarks>
    /// Process the input to ensure the value is between min and max value. If the input is different set the value and send the callback is required.
    /// </remarks>
    protected virtual void SetOffset(float offsetInput, bool sendCallback = true, bool forceUpdate = false)
    {
        // Clamp the input
        float newValue = ClampValue(offsetInput);

        // If the stepped value doesn't match the last one, it's time to update
        if (m_OffsetValue == newValue && !forceUpdate)
            return;

        m_OffsetValue = newValue;
        UpdateVisuals();
        if (sendCallback)
        {
            UISystemProfilerApi.AddMarker("Slider.offsetValue", this);
            //m_OnValueChanged.Invoke(newValue);
            //SendNewData(newValue);
        }
    }

    protected override void OnRectTransformDimensionsChange()
    {
        base.OnRectTransformDimensionsChange();

        //This can be invoked before OnEnabled is called. So we shouldn't be accessing other objects, before OnEnable is called.
        if (!IsActive())
            return;

        UpdateVisuals();
    }

    enum Axis
    {
        Horizontal = 0,
        Vertical = 1
    }

    // Force-update the slider. Useful if you've changed the properties and want it to update visually.
    private void UpdateVisuals()
    {
#if UNITY_EDITOR
        if (!Application.isPlaying)
            UpdateCachedReferences();
#endif

        m_Tracker.Clear();

        var normalizedVal = normalizedValue;
        var normalizedOffsetVal = normalizedOffsetValue;
        if (rotateableLever && leverArm != null){
            var leverRot = leverArm.transform.localRotation.eulerAngles;
            leverRot = minRotation + normalizedVal * (maxRotation - minRotation);
            leverArm.transform.localRotation = Quaternion.Euler(leverRot);
            var rotNeutralizer = interactionHandle.GetComponent<RotationNeutralisation>();
            if (rotNeutralizer) rotNeutralizer.NeutralizeRotation();
        } else if (m_HandleContainerRect != null) {
            var parentRect = m_HandleContainerRect.GetComponent<RectTransform>();
            var newXPos =  parentRect.rect.size.x * normalizedVal;

            var handlePos = interactionHandle.anchoredPosition;
            handlePos.x = newXPos;
            interactionHandle.anchoredPosition = handlePos;
        }
        if (fillingRect != null) {
            var newAnchorMaxVal = new Vector2(normalizedVal, fillingRect.anchorMax.y);
            var newAnchroMinVal = new Vector2(normalizedOffsetVal, fillingRect.anchorMin.y);
            if (Application.isPlaying){
                Tween tween = fillingRect.DOAnchorMax(newAnchorMaxVal, 0.7f).SetEase(Ease.OutExpo).SetRelative(false).SetLink(fillingRect.gameObject);
                if (useOffset) {
                     Tween tweenMin = fillingRect.DOAnchorMin(newAnchroMinVal, 0.7f).SetEase(Ease.OutExpo).SetRelative(false).SetLink(fillingRect.gameObject);
                }
            } else {
                fillingRect.anchorMax = newAnchorMaxVal;
                if (useOffset) fillingRect.anchorMin = newAnchroMinVal;
            }

        }
    }

    float currentSpeed = 0;
    void UpdateNotDraggin(){
        bool isInFinalPosition = false; // dont do anything if we have reached any stable position
        if (normalizedValue <= 0 || normalizedValue >= 1.0f) isInFinalPosition = true;
        if (!currentlyDraggin && !isInFinalPosition && hasActiveResistanceForces){
            float dt = Time.deltaTime;
            var frameGravity = gravityStrength * dt;
            float directionModifier = 1f;
            if (normalizedValue < gravityMidpoint) directionModifier = -1f;
            currentSpeed += directionModifier * frameGravity / 100;
            float newVal = normalizedValue + currentSpeed * dt;
            if (newVal > 1.0f) newVal = 1.0f;
            if (newVal < 0) newVal = 0;
            normalizedValue = newVal;
        } else {
            currentSpeed = 0;
        }
    }

    [FoldoutGroup("Forces")]
    public bool hasResistanceForces = false;
    [ShowIf("hasResistanceForces")]
    [FoldoutGroup("Forces")]
    public bool hasActiveResistanceForces = false;
    [ShowIf("hasResistanceForces")]
    [FoldoutGroup("Forces")]
    public float gravityMidpoint = 0.3f;
    [ShowIf("hasResistanceForces")]
    [FoldoutGroup("Forces")]
    public float gravityStrength = 10f;
    // Update the slider's position based on the mouse.
    [NonSerialized]
    public Vector2 warpOffset = new(0,0);
    void UpdateDrag(PointerEventData eventData, Camera cam)
    {
        //RectTransform clickRect = m_HandleContainerRect;
        RectTransform clickRect = interactionHandle;
        if (clickRect != null && clickRect.rect.size[0] > 0)
        {
            // find screen position for drag
            Vector2 position = Vector2.zero;
            if (!UtilitiesUi.MultipleDisplayUtilities.GetRelativeMousePositionForDrag(eventData, ref position))
                return;
            // and remove the warp-offset from our last mousewarp (we dont want to re-input that into our calculations)
            //position -= warpOffset;
            var positionDelta = eventData.delta - warpOffset;
            var modifiedWorldPos = interactionHandle.TransformPoint(new Vector3(m_Offset.x, m_Offset.y, 0));
            var handleScreenPos = RectTransformUtility.WorldToScreenPoint(cam, modifiedWorldPos);
            
            var testPosition = handleScreenPos + positionDelta;

            // find mouse-position relative to interaction-handle. if dragging is performed into corre
            Vector2 localCursor;
            if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(clickRect, testPosition, cam, out localCursor))
                return;
            localCursor -= m_Offset;
            float mouseMoveValue = localCursor.x;

            // use mouseMoveValue and several modifyers to decide how far you managed to move the slider, relative to all forces
            // localCursor.x is basically already per deltaTime
            float dt = Time.deltaTime;
            //float directionModifier = 1f;
            //if (normalizedValue < gravityMidpoint) directionModifier = -1f;

            float finalDampening = inputDampener;
            if (hasResistanceForces){
                // if normalizedValue is within range of gravityMidpoint forces. calculations taken from a selfmade custom function https://www.desmos.com/calculator/4vzpheiqjk
                var gravforceMult = 0f;
                if (normalizedValue - 0.45482f < gravityMidpoint && normalizedValue + 0.45482f > gravityMidpoint){
                    var justX = normalizedValue - gravityMidpoint;
                    var funcX = 5f * (normalizedValue - gravityMidpoint);
                    gravforceMult = 5.73f * (1f/(1f+math.pow(7f,-funcX))-0.5f)/(math.square(funcX)+1f)-justX;
                };

                if (gravforceMult != 0){
                    // if mouse move direction is the same as the gravBasedForce is pushing
                    if (mouseMoveValue * gravforceMult > 0f){
                        // make moving easier by reducing inputDampener down to 0.5x
                        finalDampening = inputDampener * math.lerp(1.0f, 0.5f, math.abs(gravforceMult));
                    } else {
                        // otherwise make moving harder by adding up to gravityStrength to the inputDampener
                        finalDampening = inputDampener + math.lerp(0f, gravityStrength, math.abs(gravforceMult));
                    }
                }
            }
            //Debug.LogError(finalDampening);
            // by adding the original dampening 
            float val = mouseMoveValue / finalDampening + normalizedValue;

            normalizedValue = val; // assigning value (and thus, updating visuals automatically right here)

            // old way of doing DIRECT input depending on where you had your mousepointer. I might still implement a variant of this as an option for CustomSlider (to allow the old behaviour again)
            //var unmodifiedLocalCursor = localCursor;
            //localCursor -= clickRect.rect.position;
            //float val = Mathf.Clamp01((localCursor - m_Offset)[0] / clickRect.rect.size[0]);
            //unmodifiedLocalCursor.y = 0;
            //unmodifiedLocalCursor.x = Mathf.Clamp(unmodifiedLocalCursor.x, m_Offset.x - clickRect.rect.size.x / 2, m_Offset.x + clickRect.rect.size.x / 2);
            //var modifiedWorldPos = clickRect.TransformPoint(unmodifiedLocalCursor);

            // jump the cursor back to the origin of the handle (it is hidden already, but if the hidden cursor hovers over a highlightable, it lights up, which looks 
            // like a bug and is distracting) need to round values BEFORE calculating warpOffset, because WarpCursorPosition() rounds them later, but without notifying us.
            handleScreenPos.x = Mathf.FloorToInt(handleScreenPos.x);
            handleScreenPos.y = Mathf.FloorToInt(handleScreenPos.y);

            // warpOffset will get calculated out of next drag-operation mouse-move
            warpOffset = handleScreenPos - position;
            // JUMP!
            Mouse.current.WarpCursorPosition(handleScreenPos);
        }
    }

    private bool MayDrag(PointerEventData eventData)
    {
        return IsActive() /*&& IsInteractable()*/ && eventData.button == PointerEventData.InputButton.Left;
    }

    bool currentlyDraggin = false;
    public void PointerDown(PointerEventData eventData)
    {
        if (!MayDrag(eventData))
            return;

        if (m_HandleContainerRect != null && RectTransformUtility.RectangleContainsScreenPoint(interactionHandle, eventData.pointerPressRaycast.screenPosition, eventData.enterEventCamera))
        {
            StartDragging(eventData);
        }
        else
        {
            // Outside the slider handle - jump to this point instead
            UpdateDrag(eventData, eventData.pressEventCamera);
        }
    }
    public void StartDragging(PointerEventData eventData){
        m_Offset = Vector2.zero;
        currentlyDraggin = true;
        //UnityEngine.Cursor.lockState = UnityEngine.CursorLockMode.Locked;
        UnityEngine.Cursor.visible = false; //hide cursor

        Vector2 localMousePos;
        if (RectTransformUtility.ScreenPointToLocalPointInRectangle(interactionHandle, eventData.pointerPressRaycast.screenPosition, eventData.pressEventCamera, out localMousePos)){
            m_Offset = localMousePos;
            warpOffset = new(0,0);
        }
    }
    public void StopDragging(PointerEventData eventData)
    {
        if (currentlyDraggin){
            currentlyDraggin = false;
            var modifiedWorldPos = interactionHandle.TransformPoint(new Vector3(m_Offset.x, m_Offset.y,0));
            var newScreenPos = RectTransformUtility.WorldToScreenPoint(eventData.pressEventCamera, modifiedWorldPos);
            Mouse.current.WarpCursorPosition(newScreenPos);
            UnityEngine.Cursor.visible = true; //show cursor
        }
    }
    public void PointerUp(PointerEventData eventData)
    {
        StopDragging(eventData);
    }
    public virtual void OnInitializePotentialDrag(PointerEventData eventData)
    {
        eventData.useDragThreshold = false;
    }
    public virtual void OnDrag(PointerEventData eventData)
    {
        if (!MayDrag(eventData))
            return;
        UpdateDrag(eventData, eventData.pressEventCamera);
    }
    public virtual void OnEndDrag(PointerEventData eventData){
        StopDragging(eventData);
    }

}