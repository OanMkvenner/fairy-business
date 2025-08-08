using System.Collections.Generic;

namespace Locations
{
    public class Location {
        public LocationsType type;
        public int VPGainedOnScorePhase;
        public Dictionary<GameSession.PlayerColor, int> power = new();
        public GameSession.PlayerColor currentOwner;
        public void SetPlayerPower(GameSession.PlayerColor playerIdx, int newPower){
            power[playerIdx] = newPower;
        }
        public int GetPlayerPower(GameSession.PlayerColor playerIdx){
            return power[playerIdx];
        }
    }
}