using DG.Tweening;
using UnityEngine;

namespace Animation
{
    [RequireComponent(typeof(RectTransform))]
    public abstract class BaseAnimation : MonoBehaviour, IUIAnimation
    {
        [field: SerializeField] protected AnimationSettings AnimationSettings {get; set;}
        
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