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
        [Space]
        [SerializeField] private TextMeshProUGUI artefactTitleText;
        [SerializeField] private TextMeshProUGUI artefactDescriptionText;
        [Header("Objects that are disabled in selection view")]
        [SerializeField] private List<GameObject> objectsToDisableWhileNotInGameView;

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
                currentOwnerText.text = GetOwnerLocalizedString(hoveredLocation.CurrentOwner, activeLanguageCode);
                bankNameText.text = GetLocalizedBankName(hoveredLocation.BankWrapper, activeLanguageCode);
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

        private string GetOwnerLocalizedString(PlayerColorIdentifier playerColorIdentifier, string activeLanguageCode)
        {
            switch (playerColorIdentifier)
            {
                case PlayerColorIdentifier.Blue:
                    return "Fairy Ink";
                case PlayerColorIdentifier.Red:
                    return "Evil Corp";
                default:
                    return "no one";
            }
        }

        private string GetLocalizedBankName(BankWrapper bankWrapper, string activeLanguageCode)
        {
            switch (bankWrapper.BankIdentifier)
            {
                case BankIdentifier.CasterCard:
                    return "Caster Card";
                case BankIdentifier.KingdomExpress:
                    return "Kingdom Express";
                case BankIdentifier.Vizard:
                    return "Vizard";
                default:
                    return "No one";
            }
        }
    }
}