using DG.Tweening;

namespace Animation.Tweens
{
    public class MoveTweenAnimation : BaseTweenAnimation
    {
        protected override Tween PlayAnimation()
        {
            Tween tween = RectTransform.DOMove(AnimationSettings.GetUIPosition(Canvas), 
                AnimationSettings.Duration).SetEase(AnimationSettings.Ease);
            
            ApplyEffect(tween);
            
            return tween;
        }
    }
}