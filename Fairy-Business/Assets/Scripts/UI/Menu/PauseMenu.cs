using System;
using Locations;
using UI.Menu.BaseMenu;
using UnityEngine;
using UnityEngine.UI;

namespace UI.Menu
{
    public class PauseMenu : MenuElement
    {
        [SerializeField] private Button returnToStartScreen;
        [SerializeField] private Button languageButton;
        

        private void Awake()
        {
            returnToStartScreen.onClick.AddListener(OnReturnToStartScreenButtonClicked);
            languageButton.onClick.AddListener(OnLanguageButtonClicked);
        }

        private void OnDestroy()
        {
            returnToStartScreen.onClick.RemoveAllListeners();
            languageButton.onClick.RemoveAllListeners();
        }

        private void OnLanguageButtonClicked()
        {
            string languageCode = "fr";
            Localizer.instance.SetLanguage(languageCode);
            AppUser.SaveOption("currentLanguageCode", languageCode);
        }

        private void OnReturnToStartScreenButtonClicked()
        {
            CloseMenu();
            LocationManager.instance.ResetGameLocations();
            UiManager.CallbackUiEvent("MainMenu");
        }
    }
}