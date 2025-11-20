using UnityEngine;

namespace Animation.AnimationController
{
    public abstract class BaseAnimationController : MonoBehaviour
    {
        [SerializeField] private int priority;
        
        private AnimationFlow animationFlow;
        
        protected virtual void Awake()
        {
            animationFlow = GetComponentInChildren<AnimationFlow>();
        }

        public void StartAnimations()
        {
            AnimationScheduler.instance.AddJob(new AnimationJob(animationFlow, priority));
        }
    }
}