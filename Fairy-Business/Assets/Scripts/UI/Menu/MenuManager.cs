using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace UI.Menu
{
    public class MenuManager : MonoBehaviour
    {
        private readonly List<MenuElement> menuElements = new List<MenuElement>();

        public void RegisterMenuElement(MenuElement menuElement)
        {
            menuElements.Add(menuElement);
        }

        public void OpenMenu(MenuIdentifier menuIdentifier)
        {
            MenuElement menuElement = menuElements.FirstOrDefault(a => a.MenuIdentifier == menuIdentifier);

            if (menuElement == null)
            {
                Debug.LogError("[MenuManager]Could not find Menu with identifier: " + menuIdentifier);
                return;
            }
            
            menuElement.OpenMenu();
        }
    }
}