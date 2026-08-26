using System;
using Locations;
using UI.Menu.BaseMenu;
using UnityEngine;

namespace UI.Menu.Buttons
{
    public class StartNewGame : BaseButton
    {
        protected override void OnClick()
        {
            StartGame();
        }

        private void OnEnable()
        {
            Debug.Log("Bub");
        }

        public void StartGame()
        {
            if (!LocationManager.instance.CheckEnoughLocationsSelected())
                return;
            UiManager.CallbackUiEvent("EnoughLocationsSelected");
            GameSession.instance.GameHasStarted = true;
            MenuManager.instance.CloseMenu(MenuIdentifier.LocationSelectionMenu);
        }
    }
}