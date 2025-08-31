using Locations;
using UI.Menu.BaseMenu;
using UnityEngine;

namespace UI.Menu
{
    public class SimpleSelectionMenu : MenuElement
    {
        [SerializeField] private Transform selectionParent;
        [SerializeField] private LocationUI locationUIPrefab;

        public override void OpenMenu()
        {
            base.OpenMenu();
            InitializeUI(LocationManager.instance.HoveredLocation);
        }

        private void InitializeUI(LocationDefinition locationDefinition)
        {
            LocationUI locationUI = Instantiate(locationUIPrefab, selectionParent);
            locationDefinition.InitializeLocationUI(locationUI);
        }
    }
}