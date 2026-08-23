using UI.Menu.BaseMenu;
using UnityEngine;

namespace UI.Menu.Buttons
{
    public class CloseMenuButton : BaseButton
    {
        [SerializeField] private MenuIdentifier menuIdentifier;

        protected override void OnClick()
        {
            MenuManager.instance.CloseMenu(menuIdentifier);
        }
    }
}