namespace UI.Menu.Buttons
{
    public class ReturnToMainMenuButton : BaseButton
    {
        protected override void OnClick()
        {
            GameSession.instance.GameHasStarted = false;
            UiManager.CallbackUiEvent("MainMenu");
        }
    }
}