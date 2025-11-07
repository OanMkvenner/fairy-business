using Animation.Position;
using DG.Tweening;
using UnityEngine;

namespace Animation.Tweens
{
    public class MoveOffScreenTweenAnimation : BaseTweenAnimation
    {
        [SerializeField] private UIOffScreenPosition uiOffScreenPosition;
        
        protected override Tween PlayAnimation()
        {
            return RectTransform.DOMove(uiOffScreenPosition.GetOffScreenPosition(Canvas), AnimationSettings.Duration)
                .SetEase(AnimationSettings.Ease);
        }
    }
}