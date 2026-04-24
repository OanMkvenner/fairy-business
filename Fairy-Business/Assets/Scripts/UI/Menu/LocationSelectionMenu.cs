using System.Collections.Generic;
using System.Linq;
using Locations;
using UI.Menu.BaseMenu;
using UnityEngine;
using UnityEngine.UI;

namespace UI.Menu
{
    public class LocationSelectionMenu : MenuElement
    {
        [SerializeField] private Button startGameButton;
        [SerializeField] private Button randomLocationButton;

        [SerializeField] private GameSession gameSession;
        [SerializeField] private List<Transform> locationsParents = new();
        [SerializeField] private LocationUI locationUI;
 
        private void Awake()
        {
            startGameButton.onClick.AddListener(StartNewGame);
            randomLocationButton.onClick.AddListener(PickRandomLocations);
        }

        public override void OpenMenu()
        {
            base.OpenMenu();
            CreateLocationUICards(LocationManager.instance.SelectedLocations);
        }

        public override void CloseMenu()
        {
            base.CloseMenu();
            
            LocationManager.instance.ResetSelectedLocations();
            
            foreach (Transform parentTransform in locationsParents)
            {
                parentTransform.Cast<Transform>().ToList().ForEach(child => Destroy(child.gameObject));
            }
        }

        private void CreateLocationUICards(List<LocationDefinition> locationDefinitions)
        {
            for (int index = 0; index < locationDefinitions.Count; index++)
            {
                LocationDefinition locationDefenition = locationDefinitions[index];
                LocationUI newLocationUI = Instantiate(locationUI, locationsParents[index]);
                newLocationUI.GetComponent<RectTransform>().anchorMin = Vector2.zero;
                newLocationUI.GetComponent<RectTransform>().anchorMax = Vector2.one;
                newLocationUI.GetComponent<RectTransform>().offsetMin = Vector2.zero;
                newLocationUI.GetComponent<RectTransform>().offsetMax = Vector2.zero;
                locationDefenition.InitializeLocationUI(newLocationUI);
                
            }
        }

        private void StartNewGame()
        {
            UiManager.CallbackUiEvent("EnoughLocationsSelected");
            CloseMenu();
        }

        private void PickRandomLocations()
        {
            LocationManager.instance.PickRandomLocations();    
        }
    }
}