using System;
using UI.Menu.BaseMenu;

namespace UI.Menu
{
    public class ControlDisplayMenu : MenuElement
    {
        public override void OpenMenu()
        {
            base.OpenMenu();

            CameraOpencvLib.instance._StopScanning();
        }

        public override void CloseMenu()
        {
            base.CloseMenu();
            
            CameraOpencvLib.instance._StartScanning();

            if (!GameSession.instance.IsEndOfGame) 
                return;
            
            CameraOpencvLib.instance._StopScanning();
            
            MenuManager.instance.OpenMenu(MenuIdentifier.WinScreen);
        }
    }
}