using UI.Menu.BaseMenu;
using UnityEngine;

namespace UI.Menu.Buttons
{
    public class OpenMenuButton : BaseButton
    {
        [SerializeField] private MenuIdentifier identifier;
        
        protected override void OnClick()
        {
            MenuManager.instance.OpenMenu(identifier);
        }
    }
}