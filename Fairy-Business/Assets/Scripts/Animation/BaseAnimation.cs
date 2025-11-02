using DG.Tweening;
using UnityEngine;

namespace Animation
{
    [RequireComponent(typeof(RectTransform), typeof(AnimationFlow))]
    public abstract class BaseAnimation : MonoBehaviour, IUIAnimation
    {
        [field: SerializeField] public AnimationSettings AnimationSettings {get; private set;}

        protected RectTransform RectTransform;
        
        private void Awake()
        {
            RectTransform = GetComponent<RectTransform>();
        }

        protected abstract Tween PlayAnimation();

        #region IUIAnimation

        public Tween Play()
        {
            return PlayAnimation();
        }

        #endregion
    }
}