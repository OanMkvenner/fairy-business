using System.Collections.Generic;
using Player;

namespace Locations
{
    public class Location {
        public LocationsType type;
        public int VPGainedOnScorePhase;
        public Dictionary<PlayerColor, int> power = new();
        public PlayerColor currentOwner;
        public void SetPlayerPower(PlayerColor playerIdx, int newPower){
            power[playerIdx] = newPower;
        }
        public int GetPlayerPower(PlayerColor playerIdx){
            return power[playerIdx];
        }
    }
}