using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.Serialization;
using UnityEngine;
using UnityEngine.UI;
using System;
using Sirenix.OdinInspector;
using System.Collections.Generic;
using TMPro;
using DG.Tweening;

/// <summary>
/// A custom Button that allows for several ways of working
/// </summary>
/// <remarks>
/// 
/// </remarks>
[AddComponentMenu("Custom UI/Custom Button", 30)]
[RequireComponent(typeof(RectTransform), typeof(UiElementSounds))]
public class CustomButton : UIBehaviour 
#if (HYBR_NODE_EDITOR)
, IAssignViewNodePortConnecter 
#endif
{
    public enum ButtonType
    {
        ClickButton,
        ToggleButton,
    }

    /// <summary>
    /// Display settings for when a toggle is activated or deactivated.
    /// </summary>
    public enum ToggleTransition
    {
        /// <summary>
        /// Show / hide the toggle instantly
        /// </summary>
        None,

        /// <summary>
        /// Fade the toggle in / out smoothly.
        /// </summary>
        FadeUnimplemented
    }

    [Serializable]
    /// <summary>
    /// UnityEvent callback for when a toggle is toggled.
    /// </summary>
    public class ToggleEvent : UnityEvent<bool>
    {}

    private UiElementSounds sounds;

    [OnValueChanged("AssignViewNodePortConnecter")]
    public bool toggleableButton = false;
    [ShowIf("toggleableButton")]
    public bool showEnabledImmediately = false;

    [OnValueChanged("ButtonTextChanged")]
    public string buttonText = "";
    [OnValueChanged("ButtonTextChanged")]
    [ShowIf("toggleableButton")]
    public string buttonEnabledText = "";

    /// <summary>
    /// Transition mode for the toggle.
    /// </summary>
	[FoldoutGroup("Setup")]
    public ToggleTransition toggleTransition = ToggleTransition.FadeUnimplemented;

    /// <summary>
    /// Graphic the toggle should be working with.
    /// </summary>
	[FoldoutGroup("Setup")]
    public GameObject interactionSurface; // REQUIRED! can be the same as one of the images, can be itself, OR can be any different RectTransform gameobject
	[FoldoutGroup("SetupElements")]
    public Graphic disabledGraphic;  // only shown while toggle is disabled. can also be part of the Background image though
	[FoldoutGroup("SetupElements")]
    public Graphic enabledGraphic;  // only shown while toggle is enabled
	[FoldoutGroup("SetupElements")]
    public Graphic depressedGraphic; // only shown while the mouse is held down on the button. If not existing: switches to enabled graphic immediately
    
	[FoldoutGroup("SetupElements")]
    public TMP_Text imgDisabledTextfield;
	[FoldoutGroup("SetupElements")]
    public TMP_Text imgEnabledTextfield;
	[FoldoutGroup("SetupElements")]
    public TMP_Text imgDeepPressedTextfield;

    [OnValueChanged("GroupValueChanged")]
	[FoldoutGroup("Setup")]
    public CustomButtonGroup group;
    [SerializeField]
    [HideInInspector]
    private CustomButtonGroup m_Group;

    
    // Whether the toggle is on
    [Tooltip("Is the toggle currently on or off?")]
    [ShowIf("toggleableButton")]
	[FoldoutGroup("Events")]
    [ReadOnly]
    [SerializeField]
    private bool m_IsOn;
	[FoldoutGroup("Events")]
    public UnityEvent onClickActivate = new UnityEvent();
    [ShowIf("toggleableButton")]
	[FoldoutGroup("Events")]
    public UnityEvent oReleaseIntoToggled = new UnityEvent();
    [ShowIf("toggleableButton")]
	[FoldoutGroup("Events")]
    public UnityEvent onClickWhileToggled = new UnityEvent();
	[FoldoutGroup("Events")]
    public UnityEvent onReleaseDeactivate = new UnityEvent();

	[FoldoutGroup("Events")]
    public ToggleEvent onValueChanged = new ToggleEvent();
	[FoldoutGroup("Events")]
    public UnityEvent onChangedOn = new UnityEvent();
	[FoldoutGroup("Events")]
    public UnityEvent onChangedOff = new UnityEvent();


    protected CustomButton()
    {}


    public virtual void LayoutComplete()
    {}

    public virtual void GraphicUpdateComplete()
    {}

    public void ButtonTextChanged(){

        if (imgDisabledTextfield) imgDisabledTextfield.text = buttonText;
        if (imgDeepPressedTextfield) imgDeepPressedTextfield.text = buttonText;
        if (buttonEnabledText == ""){
            if (imgEnabledTextfield) imgEnabledTextfield.text = buttonText;
        } else {
            if (imgEnabledTextfield) imgEnabledTextfield.text = buttonEnabledText;
        }
    }
    public void SetButtonText(string newButtonText, string newEnabledButtonText = ""){
        buttonText = newButtonText;
        buttonEnabledText = newEnabledButtonText;
        
        ButtonTextChanged();
    }

    public void SetDisabledButtonText(){

    }

    private void GroupValueChanged(){
        SetToggleGroup(group, true);
        PlayEffect(true);
    }
    private void SetToggleGroup(CustomButtonGroup newGroup, bool setMemberValue)
    {
        // Sometimes IsActive returns false in OnDisable so don't check for it.
        // Rather remove the toggle too often than too little.
        if (m_Group != null)
            m_Group.UnregisterToggle(this);

        // At runtime the group variable should be set but not when calling this method from OnEnable or OnDisable.
        // That's why we use the setMemberValue parameter.
        if (setMemberValue)
            m_Group = newGroup;

        // Only register to the new group if this Toggle is active.
        if (newGroup != null && IsActive())
            newGroup.RegisterToggle(this);

        // If we are in a new group, and this toggle is on, notify group.
        // Note: Don't refer to m_Group here as it's not guaranteed to have been set.
        if (newGroup != null && isOn && IsActive())
            newGroup.NotifyToggleOn(this);
    }

    public bool isOn
    {
        get { return m_IsOn; }

        set
        {
            Set(value);
        }
    }

    /// <summary>
    /// Set isOn without invoking onValueChanged callback.
    /// </summary>
    /// <param name="value">New Value for isOn.</param>
    public void SetIsOnWithoutNotify(bool value)
    {
        Set(value, false);
    }

    public void Set(bool value, bool sendCallback = true)
    {
        if (m_IsOn == value && sendCallback == false)
            return;

        // if we are in a group and set to true, do group logic
        m_IsOn = value;
        if (m_Group != null && m_Group.isActiveAndEnabled && IsActive())
        {
            if (m_IsOn || (!m_Group.AnyTogglesOn() && !m_Group.allowSwitchOff))
            {
                m_IsOn = true;
                m_Group.NotifyToggleOn(this, sendCallback);
            }
        }

        // Always send event when toggle is clicked, even if value didn't change
        // due to already active toggle in a toggle group being clicked.
        // Controls like Dropdown rely on this.
        // It's up to the user to ignore a selection being set to the same value it already was, if desired.
        PlayEffect(toggleTransition == ToggleTransition.None);
        if (sendCallback)
        {
            UISystemProfilerApi.AddMarker("Toggle.value", this);
            SendToggleCallbacks();
        }
    }
    void SendToggleCallbacks(){
        onValueChanged.Invoke(m_IsOn);
        if (m_IsOn){
            onChangedOn.Invoke();
        } else {
            onChangedOff.Invoke();
        }
    }
    public virtual void Rebuild(CanvasUpdate executing)
    {
#if UNITY_EDITOR
#endif
    }

    /// <summary>
    /// Play the appropriate effect.
    /// </summary>
    private void PlayEffect(bool instant)
    {
#if UNITY_EDITOR
        if (!Application.isPlaying){
            if (enabledGraphic != null && enabledGraphic.GetComponent<Image>().sprite != null
                && showEnabledImmediately) {
                enabledGraphic.gameObject.SetActive(m_IsOn);
                //enabledGraphic.canvasRenderer.SetAlpha(m_IsOn ? 1f : 0f);
            }
            if (disabledGraphic != null && disabledGraphic.GetComponent<Image>().sprite != null) {
                disabledGraphic.gameObject.SetActive(!m_IsOn);
                //disabledGraphic.canvasRenderer.SetAlpha(m_IsOn ? 0f : 1f);
            }
        } else {
#endif
            if (enabledGraphic != null && enabledGraphic.GetComponent<Image>().sprite != null
                && showEnabledImmediately) {
                enabledGraphic.gameObject.SetActive(m_IsOn);
                //enabledGraphic.CrossFadeAlpha(m_IsOn ? 1f : 0f, instant ? 0f : 0.1f, true);
            }
            if (disabledGraphic != null && disabledGraphic.GetComponent<Image>().sprite != null)  {
                disabledGraphic.gameObject.SetActive(!m_IsOn);
                //disabledGraphic.CrossFadeAlpha(m_IsOn ? 0f : 1f, instant ? 0f : 0.1f, true);
            }
#if UNITY_EDITOR
        }
#endif
    }

    // *** ViewNodePortConnector auto-setup ***
    public void AssignViewNodePortConnecter(){
#if (HYBR_NODE_EDITOR)
		var viewNodePortConnector = GetComponent<ViewNodePortConnector>();
		if (viewNodePortConnector != null){
            List<string> portNameEndings = new();
            if (toggleableButton){
                portNameEndings.Add("_On");
                portNameEndings.Add("_Off");
            }
            viewNodePortConnector.AssignPortNameEndings(portNameEndings);
            //onClickActivate.AddListener(new UnityAction(viewNodePortConnector.CallAssignedViewNodePorts));
		}
#endif
    }
	override protected void Reset(){
        AssignViewNodePortConnecter();
    }
    public enum ViewNodePortVariant
    {
        OnClick = -1,
        ToggleOn = 0,
        ToggleOff = 1,
    }
    public void CallViewNodePortConnecter(ViewNodePortVariant variant = ViewNodePortVariant.OnClick){
#if (HYBR_NODE_EDITOR)
		var viewNodePortConnector = GetComponent<ViewNodePortConnector>();
		if (viewNodePortConnector != null){
            viewNodePortConnector.CallAssignedViewNodePorts((int)variant);
        }
#endif
    }
    // *** ViewNodePortConnector auto-setup END ***
    

    /// <summary>
    /// Assume the correct visual state.
    /// </summary>
    protected override void Start()
    {
        ButtonTextChanged();
        AssignViewNodePortConnecter();
        PlayEffect(true);
        ReleaseDepressed();

        if (interactionSurface == null) {
            Debug.LogError("interactionSurface is null on this CustomButton. This means this button can never be called! please assign one", this);
            return;
        }
        sounds = GetComponent<UiElementSounds>();
        var interactionComponent = interactionSurface.GetComponent<CustomUiInteractionHandler>();
        if (interactionComponent == null)
        {
            interactionSurface.AddComponent<CustomUiInteractionHandler>();
        }
        interactionComponent = interactionSurface.GetComponent<CustomUiInteractionHandler>();
        interactionComponent.onPointerDownEvent.AddListener(PointerDown);
        interactionComponent.onPointerUpEvent.AddListener(PointerUp);
        interactionComponent.onPointerEnter.AddListener(PointerEnter);
        interactionComponent.onPointerExit.AddListener(PointerExit);
    }

    public void SetDepressed(){
        if (depressedGraphic != null && depressedGraphic.GetComponent<Image>().sprite != null) depressedGraphic.gameObject.SetActive(true);
    }
    public void ReleaseDepressed(){
        if (depressedGraphic != null && depressedGraphic.GetComponent<Image>().sprite != null) depressedGraphic.gameObject.SetActive(false);
    }
    public void SetEnabled(){
        Set(true);
        if (toggleableButton) CallViewNodePortConnecter(ViewNodePortVariant.ToggleOn);
        //already handled by Set() //if (enabledGraphic != null) enabledGraphic.gameObject.SetActive(true);
    }
    public void SetDisabled(){
        Set(false);
        if (toggleableButton) CallViewNodePortConnecter(ViewNodePortVariant.ToggleOff);
        //already handled by Set() //if (enabledGraphic != null) enabledGraphic.gameObject.SetActive(false);
    }
    public void PointerEnter(PointerEventData eventData)
    {
        GetComponent<AnimatorUiElement>()?.HighlightOnHovering();
    }
    public void PointerExit(PointerEventData eventData)
    {
        GetComponent<AnimatorUiElement>()?.StopHighlightOnHovering();
    }
    bool wasOnDuringPointerDown = false;
    public void PointerDown(PointerEventData eventData)
    {
        if (toggleableButton){
            wasOnDuringPointerDown = m_IsOn;
            SetDepressed();
            if (!m_IsOn){
                SetEnabled();
                onClickActivate.Invoke();
                sounds.PlaySound(UiSoundType.MouseDownDefault);
            } else {
                onClickWhileToggled.Invoke();
                sounds.PlaySound(UiSoundType.MouseDownAlternate);
            }
        } else {
            SetDepressed();
            SetEnabled();
            onClickActivate.Invoke();
            sounds.PlaySound(UiSoundType.MouseDownDefault);
        }
    }
    public void PointerUp(PointerEventData eventData)
    {
        if (toggleableButton){
            ReleaseDepressed();
            if (!wasOnDuringPointerDown){
                oReleaseIntoToggled.Invoke();
                sounds.PlaySound(UiSoundType.MouseUpDefault);
            } else {
                SetDisabled();
                onReleaseDeactivate.Invoke();
                sounds.PlaySound(UiSoundType.MouseUpAlternate);
            }
            if (enabledGraphic != null && enabledGraphic.GetComponent<Image>().sprite != null
                && !showEnabledImmediately) {
                enabledGraphic.gameObject.SetActive(m_IsOn);
                //enabledGraphic.canvasRenderer.SetAlpha(m_IsOn ? 1f : 0f);
            }
        } else {
            ReleaseDepressed();
            SetDisabled();
            CallViewNodePortConnecter(ViewNodePortVariant.OnClick);
            onReleaseDeactivate.Invoke();
            sounds.PlaySound(UiSoundType.MouseUpDefault);
        }
    }
    
    public void SetInteractable(bool interactable){
        if (interactable){
            interactionSurface.SetActive(true);
            disabledGraphic.color = Color.white;
        } else {
            interactionSurface.SetActive(false);
            GetComponent<AnimatorUiElement>()?.StopHighlightOnHovering();
            disabledGraphic.color = new Color(0.8f,0.8f,0.8f,1.0f);
        }
    }

    private void InternalToggle()
    {
        if (!IsActive()/* || !IsInteractable()*/)
            return;

        isOn = !isOn;
    }

}
