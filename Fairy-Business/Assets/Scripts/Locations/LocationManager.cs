using System;
using System.Collections.Generic;
using HelperClasses;
using Player;
using UI.Menu;
using UnityEngine;
using UnityEngine.UI;
using Random = UnityEngine.Random;

namespace Locations
{
    public class LocationManager : MonobheaviourSingletonCustom<LocationManager>
    {
        public List<LocationDefinition> SelectedLocations { get; private set; } = new List<LocationDefinition>();

        public Dictionary<int, Location> Locations
        {
            get => locations;
            set => locations = value;
        }

        [SerializeField] private List<LocationData> locationDataCollection;

        [SerializeField] private Button randomLocationButton;

        [SerializeField] private LocationDefinition locationDefinitionPrefab;

        [SerializeField] private Transform locationsParent;

        [Header("GameField")]
        [SerializeField] private Transform gameFieldParent;

        [SerializeField] private PlayerLine[] lines = new PlayerLine[3];

        private List<LocationDefinition> gameLocationDefinitions = new List<LocationDefinition>();

        /// <summary>
        /// Locations in the Game view, that can be moved around.
        /// </summary>
        public List<LocationDefinition> GameLocationDefinitions
        {
            get => gameLocationDefinitions;
            set => gameLocationDefinitions = value;
        }

        private readonly List<LocationDefinition> allAvailableLocations = new List<LocationDefinition>();
        private Dictionary<int, Location> locations;

        private void Awake()
        {
            randomLocationButton.onClick.AddListener(PickRandomLocations);
            SetUpLocations();
        }

        public void CreateGameLocations()
        {
            locations = new Dictionary<int, Location>();
            
            // apply the power setups of 5-3, 4-4 and 3-5 randomly over the locations
            List<int> ints = new List<int>{5,4,3};
            Utilities.ShuffleList(ints);

            for (int index = 0; index < SelectedLocations.Count; index++)
            {
                LocationDefinition locationDefinition = SelectedLocations[index];
                LocationDefinition gameLocationDefinition = Instantiate(locationDefinitionPrefab, gameFieldParent);
                gameLocationDefinition.InitializeLocationDefinition(locationDefinition.LocationData);
                gameLocationDefinition.IsSelected = true;
                gameLocationDefinitions.Add(gameLocationDefinition);
                gameLocationDefinition.SetPosition(lines[index].neutralPosition.position);

                Location newLocation = new Location
                {
                    type = gameLocationDefinition.LocationType,
                    VPGainedOnScorePhase = gameLocationDefinition.VictoryPoints
                };
                
                int powerRed = ints[index];
                newLocation.SetPlayerPower(PlayerColor.Red, powerRed);
                newLocation.SetPlayerPower(PlayerColor.Blue, (8 - powerRed));
                locations.Add(index, newLocation);
            }

            List<LocationDefinition> shuffledGameLocations = gameLocationDefinitions.Shuffled();
            
            
        }

        public void MoveLocationOnLine(GameObject obj, int lineIndex, float t)
        {
            if (lineIndex < 0 || lineIndex >= lines.Length) return;
            
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

            List<LocationDefinition> shuffledLocations = allAvailableLocations.Shuffled();

            // Nimm die ersten 3 (oder weniger, falls die Liste kürzer ist)
            for (int i = 0; i < Math.Min(3, shuffledLocations.Count); i++)
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