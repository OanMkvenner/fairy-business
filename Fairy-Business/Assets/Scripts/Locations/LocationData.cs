using UnityEngine;

namespace Locations
{
    [CreateAssetMenu(fileName = "LocationData", menuName = "ScriptableObjects/LocationData", order = 1)]
    public class LocationData : ScriptableObject
    {
        public Sprite imageEnabled;
        public Sprite imageDisabled;
        public LocationsType locationType;
        public int VictoryPoints;
        public string locationDescription;
    }

}