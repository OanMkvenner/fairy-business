using System.Collections.Generic;
using Locations;
using Player;
using TMPro;
using UI.Gameplay;
using UI.Menu.BaseMenu;
using UI.Menu.Buttons;
using UnityEngine;
using UnityEngine.UI;

namespace UI.Menu
{
    public class HoverSelectionMenu : MenuElement
    {
        [SerializeField] private RectTransform layoutGroup;
        [SerializeField] private List<ScoreHoverUI> scoreHoverUIPrefabs = new();
        [Space]
        [SerializeField] private TextMeshProUGUI bankNameText;
        [SerializeField] private Image bankIconImage;
        [Space]
        [SerializeField] private TextMeshProUGUI currentOwnerText;

        [SerializeField] private Image currentOwnerBackground;
        [Space]
        [SerializeField] private TextMeshProUGUI artefactTitleText;
        [SerializeField] private TextMeshProUGUI artefactDescriptionText;
        [SerializeField] private Image artefactIcon;
        [Header("Objects that are disabled in selection view")]
        [SerializeField] private List<GameObject> objectsToDisableWhileNotInGameView;

        private const string bluePlayerLocalization = "hoverMenu_BluePlayer";
        private const string redPlayerLocalization = "hoverMenu_RedPlayer";
        private const string neutralPlayerLocalization = "hoverMenu_NeutralPlayer";
        private const string ownerTextLocalization = "hoverMenu_OwnerText";
        
        private const string casterCard = "casterCard";
        private const string vizardCard = "vizardCard";
        private const string kingdomExpressCard = "kingdomExpressCard";

        private string activeLanguageCode;

        private void Awake()
        {
            LocationHoverButton.LongPressDetectionEvent += InitializeUI;
            GameSession.OnGameReset += ClearScoreHoverUI;
        }

        private void OnDestroy()
        {
            LocationHoverButton.LongPressDetectionEvent -= InitializeUI;
            GameSession.OnGameReset -= ClearScoreHoverUI;
        }

        private void InitializeUI(LocationDefinition hoveredLocation, bool isTop)
        {
            foreach (GameObject activeStateObjects in objectsToDisableWhileNotInGameView)
            {
                activeStateObjects.SetActive(GameSession.instance.GameHasStarted);
            }
            
            activeLanguageCode = Localizer.instance.GetCurrentlySetLanguage();
            
            if (GameSession.instance.GameHasStarted)
            {
                currentOwnerText.text = Localizer.instance.TranslateToSpecificLanguage(ownerTextLocalization, activeLanguageCode) + 
                                        " " + Localizer.instance.TranslateToSpecificLanguage(GetOwnerLocalizedString(hoveredLocation.CurrentOwner), activeLanguageCode);
                
                bankNameText.text = Localizer.instance.TranslateToSpecificLanguage(GetLocalizedBankName(hoveredLocation.BankWrapper), activeLanguageCode);
                bankIconImage.sprite = hoveredLocation.BankWrapper.BankIcon;
            }

            artefactTitleText.text = Localizer.instance.TranslateToSpecificLanguage(
                hoveredLocation.LocationData.localizationTitleText,
                activeLanguageCode);
            
            artefactDescriptionText.text = Localizer.instance.TranslateToSpecificLanguage(
                hoveredLocation.LocationData.localizationDescriptionText,
                activeLanguageCode);

            artefactIcon.sprite = hoveredLocation.LocationData.artefactIcon;
            
            FlipMenuContent(isTop);
            
            InitializeScoreHoverUI();
            
            base.OpenMenu();
        }

        private void InitializeScoreHoverUI()
        {
            if (!GameSession.instance.GameHasStarted)
            {
                ClearScoreHoverUI();
                return;
            }

            if (GameSession.instance.RoundCounter <= 1)
                return;
            
            if (scoreHoverUIPrefabs[GameSession.instance.RoundCounter - 1].gameObject.activeSelf)
                return;
            
            scoreHoverUIPrefabs[GameSession.instance.RoundCounter - 1].gameObject.SetActive(true);
            scoreHoverUIPrefabs[GameSession.instance.RoundCounter - 1].Init(
                GameSession.instance.VictoryPointCounters[PlayerColorIdentifier.Blue], 
                GameSession.instance.VictoryPointCounters[PlayerColorIdentifier.Red],
                GameSession.instance.RoundCounter - 1
                );
        }

        private void ClearScoreHoverUI()
        {
            foreach (ScoreHoverUI scoreUI in scoreHoverUIPrefabs)
            {
                scoreUI.ClearUI();
                scoreUI.gameObject.SetActive(false);
            }
        }

        private string GetOwnerLocalizedString(PlayerColorIdentifier playerColorIdentifier)
        {
            switch (playerColorIdentifier)
            {
                case PlayerColorIdentifier.Blue:
                    currentOwnerBackground.color = Color.blue;
                    return bluePlayerLocalization;
                case PlayerColorIdentifier.Red:
                    currentOwnerBackground.color = Color.red;
                    return redPlayerLocalization;
                default:
                    currentOwnerBackground.color = Color.white;
                    return neutralPlayerLocalization;
            }
        }

        private string GetLocalizedBankName(BankWrapper bankWrapper)
        {
            switch (bankWrapper.BankIdentifier)
            {
                case BankIdentifier.CasterCard:
                    return casterCard;
                case BankIdentifier.KingdomExpress:
                    return kingdomExpressCard;
                case BankIdentifier.Vizard:
                    return vizardCard;
                default:
                    return "No one";
            }
        }

        private void FlipMenuContent(bool isTop)
        {
            layoutGroup.rotation = isTop ? Quaternion.Euler(0, 0, -180): Quaternion.Euler(0, 0, 0);
        }
    }
}