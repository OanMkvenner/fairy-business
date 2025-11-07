using System;
using DG.Tweening;
using UnityEngine;

namespace Animation.AnimationController
{
    public abstract class BaseAnimationController<T> : MonoBehaviour
    {
        public static event Action AllSequencesCompletedEvent;
        private AnimationFlow[] animationFlows;
        
        private void Awake()
        {
            animationFlows = GetComponentsInChildren<AnimationFlow>();
        }

        public void ActivateAnimations()
        {
            foreach (AnimationFlow animationeFlow in animationFlows)
            {
                animationeFlow.StartAnimation();
            }
            
            int sequencesToWait = 0;
            int sequencesCompleted = 0;
            
            foreach (AnimationFlow animationFlow in animationFlows)
            {
                if (animationFlow.Sequence != null && animationFlow.Sequence.IsActive())
                {
                    sequencesToWait++;
                    
                    animationFlow.Sequence.OnComplete(() =>
                    {
                        sequencesCompleted++;

                        if (sequencesCompleted < sequencesToWait) return;
                        
                        AllSequencesCompletedEvent?.Invoke();
                    });
                }
            }
        }
    }
}