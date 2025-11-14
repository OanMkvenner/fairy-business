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
            Tween tween = RectTransform.DOScale(RectTransform.localScale * scaleFactor, AnimationSettings.Duration
            ).SetEase(AnimationSettings.Ease);

            ApplyEffect(tween);

            return tween;
        }
    }
}