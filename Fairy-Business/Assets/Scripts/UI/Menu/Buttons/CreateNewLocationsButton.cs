using Locations;

namespace UI.Menu.Buttons
{
    public class CreateNewLocationsButton : BaseButton
    {
        protected override void OnClick()
        {
            LocationManager.instance.SetUpLocations();
        }
    }
}