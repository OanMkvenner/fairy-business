using Animation.AnimationEffects;
using DG.Tweening;
using UnityEngine;
using Utility;

namespace Animation.Tweens
{
    [RequireComponent(typeof(RectTransform), typeof(AnimationFlow))]
    public abstract class BaseTweenAnimation : MonoBehaviour, IUIAnimation
    {
        [field: SerializeField] public AnimationSettings AnimationSettings {get; private set;}

        [Space] [Tooltip("Effect is applied at the end of tween.")]
        [SerializeField] protected BaseAnimationEffect animationEffect;
        
        protected RectTransform RectTransform;
        protected Canvas Canvas;
        
        private void Awake()
        {
            RectTransform = GetComponent<RectTransform>();
            
        }

        private void Start()
        {
            Canvas = CanvasReferencer.instance.Canvas;
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