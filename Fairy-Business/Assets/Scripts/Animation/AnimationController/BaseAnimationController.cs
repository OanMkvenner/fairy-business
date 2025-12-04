using UnityEngine;

namespace Animation.AnimationController
{
    public abstract class BaseAnimationController : MonoBehaviour
    {
        [SerializeField] private int priority;
        
        [SerializeField] private AnimationFlow[] defaultAnimationFlows;
        
        protected virtual void Awake()
        {
            if (defaultAnimationFlows.Length == 0)
            {
                Debug.LogError($"No animation flow for animation controller found {this.name}");
            }
        }

        protected void StartAnimations()
        {
            foreach (AnimationFlow animationFlow in defaultAnimationFlows)
            {
                AnimationScheduler.instance.AddAnimationJob(new AnimationJob(animationFlow, priority));
            }
        }
    }
}