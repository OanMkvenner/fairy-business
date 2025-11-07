using DG.Tweening;

namespace Animation.Tweens
{
    public class MoveYTweenAnimation : BaseTweenAnimation
    {
        protected override Tween PlayAnimation()
        {
            return RectTransform.DOMoveY(AnimationSettings.GetUIPosition(Canvas).y, 
                AnimationSettings.Duration).SetEase(AnimationSettings.Ease);
        }
    }
}