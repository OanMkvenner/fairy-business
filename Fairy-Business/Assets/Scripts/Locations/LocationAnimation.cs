using System.Collections.Generic;
using DG.Tweening;
using Player;
using UnityEngine;

namespace Locations
{
    public class LocationAnimation
    {
        private float initalPauseTime = 0.5f;
        private Ease roationEaseMode = Ease.InOutCubic;
        private Sequence sequence;
        private float duration = 0.6f;

        public void UpdateLocationAnimation(List<LocationDefinition> locations)
        {
            if(sequence != null)
                sequence.Kill();
            
            sequence = DOTween.Sequence();

            foreach (LocationDefinition location in locations)
            {
                if (locations == null || locations.Count == 0)
                {
                    Debug.LogWarning("[LocationAnimation] No Location for animation found!");
                    return;
                }
                
                Transform targetPositionTransform = GetTargetTransform(location.currentOwner, location.PlayerLine);
                
                if (targetPositionTransform == null)
                {
                    Debug.LogWarning($"Ziel-Transform für {location.currentOwner} ist null!");
                    continue;
                }

                // Animationen starten
                Tween moveTween = location.MoveY(targetPositionTransform.position.y, duration)
                    .SetEase(roationEaseMode);

                Tween rotateTween = location.Rotate(targetPositionTransform.localEulerAngles.z, duration)
                    .SetEase(roationEaseMode);

                // Tweens zur Sequenz hinzufügen (parallel abspielen)
                sequence.Join(moveTween);
                sequence.Join(rotateTween);
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