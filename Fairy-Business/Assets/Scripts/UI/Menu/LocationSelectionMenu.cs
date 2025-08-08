using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace UI.Menu
{
    public class LocationSelectionMenu : MenuElement
    {
        [SerializeField] private Button closeButton;
        [SerializeField] private Button startGameButton;

        [SerializeField] private UiGraphCallback uiGraphCallback;
        [SerializeField] private GameSession gameSession;
        [SerializeField] private Transform locationsParent;
        [SerializeField] private LocationUI locationUI;
 
        private void Awake()
        {
            closeButton.onClick.AddListener(CloseMenu);
            startGameButton.onClick.AddListener(StartNewGame);
        }

        public override void OpenMenu()
        {
            base.OpenMenu();
            CreateLocationCards(gameSession.SelectedLocationTypes);
        }

        public override void CloseMenu()
        {
            base.CloseMenu();
            foreach (Transform child in locationsParent.transform) {
                GameObject.Destroy(child.gameObject);
            }
        }

        private void CreateLocationCards(List<LocationDefenition> locationDefenitions)
        {
            foreach (LocationDefenition locationDefenition in locationDefenitions)
            {
                LocationUI newLocationUI = Instantiate(locationUI, locationsParent);
                locationDefenition.InitializeLocationUI(newLocationUI);
            }
        }

        private void StartNewGame()
        {
            uiGraphCallback.callCustomUiEvent("EnoughLocationsSelected");
        }
    }
}