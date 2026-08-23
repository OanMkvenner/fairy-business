using UI.Menu.BaseMenu;

namespace UI.Menu.Buttons
{
    public class StartNewGame : BaseButton
    {
        protected override void OnClick()
        {
            GameSession.instance.GameHasStarted = true;
            UiManager.CallbackUiEvent("EnoughLocationsSelected");
            MenuManager.instance.CloseMenu(MenuIdentifier.LocationSelectionMenu);
        }
    }
}