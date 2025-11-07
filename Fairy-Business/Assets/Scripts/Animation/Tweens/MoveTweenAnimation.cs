using DG.Tweening;

namespace Animation.Tweens
{
    public class MoveTweenAnimation : BaseTweenAnimation
    {
        protected override Tween PlayAnimation()
        {
            return RectTransform.DOMove(AnimationSettings.GetUIPosition(Canvas), 
                AnimationSettings.Duration).SetEase(AnimationSettings.Ease);
        }
    }
}