using Locations;
using UI.Menu.BaseMenu;
using UnityEngine;
using UnityEngine.UI;

namespace UI.Menu
{
    public class PauseMenu : MenuElement
    {
        [SerializeField] private Button returnToStartScreen;

        private void Awake()
        {
            returnToStartScreen.onClick.AddListener(OnReturnToStartScreenButtonClicked);
        }

        private void OnDestroy()
        {
            returnToStartScreen.onClick.RemoveAllListeners();
        }

        private void OnReturnToStartScreenButtonClicked()
        {
            CloseMenu();
            GameSession.instance.GameHasStarted = false;
            UiManager.CallbackUiEvent("MainMenu");
        }
    }
}