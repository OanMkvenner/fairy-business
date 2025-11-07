using DG.Tweening;

namespace Animation.Tweens
{
    public class RotationTweenAnimation : BaseTweenAnimation
    {
        protected override Tween PlayAnimation()
        {
            return RectTransform.DORotate(AnimationSettings.GetUIPosition(Canvas), 
                AnimationSettings.Duration).SetEase(AnimationSettings.Ease);
        }
    }
}