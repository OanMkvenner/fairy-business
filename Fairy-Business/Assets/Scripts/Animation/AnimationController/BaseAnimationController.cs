using UnityEngine;

namespace Animation.AnimationController
{
    public abstract class BaseAnimationController : MonoBehaviour
    {
        [SerializeField] private int priority;
        
        [SerializeField] private AnimationFlow[] animationFlows;
        
        protected virtual void Awake()
        {
            if(animationFlows.Length == 0)
                animationFlows = GetComponentsInChildren<AnimationFlow>();

            if (animationFlows.Length == 0)
            {
                Debug.LogError($"No animation flow for animation controller found {this.name}");
            }
        }

        protected void StartAnimations()
        {
            foreach (AnimationFlow animationFlow in animationFlows)
            {
                AnimationScheduler.instance.Enqueue(new AnimationJob(animationFlow, priority));
            }
        }
    }
}