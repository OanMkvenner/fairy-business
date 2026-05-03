using Locations;
using Player;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace UI.Gameplay
{
    [RequireComponent(typeof(Image))]
    public class PlayerInfoUI : MonoBehaviour
    {
        [SerializeField] private LineIdentifier line;
        [SerializeField] private PlayerColor playerColor;
        [SerializeField] private Sprite highestScoreSprite;

        [Space]
        [SerializeField] private TextMeshProUGUI locationVictoryPoints;
        [SerializeField] private TextMeshProUGUI keyword;

        private Image scoreBackground;
        private Sprite defaultScoreSprite;

        private void Awake()
        {
            scoreBackground = GetComponent<Image>();
            defaultScoreSprite = scoreBackground.sprite;
            
            LocationDefinition.OnCurrentOwnerChangedEvent += ChangeScoreBackground;
            LocationManager.OnNewLocationCreatedEvent += AssignLocationKeyword;
            
        }

        private void OnDestroy()
        {
            LocationDefinition.OnCurrentOwnerChangedEvent -= ChangeScoreBackground;
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
            
            int victoryPoints = locationDefinition.VictoryPoints;
            this.locationVictoryPoints.text = victoryPoints.ToString();
        }

        private void ChangeScoreBackground(LocationDefinition locationDefinition)
        {
            if (locationDefinition.PlayerLine.line != this.line)
                return;
            
            if (locationDefinition.CurrentOwner != playerColor)
            {
                scoreBackground.sprite = defaultScoreSprite;
            }
            else
            {
                scoreBackground.sprite = highestScoreSprite;
            }
        }
    }
}