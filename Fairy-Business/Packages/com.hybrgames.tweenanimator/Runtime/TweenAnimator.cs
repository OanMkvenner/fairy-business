using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using System;
using UnityEngine.UI;


public class TweenAnimator : MonoBehaviour
{
    public bool reactToAnyChildButton = true;
    public bool reactToParentButton = false;
    // serialized
    [HideInInspector] public TweenMode btnClickTweenMode = TweenMode.DefaultClickedExplode;
    [HideInInspector] public TweenMode highlightTweenMode= TweenMode.DefaultHighlightHover;

    public bool autoHighlight = false;
    [HideInInspector] public ValueSet onClickValues;
    [HideInInspector] public ValueSet highlightValues;

	Sequence mySequence = null;

    [Serializable]
    public class ValueSet{
        [HideInInspector] public Ease easeMode = Ease.InOutSine;
        [HideInInspector] public float val_1 = 0;
        [HideInInspector] public float val_2 = 0;
        [HideInInspector] public float val_3 = 0;
        [HideInInspector] public float val_4 = 0;
        [HideInInspector] public float duration = 1;
        public ValueSet (){}
        public ValueSet (ValueSet copyVal){
            easeMode = copyVal.easeMode;
            val_1 = copyVal.val_1;
            val_2 = copyVal.val_2;
            val_3 = copyVal.val_3;
            val_4 = copyVal.val_4;
            duration = copyVal.duration;
        }
    }
    public enum TweenMode
    {
        None,
        DefaultClickedExplode,
        DefaultHighlightHover,
        RectMove,
        RectRotate,
        RectScale,
        CanvasGroupFade,
        RectMoveScreenwidthLeft,
        RectMoveScreenwidthRight,
        ClickedExplode,
        ClickedBounce,
        HighlightHover,
        DefaultShowByScalingIn,
    }
    bool initialized = false;
    bool elementHideOverride; // this is used to stop the element from reappearing on viewchanges
    // use this to ignore any future "ShowElement" calls and keep the element hidden even on ViewChange
    public void HideElementOverride(TweenMode tweenMode = TweenMode.None){
        HideElement(tweenMode);
        elementHideOverride = true;
    }
    // use this to lift the override on the element, allowing it to be shown using "ShowElement" again and to 
    public void ShowElementOverride(TweenMode tweenMode = TweenMode.None){
        elementHideOverride = false;
        ShowElement(tweenMode);
    }
    public void HideElement(TweenMode tweenMode = TweenMode.None){
        CheckInitialized();
        if (tweenMode == TweenMode.None)
        {
            cnvsGroupReseter.DisableCanvasGroup();
        } else {
            Debug.LogError("HideElement() with custom TweenMode not implemented yet!");
        }
    }
    void CheckInitialized(){
        if (!initialized){
            Awake();
        }
    }
    public void ShowElement(TweenMode tweenMode = TweenMode.None){
        CheckInitialized();
        if (elementHideOverride) {
            // this gets called after TransformReseter & CanvasGroupReseters did their work. So we need to hide again!
            HideElement();
            return;
        };
        cnvsGroupReseter.ResetCanvasGroup();
        transformReseter.ResetTransform();
        if (tweenMode != TweenMode.None)
        {
            StartAnim(tweenMode, new ValueSet{}); 
        } else {
            if (autoHighlight)
            {
                StartHighlightAnim();
            }
        }
    }
    public static Transform GetMainSceneCanvas(){
        if (MainCanvasReferencer.mainCanvas == null){
            Debug.LogError("You need to add a MainCanvasReferencer to the main canvas in your scene. If you have multiple main Canvas'es, you might need to change a few things...");
        }
        return MainCanvasReferencer.mainCanvas;
    }
    public bool GetAnyFieldsRequired(TweenMode tweenMode){
        bool fieldsRequired = true;
        if (tweenMode == TweenMode.None || tweenMode == TweenMode.DefaultClickedExplode || tweenMode == TweenMode.DefaultHighlightHover)
        {
            fieldsRequired = false;
        }
        return fieldsRequired;
    }
    // used by Inspector to decide how to name the variables and how many to show
    public string[] GetRequiredDataFields(TweenMode tweenMode){
        switch (tweenMode)
        {
            case TweenMode.DefaultClickedExplode: return new string[]{};
            case TweenMode.DefaultHighlightHover: return new string[]{};
            case TweenMode.RectMove: return new string[]{"Move along X","Move along Y","Move along Z"};
            case TweenMode.RectRotate: return new string[]{"Rotate on X","Rotate on Y","Rotate on Z"};
            case TweenMode.RectScale: return new string[]{"Scale along X","Scale along Y","Scale along Z"};
            case TweenMode.CanvasGroupFade: return new string[]{"Alpha"};
            case TweenMode.RectMoveScreenwidthLeft: return new string[]{};
            case TweenMode.RectMoveScreenwidthRight: return new string[]{};
            case TweenMode.HighlightHover: return new string[]{"Scale from", "Scale to", "Alpha from", "Alpha to"};
            case TweenMode.ClickedExplode: return new string[]{"Scale", "Alpha"};
            default:
                return new string[]{};
        }
    }

