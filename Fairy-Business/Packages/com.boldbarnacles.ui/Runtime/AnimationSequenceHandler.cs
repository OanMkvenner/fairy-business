using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

public class AnimationSequenceHandler : MonoBehaviour {
    List<AnimationSequencer> sequences = new List<AnimationSequencer>();
    private void OnDestroy() {
        for (int i = 0; i < sequences.Count; i++)
        {
            sequences[i].KillAnimations();
        }
        sequences.Clear();
    }
    public void RegisterAnimationSequencer(AnimationSequencer animSequencer){
        sequences.Add(animSequencer);
    }
    public void UnregisterAnimationSequencer(AnimationSequencer animSequencer){
        animSequencer.assignedAnimationSequenceHandler = null;
        animSequencer.attachedGameObject = null;
        sequences.Remove(animSequencer);
        if (sequences.Count == 0) Destroy(this);
    }
}

public class AnimationSequencer
{
    protected Sequence animationSequence;
    protected float sequenceDelay = 0;
    protected TweenCallback queuedAction = null;
    private bool flaggedForDeletion = false;
    bool appendNextTween = false;

    public AnimationSequenceHandler assignedAnimationSequenceHandler = null;
    public GameObject attachedGameObject = null;

    static public AnimationSequencer CreateAnimator(GameObject attachedGameObject){
        var newAnimationSequencer = new AnimationSequencer();
        newAnimationSequencer.RegisterAnimationSequencer(attachedGameObject);
        return newAnimationSequencer;
    }
    public void RegisterAnimationSequencer(GameObject attachedGameObject){
        if (this.attachedGameObject == attachedGameObject) return; // already registered
        this.attachedGameObject = attachedGameObject;
        if(attachedGameObject == null) return;
        if (assignedAnimationSequenceHandler != null) {
            assignedAnimationSequenceHandler.UnregisterAnimationSequencer(this);
        }
        var animSequenceHandler = attachedGameObject.GetComponent<AnimationSequenceHandler>();
        if (animSequenceHandler == null) {
            animSequenceHandler = attachedGameObject.AddComponent<AnimationSequenceHandler>();
        }
        if (animSequenceHandler != null) {
            animSequenceHandler.RegisterAnimationSequencer(this);
            assignedAnimationSequenceHandler = animSequenceHandler;
        };
    }

    public void KillAnimations(){
        animationSequence.Kill();
        animationSequence = null;
    }
    public bool IsAnimating(){
        return animationSequence.IsActive() && animationSequence.IsPlaying();
    }
    public void StopAnimating(bool forceStop = false){
        if (appendNextTween && !forceStop){
            appendNextTween = false;
            // dont kill tween on AppendTween!
            return;
        }
        if (flaggedForDeletion){
            Debug.LogError("WARNING: animator was flagged to delete gameobject, but tween was Killed by 'StopAnimating' call! Check if 'OnKill' fired properly and destroying worked!?");
        }
        animationSequence.Kill(complete: false);
        queuedAction = null;
        animationSequence = DOTween.Sequence();
    }
    public void FinishAnimating(bool forceStop = false){
        if (appendNextTween && !forceStop){
            appendNextTween = false;
            // dont kill tween on AppendTween!
            return;
        }
        animationSequence.Kill(complete: true);
        queuedAction = null;
        animationSequence = DOTween.Sequence();
    }
    public void ApplySequenceDelay(){
        if (sequenceDelay > 0){
            animationSequence.AppendInterval(sequenceDelay);
            sequenceDelay = 0;
        }
    }
    public void ClearActionOnFinish(){
        queuedAction = null;
        // replace OnKill with empty callbacks
        animationSequence.OnKill(() =>{});
    }
    public void AddActionOnFinish(TweenCallback action){
        if (queuedAction != null){
            queuedAction = (TweenCallback)TweenCallback.Combine(queuedAction, action);
        } else {
            queuedAction = action;
        }
        animationSequence.OnKill(queuedAction);
    }

    public void ApplyQueuedOptions(){
        ApplySequenceDelay();
    }
    public void SetSequenceDelay(float i_sequenceDelay){
        sequenceDelay = i_sequenceDelay;
    }
    public void QueueNextTween(bool setValue = true){
        SetAppendNextTween(setValue);
    }
    public void SetAppendNextTween(bool setValue = true){
        appendNextTween = setValue;
    }
    public void StartSequence(GameObject attachedGameObject, Tween tween){
        RegisterAnimationSequencer(attachedGameObject);
        StopAnimating();
        ApplyQueuedOptions();
        animationSequence.Append(tween);
    }
    public void StartSequence(GameObject attachedGameObject, Sequence tweenSequence){
        RegisterAnimationSequencer(attachedGameObject);
        StopAnimating();
        ApplyQueuedOptions();
        animationSequence.Append(tweenSequence);
    }
}