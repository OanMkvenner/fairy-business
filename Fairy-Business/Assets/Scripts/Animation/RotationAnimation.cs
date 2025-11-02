using DG.Tweening;

namespace Animation
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