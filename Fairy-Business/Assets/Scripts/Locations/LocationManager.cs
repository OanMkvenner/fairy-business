using System.Collections.Generic;
using UI.Menu;
using UnityEngine;
using UnityEngine.UI;
using Random = UnityEngine.Random;

namespace Locations
{
    public class LocationManager : MonobheaviourSingletonCustom<LocationManager>
    {
        public List<LocationDefinition> SelectedLocations { get; private set; } = new List<LocationDefinition>();

        [SerializeField] private List<LocationData> locationDataCollection;
        [SerializeField] private Button randomLocationButton;
        [SerializeField] private LocationDefinition locationDefinitionPrefab;
        [SerializeField] private Transform locationsParent;

        private readonly List<LocationDefinition> allAvailableLocations = new List<LocationDefinition>();

        private Dictionary<int, Location> locations;

        public Dictionary<int, Location> Locations
        {
            get => locations;
            set => locations = value;
        }

        private void Awake()
        {
            randomLocationButton.onClick.AddListener(PickRandomLocations);
            SetUpLocations();
        }

        public void CreateLocations()
        {
            locations = new Dictionary<int, Location>();
            int i = 0;

            foreach (LocationDefinition locationDefinition in SelectedLocations)
            {
                GameSession.instance.sceneLocationDefinition[i] = locationDefinition;
                i++;
                Location newLocation = new Location { type = locationDefinition.LocationType, VPGainedOnScorePhase = locationDefinition.VictoryPoints };
            
                locations.Add(i, newLocation);
            }
        }

        public void SetupSelectLocation(LocationDefinition locationDefinition){
            
            if (SelectedLocations.Contains(locationDefinition))
            {
                SelectedLocations.Remove(locationDefinition);
                locationDefinition.IsSelected = false;
                
            } else {
                
                SelectedLocations.Add(locationDefinition);
                locationDefinition.IsSelected = true;
            }
            
            CheckEnoughLocationsSelected();
        }

        private void SetUpLocations()
        {
            foreach (LocationData locationData in locationDataCollection)
            {
                LocationDefinition locationDefinition = Instantiate(locationDefinitionPrefab, locationsParent);
                locationDefinition.InitializeLocationDefinition(locationData);
                allAvailableLocations.Add(locationDefinition);
            }
        }

        private void PickRandomLocations()
        {
            SelectedLocations.Clear();

            List<LocationDefinition> shuffledLocations = new List<LocationDefinition>(allAvailableLocations);
            
            for (int i = 0; i < shuffledLocations.Count; i++)
            {
                int randomIndex = Random.Range(i, shuffledLocations.Count);
                (shuffledLocations[i], shuffledLocations[randomIndex]) = (shuffledLocations[randomIndex], shuffledLocations[i]);
            }
            
            for (int i = 0; i < 3 && i < shuffledLocations.Count; i++)
            {
               SetupSelectLocation(shuffledLocations[i]);
            }
            
        }

        private void CheckEnoughLocationsSelected(){
            if (SelectedLocations.Count == 3){
                MenuManager.OpenMenu(MenuIdentifier.LocationSelectionMenu);
            }
        }
    }
}