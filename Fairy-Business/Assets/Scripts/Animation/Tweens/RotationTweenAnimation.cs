using DG.Tweening;
using UnityEngine;

namespace Animation.Tweens
{
    public class RotationTweenAnimation : BaseTweenAnimation
    {
        [Header("Rotation Values")]
        [SerializeField] private float rotationValue;
        protected override Tween PlayAnimation()
        {
            return RectTransform.DORotate(new Vector3(0, 0, rotationValue), 
                AnimationSettings.Duration).SetEase(AnimationSettings.Ease);
        }
    }
}