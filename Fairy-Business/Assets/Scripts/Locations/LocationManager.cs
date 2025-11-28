using System;
using System.Collections.Generic;
using HelperClasses;
using Player;
using UI.Menu.BaseMenu;
using UnityEngine;

namespace Locations
{
    public class LocationManager : MonobehaviourSingletonCustom<LocationManager>
    {
        public List<LocationDefinition> SelectedLocations { get; private set; } = new List<LocationDefinition>();
        public List<LocationDefinition> GameLocations { get; private set; } 

        [SerializeField] private List<LocationData> locationDataCollection;
        [SerializeField] private LocationDefinition locationDefinitionPrefab;
        [SerializeField] private Transform locationsParent;

        [Header("GameField")]
        [SerializeField] private Transform gameFieldParent;
        [SerializeField] private PlayerLine[] lines = new PlayerLine[3];

        private readonly List<LocationDefinition> allAvailableLocations = new List<LocationDefinition>();
        private LocationAnimation locationAnimation;

        private void Awake()
        {
            SetUpLocations();
        }

        public void CreateGameLocations()
        {
            GameLocations = new List<LocationDefinition>();

            foreach (var locationDefinition in SelectedLocations)
            {
                LocationDefinition gameLocationDefinition = Instantiate(locationDefinitionPrefab, gameFieldParent);
                gameLocationDefinition.InitializeLocationDefinition(locationDefinition.LocationData, true);
                gameLocationDefinition.IsSelected = true;

                GameLocations.Add(gameLocationDefinition);
            }
            
            AssignLocationOwner();
            AssignBackgroundColorAndPlayerLine();

            foreach (LocationDefinition gameLocation in GameLocations)
            {
                gameLocation.SetPosition(gameLocation.PlayerLine.neutralPosition.position);
            }
        }

        public void ResetGameLocations()
        {
            if (GameLocations == null)
                return;
            
            foreach (Transform child in gameFieldParent.transform) {
                Destroy(child.gameObject);
            }
            
            GameLocations.Clear();
        }

        public void ResetSelectedLocations()
        {
            foreach (LocationDefinition locationDefinition in SelectedLocations)
            {
                locationDefinition.IsSelected = false;
            }
            
            SelectedLocations.Clear();
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

        public void PickRandomLocations()
        {
            SelectedLocations.Clear();

            List<LocationDefinition> shuffledLocations = allAvailableLocations.Shuffled();

            // Nimm die ersten 3 (oder weniger, falls die Liste kürzer ist)
            for (int i = 0; i < Math.Min(3, shuffledLocations.Count); i++)
            {
                SetupSelectLocation(shuffledLocations[i]);
            }
        }

        public void UpdateLocationAnimation()
        {
            if(locationAnimation == null)
                locationAnimation = new LocationAnimation();
            
            locationAnimation.UpdateLocationAnimation(GameLocations);
        }

        /// <summary>
        /// Assigns Background Color and PlayerLine.
        /// </summary>
        private void AssignBackgroundColorAndPlayerLine()
        {
            GameLocations[0].PlayerLine = lines[0];
            GameLocations[1].PlayerLine = lines[1];
            GameLocations[2].PlayerLine = lines[2];
        }
        
        /// <summary>
        /// Sets Player Owner at the start of the game and then shuffles the locations.
        /// </summary>
        private void AssignLocationOwner()
        {
            GameLocations[0].CurrentOwner = PlayerColor.Blue;
            GameLocations[0].AddPlayerPower(PlayerColor.Blue, 5);
            GameLocations[0].AddPlayerPower(PlayerColor.Red, 3);
            
            GameLocations[1].CurrentOwner = PlayerColor.Red;
            GameLocations[1].AddPlayerPower(PlayerColor.Red, 5);
            GameLocations[1].AddPlayerPower(PlayerColor.Blue, 3);
            
            GameLocations[2].CurrentOwner = PlayerColor.Neutral;
            GameLocations[2].AddPlayerPower(PlayerColor.Red, 4);
            GameLocations[2].AddPlayerPower(PlayerColor.Blue, 4);
        }

        private void SetUpLocations()
        {
            foreach (LocationData locationData in locationDataCollection)
            {
                LocationDefinition locationDefinition = Instantiate(locationDefinitionPrefab, locationsParent);
                locationDefinition.InitializeLocationDefinition(locationData, false);
                allAvailableLocations.Add(locationDefinition);
            }
        }

        private void CheckEnoughLocationsSelected(){
            if (SelectedLocations.Count == 3){
                MenuManager.instance.OpenMenu(MenuIdentifier.LocationSelectionMenu);
            }
        }
    }
}