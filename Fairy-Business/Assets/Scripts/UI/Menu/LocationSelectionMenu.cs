using System.Collections.Generic;
using Locations;
using UI.Menu.BaseMenu;
using UnityEngine;

namespace UI.Menu
{
    public class LocationSelectionMenu : MenuElement
    {
        [SerializeField] private GameSession gameSession;
        [SerializeField] private Transform locationsParent;
        [SerializeField] private LocationUI locationUI;

        public override void OpenMenu()
        {
            base.OpenMenu();
            DestroySelection();
            CreateLocationUICards(LocationManager.instance.SelectedLocations);
        }

        public override void CloseMenu()        
        {
            base.CloseMenu();
            
            DestroySelection();
            LocationManager.instance.ResetSelectedLocations();
        }

        private void CreateLocationUICards(List<LocationDefinition> locationDefinitions)
        {
            for (int index = 0; index < locationDefinitions.Count; index++)
            {
                LocationDefinition locationDefenition = locationDefinitions[index];
                LocationUI newLocationUI = Instantiate(locationUI, locationsParent);
                locationDefenition.InitializeLocationUI(newLocationUI);
            }
        }

        private void DestroySelection()
        {
            foreach (Transform child in locationsParent)
            {
                Destroy(child.gameObject);
            }
        }
    }
}