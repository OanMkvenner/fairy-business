using DG.Tweening;
using UnityEngine;

namespace Animation.Tweens
{
    public class ScaleTweenAnimation : BaseTweenAnimation
    {
        [Header("Scale Settings")]
        [SerializeField] private float scaleFactor;
        protected override Tween PlayAnimation()
        {
            return RectTransform.DOScale(Vector3.zero * scaleFactor, 
                AnimationSettings.Duration).SetEase(AnimationSettings.Ease);
        }
    }
}