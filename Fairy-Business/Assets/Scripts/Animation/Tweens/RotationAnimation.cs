using DG.Tweening;

namespace Animation.Tweens
{
    public class RotationAnimation : BaseAnimation
    {
        protected override Tween PlayAnimation()
        {
            return RectTransform.DORotate(AnimationSettings.UIEndPosition.GetUIPosition(Canvas), 
                AnimationSettings.Duration).SetEase(AnimationSettings.Ease);
        }
    }
}