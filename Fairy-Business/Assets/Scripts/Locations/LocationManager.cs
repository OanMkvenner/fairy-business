using System;
using System.Collections.Generic;
using HelperClasses;
using Player;
using UI.Menu;
using UnityEngine;
using UnityEngine.UI;

namespace Locations
{
    public class LocationManager : MonobheaviourSingletonCustom<LocationManager>
    {
        public List<LocationDefinition> SelectedLocations { get; private set; } = new List<LocationDefinition>();

        public Dictionary<int, LocationDefinition> Locations { get; set; } 

        [SerializeField] private List<LocationData> locationDataCollection;
        [SerializeField] private Button randomLocationButton;
        [SerializeField] private LocationDefinition locationDefinitionPrefab;
        [SerializeField] private Transform locationsParent;

        [Header("GameField")]
        [SerializeField] private Transform gameFieldParent;
        [SerializeField] private PlayerLine[] lines = new PlayerLine[3];
        [SerializeField] private List<Color> lineColors = new List<Color>();

        /// <summary>
        /// Locations in the Game view, that can be moved around.
        /// </summary>
        public List<LocationDefinition> GameLocationDefinitions { get; private set; } 

        private readonly List<LocationDefinition> allAvailableLocations = new List<LocationDefinition>();

        private void Awake()
        {
            randomLocationButton.onClick.AddListener(PickRandomLocations);
            SetUpLocations();
        }

        public void CreateGameLocations()
        {
            Locations = new Dictionary<int, LocationDefinition>();
            
            // apply the power setups of 5-3, 4-4 and 3-5 randomly over the locations
            List<int> ints = new List<int>{5,4,3};
            Utilities.ShuffleList(ints);

            for (int index = 0; index < SelectedLocations.Count; index++)
            {
                LocationDefinition locationDefinition = SelectedLocations[index];
                LocationDefinition gameLocationDefinition = Instantiate(locationDefinitionPrefab, gameFieldParent);
                gameLocationDefinition.InitializeLocationDefinition(locationDefinition.LocationData);
                gameLocationDefinition.IsSelected = true;
                gameLocationDefinition.SetPosition(lines[index].neutralPosition.position);
                gameLocationDefinition.SetBackgroundColor(lineColors[index]);
                
                GameLocationDefinitions.Add(gameLocationDefinition);
                
                
                int powerRed = ints[index];
                gameLocationDefinition.SetPlayerPower(PlayerColor.Red, powerRed);
                gameLocationDefinition.SetPlayerPower(PlayerColor.Blue, (8 - powerRed));
                Locations.Add(index, gameLocationDefinition);
            }
            
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