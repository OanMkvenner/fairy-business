using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Reflection;

using System.Linq;
using System;
using DG.Tweening;

namespace XNode.UiStateGraph {
	[NodeWidth(146), NodeTint(200, 80, 50)]
	public class TweenAnimationNode : UiStateNode {
		[Output] public TargetNode cancelAnims;
		[Output] public ButtonNode animFinished;
    	[HideInInspector] public GuidReference assignedTweenAnimationTarget = new GuidReference(); // reference to the object, wich the specified method is called on
		// serialized

		[HideInInspector] public TweenMode tweenMode;
		[HideInInspector] public ToFromMode tweenToFromMode;
		[HideInInspector] public Ease easeMode = Ease.InOutSine;

		[HideInInspector] public float x = 0;
		[HideInInspector] public float y = 0;
		[HideInInspector] public float z = 0;
		[HideInInspector] public float k = 0;
		[HideInInspector] public float duration = 1;

		Sequence mySequence = null;

		public enum TweenMode
		{
			RectMove,
			RectRotate,
			RectScale,
			CanvasGroupFade,
			RectMoveScreenwidthLeft,
			RectMoveScreenwidthRight,
			HighlightHover,
			ClickedExplode
		}
		public enum ToFromMode
		{
			ToValue,
			FromValue,
		}

		override protected void Init(){
			mySequence = DOTween.Sequence();
		}

		override public void OnEnter(UiStateNode originNode, NodePort incomingPort) {
			base.OnEnter(originNode, incomingPort);
			
			StartAssignedAnimation();
		}

		public void AddTweenAnimationTarget(GuidReference canvasGuidRef){
			assignedTweenAnimationTarget = canvasGuidRef;
		}

		void OnFinishTween(){
			MoveAlongPortSupressingErrors("animFinished");
		}

		public void StopCurrentAnimation() {
			if (mySequence.IsActive()) {
				mySequence.Kill(false);
			}
			mySequence = DOTween.Sequence();
		}

		
		override public void OnLeave(){
			// dont cancel Animationprocess when leaving normally (is completed anyway)
			base.OnDeactivate();
		}

		override public void OnDeactivate(){
			StopCurrentAnimation();
			base.OnDeactivate();
		}


		public string[] GetRequiredDataFields(){
			switch (tweenMode)
			{
				case TweenMode.RectMove: 
					return new string[]{"Move along X","Move along Y","Move along Z"};
				case TweenMode.RectRotate: 
					return new string[]{"Rotate on X","Rotate on Y","Rotate on Z"};
				case TweenMode.RectScale: 
					return new string[]{"Scale along X","Scale along Y","Scale along Z"};
				case TweenMode.CanvasGroupFade: 
					return new string[]{"Alpha"};
				case TweenMode.RectMoveScreenwidthLeft: 
					return new string[]{};
				case TweenMode.RectMoveScreenwidthRight: 
					return new string[]{};
				case TweenMode.HighlightHover: 
					return new string[]{"Scale from", "Scale to", "Alpha from", "Alpha to"};
				case TweenMode.ClickedExplode: 
					return new string[]{"Scale", "Alpha"};
				default:
					return new string[]{};
			}
		}

