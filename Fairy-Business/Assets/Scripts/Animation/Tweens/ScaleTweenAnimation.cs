using DG.Tweening;

namespace Animation.Tweens
{
    public class ScaleTweenAnimation : BaseTweenAnimation
    {
        protected override Tween PlayAnimation()
        {
            return RectTransform.DOScale(AnimationSettings.GetUIPosition(Canvas),
                AnimationSettings.Duration).SetEase(AnimationSettings.Ease);
        }
    }
}