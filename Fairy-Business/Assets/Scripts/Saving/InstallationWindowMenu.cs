using UI.Menu.BaseMenu;

namespace Saving
{
    public class InstallationWindowMenu : MenuElement
    {
        private bool firstTimeOpen = true;
        
        protected override void Start()
        {
            base.Start();

            firstTimeOpen = AppUser.GetOptionOrDefault("installationWindow" ,true);
            
            if (!firstTimeOpen)
                return;
            
            OpenMenu();
            firstTimeOpen = false;
            AppUser.SaveOption("installationWindow", firstTimeOpen);
        }
    }
}