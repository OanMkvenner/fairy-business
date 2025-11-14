using Animation.AnimationEffects;
using DG.Tweening;
using UnityEngine;

namespace Animation.Tweens
{
    [RequireComponent(typeof(RectTransform), typeof(AnimationFlow))]
    public abstract class BaseTweenAnimation : MonoBehaviour, IUIAnimation
    {
        [field: SerializeField] public AnimationSettings AnimationSettings {get; private set;}

        [Space]
        [SerializeField] protected BaseAnimationEffect animationEffect;
        
        protected RectTransform RectTransform;
        protected Canvas Canvas;
        
        private void Awake()
        {
            RectTransform = GetComponent<RectTransform>();
            Canvas = MainCanvasReferencer.instance.Canvas;
        }

        protected void ApplyEffect(Tween tween)
        {
            if (animationEffect != null)
            {
                tween.OnComplete(() => animationEffect.ApplyEffect());
            }
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