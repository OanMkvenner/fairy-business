using UnityEngine;

namespace Locations
{
    [CreateAssetMenu(fileName = "LocationData", menuName = "ScriptableObjects/LocationData", order = 1)]
    public class LocationData : ScriptableObject
    {
        public Sprite imageEnabled;
        public Sprite imageDisabled;
        public LocationsIdentifier LocationIdentifier;
        public ModeIdentifier ModeIdentifier;
        public LocationsIdentifier CouplingIdentifier;
        
        [Header("Game Variables")]
        public int VictoryPoints;

        [Header("Localizations")]
        public string localizationDescriptionText;
        public string localizationTitleText;
        public string localizationKeywordText;
    }
}