    // could return an error here for the inspector if some settings are incorrect - see "TweenAnimatorEditor" for how its being used
    public string GetPotentialError(){
        string errorText = "";
        return errorText;
    }

    bool viewNotActive = true;
    public void OnViewEnter(){
        viewNotActive = false;
        ShowElement();
    }
    public void OnViewExit(){
        viewNotActive = true;
        StopCurrentAnimation();
    }

    CanvasGroup cnvsGroup = null;
    CanvasGroupReseter cnvsGroupReseter = null;
    TransformReseter transformReseter = null;
    void Awake() {
        if(!Application.isPlaying) return; // dont allow any Tweener changes in Edit-mode
        if(initialized) return;
        initialized = true;

        mySequence = DOTween.Sequence();
        Button btn = GetComponent<Button>();
        if (btn != null)
        {
            btn.onClick.AddListener(delegate{this.StartOnClickedAnim();});
        }
        if (reactToAnyChildButton)
        {
            List<Button> allChildrenButtons = Utilities.GetComponentsInChildrenIncludingDisabled<Button>(this.transform);
            foreach (var childBtn in allChildrenButtons)
            {
                childBtn.onClick.AddListener(delegate{this.StartOnClickedAnim();});
            }
        }
        if (reactToParentButton)
        {
            Button parentBtn = this.transform.parent.GetComponent<Button>();
            if (parentBtn)
            {
                parentBtn.onClick.AddListener(delegate{this.StartOnClickedAnim();});
            }
        }
        cnvsGroup = GetComponent<CanvasGroup>();
        if (cnvsGroup == null){
            cnvsGroup = this.gameObject.AddComponent<CanvasGroup>();
            cnvsGroup.alpha = 1.0f;
            cnvsGroup.blocksRaycasts = true;
            cnvsGroup.enabled = true;
            cnvsGroup.ignoreParentGroups = false;
        }
        // add TransformReseter to realign positions on entering view
        transformReseter = GetComponent<TransformReseter>();
        if (transformReseter == null) {
            transformReseter = this.gameObject.AddComponent<TransformReseter>();
        }
        cnvsGroupReseter = GetComponent<CanvasGroupReseter>();
        if (cnvsGroupReseter == null) {
            cnvsGroupReseter = this.gameObject.AddComponent<CanvasGroupReseter>();
        }
    }
    void Start(){
    }
    
    public void StartOnClickedAnim(){
        StartAnim(btnClickTweenMode, onClickValues);
    }
    public void StartHighlightAnim(){
        StartAnim(highlightTweenMode, highlightValues);
    }

    public void StopCurrentAnimation() {
        CheckInitialized();
        if (mySequence.IsActive()) {
            mySequence.Kill(false);
        }
        mySequence = DOTween.Sequence();
    }

