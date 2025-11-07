using DG.Tweening;
using UnityEngine;

namespace Animation.Tweens
{
    public class ShakeScaleTweenAnimation : BaseTweenAnimation
    {
        [Header("ShakeScale Values")]
        [SerializeField] private float strength;
        [SerializeField] private int vibrato;
        [SerializeField] private float randomness;
        [SerializeField] private bool fadeOut;
        [SerializeField] private ShakeRandomnessMode shakeRandomnessMode;
        
        protected override Tween PlayAnimation()
        {
            return RectTransform.DOShakeScale(AnimationSettings.Duration, strength, vibrato, randomness, fadeOut, shakeRandomnessMode)
                .SetEase(AnimationSettings.Ease);
        }
    }
}