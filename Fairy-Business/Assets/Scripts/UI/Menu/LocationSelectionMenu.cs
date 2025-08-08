using UnityEngine;
using UnityEngine.UI;

namespace UI.Menu
{
    public class LocationSelectionMenu : MenuElement
    {
        [SerializeField] private Button closeButton;
        [SerializeField] private Button startGameButton;

        [SerializeField] private UiGraphCallback uiGraphCallback;
 
        private void Awake()
        {
            closeButton.onClick.AddListener(CloseMenu);
            startGameButton.onClick.AddListener(StartNewGame);
        }

        private void StartNewGame()
        {
            uiGraphCallback.callCustomUiEvent("EnoughLocationsSelected");
        }
    }
}