    void StartAnim(TweenMode tweenMode, ValueSet readOnlyValueSet){
        CheckInitialized();
        if (viewNotActive) { 
            Debug.LogWarning("view not active, no animations allowed yet on this TweenAnimator!");
            return;
        }
        ValueSet valueSet = new ValueSet(readOnlyValueSet);
        if(!Application.isPlaying) return; // dont allow any Tweener changes in Edit-mode
        if (tweenMode == TweenMode.None) return; // ignore "None"

        StopCurrentAnimation();

        Transform rectTrans = gameObject.GetComponent<Transform>();

        CanvasGroup canvGroup = gameObject.GetComponent<CanvasGroup>();
        // if tweenToFromMode is set to FromValue we revert the relativeness first because From(true) needs a different kind of relative
        if(rectTrans != null) {

            if (tweenMode == TweenMode.RectMove) {
                var tween = rectTrans.DOLocalMove(new Vector3(valueSet.val_1,valueSet.val_2,valueSet.val_3), valueSet.duration).SetRelative().SetEase(valueSet.easeMode);
                //if (tweenToFromMode == ToFromMode.FromValue) tween.SetRelative(false).From(true);
                mySequence.Append(tween);
            }
            if (tweenMode == TweenMode.RectRotate) {
                var tween = rectTrans.DOLocalRotate(new Vector3(valueSet.val_1,valueSet.val_2,valueSet.val_3), valueSet.duration).SetRelative().SetEase(valueSet.easeMode);
                //if (tweenToFromMode == ToFromMode.FromValue) tween.SetRelative(false).From(true);
                mySequence.Append(tween);
            }
                
            if (tweenMode == TweenMode.RectScale) {
                var tween = rectTrans.DOScale(new Vector3(valueSet.val_1,valueSet.val_2,valueSet.val_3), valueSet.duration).SetRelative(false).SetEase(valueSet.easeMode);
                //if (tweenToFromMode == ToFromMode.FromValue) tween.SetRelative(false).From(true);
                mySequence.Append(tween);
            }				
            if (tweenMode == TweenMode.CanvasGroupFade) {
                if(canvGroup != null) {
                    var tween = canvGroup.DOFade(valueSet.val_1, valueSet.duration).SetRelative(false).SetEase(valueSet.easeMode);
                    //if (tweenToFromMode == ToFromMode.FromValue) tween.SetRelative(false).From(true);
                    mySequence.Append(tween);
                }
            }
            if (tweenMode == TweenMode.RectMoveScreenwidthLeft) {
                float mainCanvasWidth = GetMainSceneCanvas().GetComponent<RectTransform>().sizeDelta.x;
                var tween = rectTrans.DOLocalMove(new Vector3(-mainCanvasWidth,0,0), valueSet.duration).SetRelative().SetEase(valueSet.easeMode).SetRelative(false);
                //if (tweenToFromMode == ToFromMode.FromValue) tween.From(true);
                mySequence.Append(tween);
            }				
            if (tweenMode == TweenMode.RectMoveScreenwidthRight) {
                float mainCanvasWidth = GetMainSceneCanvas().GetComponent<RectTransform>().sizeDelta.x;
                var tween = rectTrans.DOLocalMove(new Vector3(mainCanvasWidth,0,0), valueSet.duration).SetRelative().SetEase(valueSet.easeMode).SetRelative(false);
                //if (tweenToFromMode == ToFromMode.FromValue) tween.From(true);
                mySequence.Append(tween);
            }
            if (tweenMode == TweenMode.ClickedExplode || tweenMode == TweenMode.DefaultClickedExplode) {
                bool relativeScaling = false;
                cnvsGroupReseter.DisableInteractions();
                if (tweenMode == TweenMode.DefaultClickedExplode)
                {
                    valueSet = new ValueSet{ easeMode = Ease.OutExpo, val_1 = 3,val_2 = 0,val_3 = 0,val_4 = 0, duration = 1 };
                    relativeScaling = true;
                }
                var tween = rectTrans.DOScale(new Vector3(valueSet.val_1,valueSet.val_1,valueSet.val_1), valueSet.duration).SetRelative(relativeScaling).SetEase(valueSet.easeMode);
                //if (tweenToFromMode == ToFromMode.FromValue) tween.SetRelative(false).From(true);
                mySequence.Append(tween);
                if(canvGroup != null) {
                    canvGroup.interactable = false;
                    canvGroup.blocksRaycasts = false;
                    var tween2 = canvGroup.DOFade(valueSet.val_2, valueSet.duration).SetRelative(false).SetEase(valueSet.easeMode);
                    //if (tweenToFromMode == ToFromMode.FromValue) tween2.SetRelative(false).From(true);
                    mySequence.Join(tween2);
                }
            }
            if (tweenMode == TweenMode.ClickedBounce) {
                bool relativeScaling = true;
                var tween = rectTrans.DOScale(new Vector3(valueSet.val_1,valueSet.val_1,valueSet.val_1), valueSet.duration).SetRelative(relativeScaling).SetEase(valueSet.easeMode);
                //if (tweenToFromMode == ToFromMode.FromValue) tween.SetRelative(false).From(true);
                mySequence.Append(tween);
            }
            if (tweenMode == TweenMode.HighlightHover || tweenMode == TweenMode.DefaultHighlightHover) {
                cnvsGroupReseter.ResetCanvasGroup();
                transformReseter.ResetTransform();
                bool relativeScaling = false;
                if (tweenMode == TweenMode.DefaultHighlightHover)
                {
                    valueSet = new ValueSet{ easeMode = Ease.InOutSine, val_1 = 1f,val_2 = -0.05f,val_3 = canvGroup.alpha * 0.6f,val_4 = canvGroup.alpha, duration = 1 };
                    relativeScaling = true;
                }
                rectTrans.localScale = new Vector3(valueSet.val_1,valueSet.val_1,valueSet.val_1);
                var tween0 = rectTrans.DOScale(new Vector3(valueSet.val_2,valueSet.val_2,valueSet.val_2), valueSet.duration).SetRelative(relativeScaling).SetEase(valueSet.easeMode);
                mySequence.Append(tween0);
                if(canvGroup != null) {
                    canvGroup.alpha = valueSet.val_4;
                    var tween3 = canvGroup.DOFade(valueSet.val_3, valueSet.duration).SetRelative(false).SetEase(valueSet.easeMode);
                    mySequence.Join(tween3);
                }
                //var tween1 = rectTrans.DOScale(new Vector3(valueSet.val_1,x,x), valueSet.duration).SetRelative(false).SetEase(valueSet.easeMode);
                //mySequence.Append(tween1);
                //if(canvGroup != null) {
                //	var tween4 = canvGroup.DOFade(z, valueSet.duration).SetRelative(false).SetEase(valueSet.easeMode);
                //	mySequence.Join(tween4);
                //}
                mySequence.SetLoops(-1, LoopType.Yoyo); // unlimited loops
            }
            if (tweenMode == TweenMode.DefaultShowByScalingIn) {
                // reset first, then use a "From(true)" tween to scale things TO these default values
                cnvsGroupReseter.ResetCanvasGroup();
                transformReseter.ResetTransform();

                valueSet = new ValueSet{ easeMode = Ease.OutExpo, val_1 = rectTrans.localScale.x,val_2 = rectTrans.localScale.y,val_3 = rectTrans.localScale.z,val_4 = 0, duration = 0.7f };
                rectTrans.localScale = new Vector3(0,0,0);
                var tween0 = rectTrans.DOScale(new Vector3(valueSet.val_1,valueSet.val_2,valueSet.val_3), valueSet.duration).SetRelative(false).SetEase(valueSet.easeMode);
                mySequence.Append(tween0);
                if(canvGroup != null) {
                    var tween3 = canvGroup.DOFade(valueSet.val_2, valueSet.duration).SetRelative(false).SetEase(valueSet.easeMode).From(true);
                    mySequence.Join(tween3);
                }
                // if autohighlighting is on, start that after showing anim is finished
                if (autoHighlight)
                {
                    mySequence.AppendCallback(() =>{
                        StartHighlightAnim();
                    });
                }
            }

        }

        mySequence.OnComplete(() =>{  });

        //DOTween.To(()=> (float)info.GetValue(rectTrans), x=> info.SetValue(rectTrans, x), 0.0f, 1.0f);
        
    }

}
