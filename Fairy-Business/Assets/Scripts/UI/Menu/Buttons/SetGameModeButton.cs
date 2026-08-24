using Locations;
using UnityEngine;

namespace UI.Menu.Buttons
{
    public class SetGameModeButton : BaseButton
    {
        [SerializeField] private ModeIdentifier modeIdentifier;
        
        protected override void OnClick()
        {
            LocationManager.instance.CurrentMode = modeIdentifier;
        }
    }
}