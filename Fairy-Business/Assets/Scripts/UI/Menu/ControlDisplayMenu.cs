using UI.Menu.BaseMenu;

namespace UI.Menu
{
    public class ControlDisplayMenu : MenuElement
    {
        public override void OpenMenu()
        {
            base.OpenMenu();

            CameraOpencvLib.instance._StopScanning();

            GameSession.instance.ShowPower();
        }

        public override void CloseMenu()
        {
            base.CloseMenu();
            
            if (GameSession.instance.IsEndOfGame)
            {
                CameraOpencvLib.instance._StopScanning();

                MenuManager.instance.OpenMenu(MenuIdentifier.WinScreen);
                return;
            }

            GameSession.instance.HidePower();
            CameraOpencvLib.instance._StartScanning();
        }
    }
}