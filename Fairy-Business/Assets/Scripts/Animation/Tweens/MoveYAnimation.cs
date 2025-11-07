using DG.Tweening;

namespace Animation.Tweens
{
    public class MoveYAnimation : BaseAnimation
    {
        protected override Tween PlayAnimation()
        {
            return RectTransform.DOMoveY(AnimationSettings.UIEndPosition.GetUIPosition(Canvas).y, AnimationSettings.Duration).SetEase(AnimationSettings.Ease);
        }
    }
}