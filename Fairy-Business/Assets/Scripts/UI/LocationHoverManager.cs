using Locations;
using Player;

namespace UI
{
    public class LocationHoverManager : MonobehaviourSingletonCustom<LocationHoverManager>
    {
        public PlayerColor CurrentPlayerColor { get; set; }
        public LineIdentifier CurrentLine { get; set; }
        public LocationDefinition HoveredLocation { get; set; }
        
        
        public int LineIndex()
        {
            if (CurrentLine == LineIdentifier.Left && CurrentPlayerColor == PlayerColor.Red)
                return 0;
            
            if(CurrentLine == LineIdentifier.Left && CurrentPlayerColor == PlayerColor.Blue)
                return 0;

            if (CurrentLine == LineIdentifier.Right && CurrentPlayerColor == PlayerColor.Red)
                return 2;

            if (CurrentLine == LineIdentifier.Right && CurrentPlayerColor == PlayerColor.Blue)
                return 2;

            if (CurrentLine == LineIdentifier.Middle && CurrentPlayerColor == PlayerColor.Red)
                return 1;
            
            if(CurrentLine == LineIdentifier.Middle && CurrentPlayerColor == PlayerColor.Blue)
                return 1;

            return 0;
        }
    }
}