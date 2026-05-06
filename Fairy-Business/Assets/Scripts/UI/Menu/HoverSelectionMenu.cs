using System.Collections.Generic;
using Locations;
using Player;
using TMPro;
using UI.Buttons;
using UI.Gameplay;
using UI.Menu.BaseMenu;
using UnityEngine;
using UnityEngine.UI;

namespace UI.Menu
{
    public class HoverSelectionMenu : MenuElement
    {
        [SerializeField] private ScoreHoverUI scoreHoverUIPrefab;
        [SerializeField] private Transform scoreHoverUIParent;
        [Space]
        [SerializeField] private TextMeshProUGUI bankNameText;
        [SerializeField] private Image bankIconImage;
        [Space]
        [SerializeField] private TextMeshProUGUI currentOwnerText;

        [SerializeField] private Image currentOwnerBackground;
        [Space]
        [SerializeField] private TextMeshProUGUI artefactTitleText;
        [SerializeField] private TextMeshProUGUI artefactDescriptionText;
        [Header("Objects that are disabled in selection view")]
        [SerializeField] private List<GameObject> objectsToDisableWhileNotInGameView;

        private const string bluePlayerLocalization = "hoverMenu_BluePlayer";
        private const string redPlayerLocalization = "hoverMenu_RedPlayer";
        private const string neutralPlayerLocalization = "hoverMenu_NeutralPlayer";
        private const string ownerTextLocalization = "hoverMenu_OwnerText";
        
        private const string casterCard = "casterCard";
        private const string vizardCard = "vizardCard";
        private const string kingdomExpressCard = "kingdomExpressCard";

        private void Awake()
        {
            LocationHoverButton.LongPressDetectionEvent += InitializeUI;
        }

        private void OnDestroy()
        {
            LocationHoverButton.LongPressDetectionEvent -= InitializeUI;
        }

        private void InitializeUI(LocationDefinition hoveredLocation)
        {
            string activeLanguageCode = Localizer.instance.GetCurrentlySetLanguage();

            foreach (GameObject activeStateObjects in objectsToDisableWhileNotInGameView)
            {
                activeStateObjects.SetActive(GameSession.instance.GameHasStarted);
            }
            
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
            
            base.OpenMenu();
        }

        public override void CloseMenu()
        {
            base.CloseMenu();

            foreach (GameObject child in scoreHoverUIParent)
            {
                Destroy(child);
            }
        }

        private string GetOwnerLocalizedString(PlayerColorIdentifier playerColorIdentifier)
        {
            switch (playerColorIdentifier)
            {
                case PlayerColorIdentifier.Blue:
                    return bluePlayerLocalization;
                case PlayerColorIdentifier.Red:
                    return redPlayerLocalization;
                default:
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
    }
}