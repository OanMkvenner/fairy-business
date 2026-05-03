using System.Collections.Generic;
using DG.Tweening;
using Player;
using UnityEngine;

namespace Locations
{
    public class LocationAnimation
    {
        private float initalPauseTime = 0.5f;
        private Ease rotationEaseMode = Ease.InOutCubic;
        private Sequence sequence;
        private float duration = 0.6f;

        public void UpdateLocationAnimation(List<LocationDefinition> locations)
        {
            if(sequence != null)
                sequence.Kill();
            
            sequence = DOTween.Sequence();

            foreach (LocationDefinition location in locations)
            {
                if (locations.Count == 0)
                {
                    Debug.LogWarning("[LocationAnimation] No Location for animation found!");
                    return;
                }
                
                Sequence locationSequence = DOTween.Sequence();
                
                Transform targetPositionTransform = GetTargetTransform(location.CurrentOwner, location.PlayerLine);
                
                if (targetPositionTransform == null)
                {
                    Debug.LogWarning($"Target-Transform for {location.CurrentOwner} is null!");
                    continue;
                }
                
                Tween popArtifact = location.Artifact.GetComponent<RectTransform>().DOPunchScale(Vector3.one * 0.2f, duration);
                
                Tween moveTween = location.MoveObject.MoveY(targetPositionTransform.position.y, duration)
                    .SetEase(rotationEaseMode);

                Tween rotateTween = location.MoveObject.Rotate(targetPositionTransform.localEulerAngles.z, duration)
                    .SetEase(rotationEaseMode);
                
                //Hover Buttons are invisible needs, but are children from the locations 
                Tween rotateHoverButton = location.LocationHoverButtons[0].transform.parent.GetComponent<RectTransform>()
                    .DORotate(new Vector3(0, 0, 0), duration);

                locationSequence.Join(moveTween);
                locationSequence.Join(rotateTween);
                locationSequence.Join(rotateHoverButton);
                locationSequence.Append(popArtifact).OnComplete(() =>
                {
                    //Set the scale and size of parent
                    location.transform.SetParent(targetPositionTransform);
                    location.RectTransform.anchorMin = Vector2.zero;
                    location.RectTransform.anchorMax = Vector2.one;
                    location.RectTransform.offsetMin = Vector2.zero;
                    location.RectTransform.offsetMax = Vector2.zero;
                });

                sequence.Join(locationSequence);
            }
        }
    
        private Transform GetTargetTransform(PlayerColor owner, PlayerLine playerLine)
        {
            return owner switch
            {
                PlayerColor.Neutral => playerLine.neutralPosition,
                PlayerColor.Blue => playerLine.bluePosition,
                PlayerColor.Red => playerLine.redPosition,
                _ => null
            };
        }
    }
}