using System;
using System.Collections.Generic;
using Locations;
using UI.Menu.BaseMenu;
using UnityEngine;

namespace UI.Menu
{
    public class LocationSelectionMenu : MenuElement {
        static public LocationSelectionMenu instance;
        [SerializeField] private GameSession gameSession;
        [SerializeField] private Transform locationsParent;
        [SerializeField] private LocationUI locationUI;

        private void Awake() {
            instance = this;
        }

        public override void OpenMenu()
        {
            base.OpenMenu();
            CreateLocationUICards(LocationManager.instance.SelectedLocations);
        }

        public override void CloseMenu()        
        {
            base.CloseMenu();
            
            DestroySelection();
            LocationManager.instance.ResetSelectedLocations();
        }

        static public void CreateLocationUICards(List<LocationDefinition> locationDefinitions)
        {
            instance.DestroySelection();
            for (int index = 0; index < locationDefinitions.Count; index++)
            {
                LocationDefinition locationDefenition = locationDefinitions[index];
                LocationUI newLocationUI = Instantiate(instance.locationUI, instance.locationsParent);
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