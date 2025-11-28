using UnityEngine;

namespace Animation.AnimationController
{
    public abstract class BaseAnimationController : MonoBehaviour
    {
        [SerializeField] private int priority;
        
        private AnimationFlow[] animationFlows;
        
        protected virtual void Awake()
        {
            animationFlows = GetComponentsInChildren<AnimationFlow>();
        }

        public void StartAnimations()
        {
            foreach (AnimationFlow animationFlow in animationFlows)
            {
                AnimationScheduler.instance.AddJob(new AnimationJob(animationFlow, priority));
            }
        }
    }
}