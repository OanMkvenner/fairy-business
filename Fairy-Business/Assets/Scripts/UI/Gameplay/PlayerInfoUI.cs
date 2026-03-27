using Locations;
using Player;
using TMPro;
using UnityEngine;

namespace UI.Gameplay
{
    public class PlayerInfoUI : MonoBehaviour
    {
        [SerializeField] private LineIdentifier line;
        [SerializeField] private PlayerColor playerColor;
        [SerializeField] private Sprite highestScoreSprite;
        [SerializeField] private Sprite defaultScoreSprite;
        
        [Space]
        [SerializeField] private TextMeshProUGUI power;
        [SerializeField] private TextMeshProUGUI keyword;

        private void Awake()
        {
            LocationDefinition.OnNewPowerAddedEvent += SetTurnPoints;
            LocationManager.OnNewLocationCreatedEvent += AssignLocationKeyword;
        }

        private void OnDestroy()
        {
            LocationDefinition.OnNewPowerAddedEvent -= SetTurnPoints;
            LocationManager.OnNewLocationCreatedEvent -= AssignLocationKeyword;
        }

        private void AssignLocationKeyword(LocationDefinition locationDefinition)
        {
            if (locationDefinition.PlayerLine.line != line)
                return;
            
            string activeLanguageCode = Localizer.instance.GetCurrentlySetLanguage();
            string localizedKeyword = Localizer.instance.TranslateToSpecificLanguage(locationDefinition.LocationData.localizationKeywordText, 
                activeLanguageCode);
            
            keyword.text = localizedKeyword;
        }
        
        private void SetTurnPoints(PlayerColor currentPlayerColor, LocationDefinition locationDefinition)
        {
            if (playerColor != currentPlayerColor)
                return;
            
            if (locationDefinition.PlayerLine.line != this.line)
                return;
            
            int power = locationDefinition.GetPlayerPower(currentPlayerColor);
            this.power.text = power.ToString();
        }
    }
}