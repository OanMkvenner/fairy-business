using UnityEngine;

namespace Locations
{
    [CreateAssetMenu(fileName = "LocationData", menuName = "ScriptableObjects/LocationData", order = 1)]
    public class LocationData : ScriptableObject
    {
        public Sprite imageGameView;
        public Sprite imageEnabled;
        public Sprite imageDisabled;
        public Sprite artefactIcon;
        public Sprite effectEnabledIcon;
        public Sprite effectDisabledIcon;
        public Sprite locationBackground;
        public LocationsIdentifier LocationIdentifier;
        public ModeIdentifier ModeIdentifier;
        
        [Header("Game Variables")]
        public int VictoryPoints;

        [Header("Localizations")]
        public string localizationDescriptionText;
        public string localizationTitleText;
        public string localizationKeywordText;
        public string localizationArtifact;
    }
}