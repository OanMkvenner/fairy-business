using DG.Tweening;

namespace Animation
{
    public class MoveAnimation : BaseAnimation
    {
        protected override Tween PlayAnimation()
        {
            return RectTransform.DOMove(AnimationSettings.UIEndPosition.GetUIPosition(Canvas), 
                AnimationSettings.Duration).SetEase(AnimationSettings.Ease);
        }
    }
}