		public string GetPotentialError(){
			string errorText = "";
			if (assignedTweenAnimationTarget.gameObject == null) return "";
			if ((assignedTweenAnimationTarget.gameObject.GetComponent<CanvasGroup>()) == null
			 &&	(tweenMode == TweenMode.CanvasGroupFade)) {
				errorText = "No CanvasGroup Component found! " + tweenMode + " only works on objects containing such acomponent!";
				//Rethink ACTUALLY EVERY object has a Transform. But this might be needed for fur
			}
			return errorText;
		}

			
		void StartAssignedAnimation(){
			StateGraph stateGraph = graph as StateGraph;
			if(!Application.isPlaying) return; // dont allow any Tweener changes in Edit-mode
			if(assignedTweenAnimationTarget == null) return;
			
			StopCurrentAnimation();
			TweenAnimCancelNode.CancelAllConnectedTweenAnimationNodes(currentNode: this, animationTargetsPort: "cancelAnims");

			Transform rectTrans = assignedTweenAnimationTarget.gameObject.GetComponent<Transform>();

			CanvasGroup canvGroup = assignedTweenAnimationTarget.gameObject.GetComponent<CanvasGroup>();
			// if tweenToFromMode is set to FromValue we revert the relativeness first because From(true) needs a different kind of relative
			if(rectTrans != null) {

				if (tweenMode == TweenMode.RectMove) {
					var tween = rectTrans.DOLocalMove(new Vector3(x,y,z), duration).SetRelative().SetEase(easeMode);
					if (tweenToFromMode == ToFromMode.FromValue) tween.SetRelative(false).From(true);
					mySequence.Append(tween);
				}
				if (tweenMode == TweenMode.RectRotate) {
					var tween = rectTrans.DOLocalRotate(new Vector3(x,y,z), duration).SetRelative().SetEase(easeMode);
					if (tweenToFromMode == ToFromMode.FromValue) tween.SetRelative(false).From(true);
					mySequence.Append(tween);
				}
					
				if (tweenMode == TweenMode.RectScale) {
					var tween = rectTrans.DOScale(new Vector3(x,y,z), duration).SetRelative(false).SetEase(easeMode);
					if (tweenToFromMode == ToFromMode.FromValue) tween.SetRelative(false).From(true);
					mySequence.Append(tween);
				}				
				if (tweenMode == TweenMode.CanvasGroupFade) {
					if(canvGroup != null) {
						var tween = canvGroup.DOFade(x, duration).SetRelative(false).SetEase(easeMode);
						if (tweenToFromMode == ToFromMode.FromValue) tween.SetRelative(false).From(true);
						mySequence.Append(tween);
					}
				}
				if (tweenMode == TweenMode.RectMoveScreenwidthLeft) {
        			float mainCanvasWidth = TweenAnimator.GetMainSceneCanvas().GetComponent<RectTransform>().sizeDelta.x;
					var tween = rectTrans.DOLocalMove(new Vector3(-mainCanvasWidth,0,0), duration).SetRelative().SetEase(easeMode).SetRelative(false);
					if (tweenToFromMode == ToFromMode.FromValue) tween.From(true);
					mySequence.Append(tween);
				}				
				if (tweenMode == TweenMode.RectMoveScreenwidthRight) {
        			float mainCanvasWidth = TweenAnimator.GetMainSceneCanvas().GetComponent<RectTransform>().sizeDelta.x;
					var tween = rectTrans.DOLocalMove(new Vector3(mainCanvasWidth,0,0), duration).SetRelative().SetEase(easeMode).SetRelative(false);
					if (tweenToFromMode == ToFromMode.FromValue) tween.From(true);
					mySequence.Append(tween);
				}			
				if (tweenMode == TweenMode.HighlightHover) {
					rectTrans.localScale = new Vector3(x,x,x);
					var tween0 = rectTrans.DOScale(new Vector3(y,y,y), duration).SetRelative(false).SetEase(easeMode);
					mySequence.Append(tween0);
					if(canvGroup != null) {
						canvGroup.interactable = true;
						canvGroup.blocksRaycasts = true;
						canvGroup.alpha = z;
						var tween3 = canvGroup.DOFade(k, duration).SetRelative(false).SetEase(easeMode);
						mySequence.Join(tween3);
					}
					//var tween1 = rectTrans.DOScale(new Vector3(x,x,x), duration).SetRelative(false).SetEase(easeMode);
					//mySequence.Append(tween1);
					//if(canvGroup != null) {
					//	var tween4 = canvGroup.DOFade(z, duration).SetRelative(false).SetEase(easeMode);
					//	mySequence.Join(tween4);
					//}
					mySequence.SetLoops(-1, LoopType.Yoyo); // unlimited loops
				}			
				if (tweenMode == TweenMode.ClickedExplode) {
					var tween = rectTrans.DOScale(new Vector3(x,x,x), duration).SetRelative(false).SetEase(easeMode);
					if (tweenToFromMode == ToFromMode.FromValue) tween.SetRelative(false).From(true);
					mySequence.Append(tween);
					if(canvGroup != null) {
						canvGroup.interactable = false;
						canvGroup.blocksRaycasts = false;
						var tween2 = canvGroup.DOFade(y, duration).SetRelative(false).SetEase(easeMode);
						if (tweenToFromMode == ToFromMode.FromValue) tween2.SetRelative(false).From(true);
						mySequence.Join(tween2);
					}
				}

			}

			mySequence.OnComplete(() =>{OnFinishTween();});

			//DOTween.To(()=> (float)info.GetValue(rectTrans), x=> info.SetValue(rectTrans, x), 0.0f, 1.0f);
			
		}

	}

}