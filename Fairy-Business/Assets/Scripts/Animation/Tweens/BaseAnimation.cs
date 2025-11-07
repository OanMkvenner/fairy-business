using DG.Tweening;
using UnityEngine;

namespace Animation.Tweens
{
    [RequireComponent(typeof(RectTransform), typeof(AnimationFlow))]
    public abstract class BaseAnimation : MonoBehaviour, IUIAnimation
    {
        [field: SerializeField] public AnimationSettings AnimationSettings {get; private set;}

        protected RectTransform RectTransform;

        protected Canvas Canvas;
        
        private void Awake()
        {
            RectTransform = GetComponent<RectTransform>();
            Canvas = GetComponentInParent<Canvas>();
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