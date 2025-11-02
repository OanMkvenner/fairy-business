using DG.Tweening;

namespace Animation
{
    public class MoveYAnimation : BaseAnimation
    {
        protected override Tween PlayAnimation()
        {
            return RectTransform.DOMoveY(AnimationSettings.UIEndPosition.GetUIPosition(Canvas).y, AnimationSettings.Duration).SetEase(AnimationSettings.Ease);
        }
    }
}