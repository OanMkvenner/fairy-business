using Locations;
using UI.Menu.BaseMenu;

namespace UI.Menu.Buttons
{
    public class StartNewGame : BaseButton
    {
        protected override void OnClick()
        {
            StartGame();
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