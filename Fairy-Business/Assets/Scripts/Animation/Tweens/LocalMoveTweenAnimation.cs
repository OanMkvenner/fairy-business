using DG.Tweening;

namespace Animation.Tweens
{
    public class LocalMoveTweenAnimation : BaseTweenAnimation
    {
        protected override Tween PlayAnimation()
        {
            Tween tween = RectTransform.DOLocalMove(AnimationSettings.UiEndPosition.position, 
                AnimationSettings.Duration).SetEase(AnimationSettings.Ease);
            
            ApplyEffect(tween);
            
            return tween;
        }
    }
}