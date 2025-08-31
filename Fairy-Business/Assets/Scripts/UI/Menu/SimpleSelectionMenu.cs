using System.Collections.Generic;
using Locations;
using UI.Menu.BaseMenu;
using UnityEngine;

namespace UI.Menu
{
    public class SimpleSelectionMenu : MenuElement
    {
        [SerializeField] protected List<Transform> hoverParent;
        [SerializeField] protected LocationUI locationUIPrefab;

        public override void OpenMenu()
        {
            base.OpenMenu();
            InitializeUI(LocationHoverManager.instance.HoveredLocation);
        }

        private void InitializeUI(LocationDefinition locationDefinition)
        {
            int index = LocationHoverManager.instance.LineIndex();
            LocationUI locationUI = Instantiate(locationUIPrefab, hoverParent[index]);
            locationDefinition.InitializeLocationUI(locationUI);
        }

        public override void CloseMenu()
        {
            base.CloseMenu();
            
            foreach (Transform parent in hoverParent)
            {
                foreach (Transform child in parent.transform) {
                    Destroy(child.gameObject);
                }
            }
        }
    }
}