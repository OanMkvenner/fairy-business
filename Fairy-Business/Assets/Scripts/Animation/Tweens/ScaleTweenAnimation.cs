using DG.Tweening;
using UnityEngine;

namespace Animation.Tweens
{
    public class ScaleTweenAnimation : BaseTweenAnimation
    {
        [Header("Scale Settings")]
        [SerializeField] private Vector3 scale;

        protected override Tween PlayAnimation()
        {
            Tween tween = RectTransform.DOScale(scale, AnimationSettings.Duration
            ).SetEase(AnimationSettings.Ease);

            ApplyEffect(tween);

            return tween;
        }
    }
}