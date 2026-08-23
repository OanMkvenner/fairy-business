using UI.Menu.BaseMenu;
using UnityEngine;

namespace UI.Menu.Buttons
{
    public class ReturnToMainMenuButton : BaseButton
    {
        [SerializeField] private MenuIdentifier menuToClose;
        
        protected override void OnClick()
        {
            MenuManager.instance.CloseMenu(menuToClose);
            GameSession.instance.GameHasStarted = false;
            UiManager.CallbackUiEvent("MainMenu");
        }
    }
}