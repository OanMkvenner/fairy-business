using DG.Tweening;

namespace Animation
{
    public class RotationAnimation : BaseAnimation
    {
        protected override Tween PlayAnimation()
        {
            return RectTransform.DORotate(AnimationSettings.VectorValue, AnimationSettings.Duration).SetEase(AnimationSettings.Ease);
        }
    }
}