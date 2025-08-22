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

        public List<LocationDefinition> GameLocations { get; set; } 

        [SerializeField] private List<LocationData> locationDataCollection;
        [SerializeField] private Button randomLocationButton;
        [SerializeField] private LocationDefinition locationDefinitionPrefab;
        [SerializeField] private Transform locationsParent;

        [Header("GameField")]
        [SerializeField] private Transform gameFieldParent;
        [SerializeField] private PlayerLine[] lines = new PlayerLine[3];
        [SerializeField] private List<Color> lineColors = new List<Color>();

        private readonly List<LocationDefinition> allAvailableLocations = new List<LocationDefinition>();
        private LocationAnimation locationAnimation;

        private void Awake()
        {
            randomLocationButton.onClick.AddListener(PickRandomLocations);
            SetUpLocations();
        }

        public void CreateGameLocations()
        {
            GameLocations = new List<LocationDefinition>();
            
            // apply the power setups of 5-3, 4-4 and 3-5 randomly over the locations
            List<int> ints = new List<int>{5,4,3};
            Utilities.ShuffleList(ints);

            for (int index = 0; index < SelectedLocations.Count; index++)
            {
                LocationDefinition locationDefinition = SelectedLocations[index];
                LocationDefinition gameLocationDefinition = Instantiate(locationDefinitionPrefab, gameFieldParent);
                gameLocationDefinition.InitializeLocationDefinition(locationDefinition.LocationData);
                gameLocationDefinition.IsSelected = true;

                int powerRed = ints[index];
                gameLocationDefinition.SetPlayerPower(PlayerColor.Red, powerRed);
                gameLocationDefinition.SetPlayerPower(PlayerColor.Blue, (8 - powerRed));
                GameLocations.Add(gameLocationDefinition);
            }

            AssignLocationOwner();
            AssignBackgroundColorAndPlayerLine();

            foreach (LocationDefinition gameLocation in GameLocations)
            {
                gameLocation.SetPosition(gameLocation.PlayerLine.neutralPosition.position);
            }
        }

        public void ResetLocations()
        {
            if (GameLocations == null)
                return;
            
            GameLocations.Clear();
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

        public void UpdateLocationAnimation()
        {
            if(locationAnimation == null)
                locationAnimation = new LocationAnimation();
            
            locationAnimation.UpdateLocationAnimation(GameLocations);
        }

        /// <summary>
        /// Sets Player Owner at the start of the game and then shuffles the locations.
        /// </summary>
        private void AssignLocationOwner()
        {
            GameLocations[0].currentOwner = PlayerColor.Blue;
            GameLocations[1].currentOwner = PlayerColor.Red;
            GameLocations[2].currentOwner = PlayerColor.Neutral;
            
            GameLocations = GameLocations.Shuffled();
        }

        /// <summary>
        /// Assigns Background Color and PlayerLine.
        /// </summary>
        private void AssignBackgroundColorAndPlayerLine()
        {
            GameLocations[0].SetBackgroundColor(lineColors[0]);
            GameLocations[0].PlayerLine = lines[0];
            GameLocations[1].SetBackgroundColor(lineColors[1]);
            GameLocations[1].PlayerLine = lines[1];
            GameLocations[2].SetBackgroundColor(lineColors[2]);
            GameLocations[2].PlayerLine = lines[2];
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