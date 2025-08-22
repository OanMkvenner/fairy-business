using DG.Tweening;
using Player;

namespace Locations
{
    public class LocationAnimation
    {
        private float initalPauseTime = 0.5f;
        private Ease roationEaseMode = Ease.InOutCubic;
        private Sequence sequence;

        public void UpdateLocationAnimation(PlayerLine[] lines)
        {
            if(sequence != null)
                sequence.Kill();
            
            sequence = DOTween.Sequence();

            for (int i = 0; i < lines.Length; i++)
            {
                LocationDefinition location = LocationManager.instance.Locations[i];

                if (location.currentOwner == PlayerColor.Neutral)
                {
                    
                }
            }
        }
    }
}