using Locations;

namespace UI.Menu.Buttons
{
    public class PickRandomLocationsButton : BaseButton
    {
        protected override void OnClick()
        {
            LocationManager.instance.PickRandomLocations();  
        }
    }
}