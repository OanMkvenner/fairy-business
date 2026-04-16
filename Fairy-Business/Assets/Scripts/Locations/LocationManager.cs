using System;
using System.Collections.Generic;
using HelperClasses;
using Player;
using UI.Menu.BaseMenu;
using UnityEngine;
using UnityEngine.UI;

namespace Locations
{
    public class LocationManager : MonobehaviourSingletonCustom<LocationManager>
    {
        public static event Action<LocationDefinition> OnNewLocationCreatedEvent; 
        public List<LocationDefinition> SelectedLocations { get; private set; } = new List<LocationDefinition>();
        public List<LocationDefinition> GameLocations { get; private set; } 

        [SerializeField] private List<LocationData> locationDataCollection;
        [SerializeField] private LocationDefinition locationDefinitionPrefab;
        [SerializeField] private Transform locationsParent;

        [Header("GameField")]
        [SerializeField] private Transform gameFieldParent;
        [SerializeField] private PlayerLine[] lines = new PlayerLine[3];
        [SerializeField] private List<Sprite> artefactsSprites = new();

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

            GameLocations[0].PlayerLine = lines[0];
            GameLocations[1].PlayerLine = lines[1];
            GameLocations[2].PlayerLine = lines[2];
            
            AssignLocationOwner();

            foreach (LocationDefinition gameLocation in GameLocations)
            {
                gameLocation.SetPosition(gameLocation.PlayerLine.neutralPosition.position);
                OnNewLocationCreatedEvent?.Invoke(gameLocation);
            }
        }

        public void ResetGameLocations()
        {
            if (GameLocations == null)
                return;
            
            foreach (LocationDefinition child in GameLocations) {
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

        /// <summary>
        /// Start the movement animation of the locations.
        /// </summary>
        public void UpdateLocationAnimation()
        {
            if(locationAnimation == null)
                locationAnimation = new LocationAnimation();
            
            locationAnimation.UpdateLocationAnimation(GameLocations);
        }
        
        /// <summary>
        /// Sets Player Owner at the start of the game.
        /// </summary>
        private void AssignLocationOwner()
        {
            GameLocations[0].AddPlayerPower(PlayerColor.Blue, 5);
            GameLocations[0].AddPlayerPower(PlayerColor.Red, 3);
            GameLocations[0].FinalizePowerAndDetermineWinner();
            GameLocations[0].Artifact.SetActive(true);
            GameLocations[0].Artifact.GetComponent<Image>().sprite = artefactsSprites[0];
            
            GameLocations[1].AddPlayerPower(PlayerColor.Red, 5);
            GameLocations[1].AddPlayerPower(PlayerColor.Blue, 3);
            GameLocations[1].FinalizePowerAndDetermineWinner();
            GameLocations[1].Artifact.SetActive(true);
            GameLocations[1].Artifact.GetComponent<Image>().sprite = artefactsSprites[1];

            GameLocations[2].AddPlayerPower(PlayerColor.Blue, 4);
            GameLocations[2].AddPlayerPower(PlayerColor.Red, 4);
            GameLocations[2].FinalizePowerAndDetermineWinner();
            GameLocations[2].Artifact.SetActive(true);
            GameLocations[2].Artifact.GetComponent<Image>().sprite = artefactsSprites[2];
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