using DG.Tweening;

namespace Animation.Tweens
{
    public class MoveYTweenAnimation : BaseTweenAnimation
    {
        protected override Tween PlayAnimation()
        {
            Tween tween = RectTransform.DOMoveY(AnimationSettings.UiEndPosition.position.y, 
                AnimationSettings.Duration).SetEase(AnimationSettings.Ease);
            
            ApplyEffect(tween);
            
            return tween;
        }
    }
}