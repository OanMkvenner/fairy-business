using Locations;
using Player;
using UnityEngine;

namespace Animation.AnimationController
{
    public class ArtefactWinAnimationController : BaseAnimationController
    {
        [SerializeField] private LineIdentifier line;
        [SerializeField] private PlayerColor playerColor;
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
            
            if (locationDefinition.CurrentOwner != playerColor)
            {
                rectTransform.localScale = Vector3.zero;
                return;
            }

            Debug.Log($"StartAnimation Triggered: {locationDefinition.LocationData.locationType} Owner={locationDefinition.CurrentOwner} ControllerColor={playerColor}");

            StartAnimations();
        }
    }
}