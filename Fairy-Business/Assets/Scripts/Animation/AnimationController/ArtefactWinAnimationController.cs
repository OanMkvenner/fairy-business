using Locations;
using Player;
using UnityEngine;

namespace Animation.AnimationController
{
    public class ArtefactWinAnimationController : BaseAnimationController
    {
        [SerializeField] private LineIdentifier line;
        [SerializeField] private PlayerColorIdentifier playerColorIdentifier;
        [SerializeField] private RectTransform rectTransform;
        
        protected override void Awake()
        {
            base.Awake();
            
            LocationDefinition.OnCurrentOwnerChangedEvent += StartAnimation;
        }

        private void OnDestroy()
        {
            LocationDefinition.OnCurrentOwnerChangedEvent -= StartAnimation;
        }

        private void StartAnimation(LocationDefinition locationDefinition)
        {
            if (locationDefinition.PlayerLine.line != line)
                return;
            
            if (locationDefinition.CurrentOwner != playerColorIdentifier || locationDefinition.CurrentOwner == PlayerColorIdentifier.Neutral)
            {
                rectTransform.localScale = Vector3.zero;
                return;
            }
            
            StartAnimations();
        }
    }
}