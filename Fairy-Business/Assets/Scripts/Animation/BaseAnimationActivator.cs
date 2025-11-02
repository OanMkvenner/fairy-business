using UnityEngine;

namespace Animation
{
    public class BaseAnimationActivator : MonoBehaviour
    {
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
        }
    }
}