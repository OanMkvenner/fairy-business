using Locations;
using UI.Menu.BaseMenu;

namespace UI.Menu.Buttons
{
    public class PickRandomLocationsButton : BaseButton
    {
        protected override void OnClick()
        {
            LocationManager.instance.PickRandomLocations();  
            MenuManager.instance.OpenMenu(MenuIdentifier.LocationSelectionMenu);
        }
    }
}