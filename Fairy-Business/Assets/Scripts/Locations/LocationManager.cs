using System;
using System.Collections.Generic;
using Player;
using UI.Menu.BaseMenu;
using UnityEngine;
using UnityEngine.UI;

namespace Locations
{
    public class LocationManager : MonobehaviourSingletonCustom<LocationManager>
    {
        public static event Action<LocationDefinition> OnNewLocationCreatedEvent; 
        public List<LocationDefinition> SelectedLocations { get; private set; } = new ();
        [field: SerializeField] public List<LocationDefinition> GameLocations { get; private set; } = new();
        
        [Header("Selection Field")]
        [SerializeField] private List<LocationData> baseLocationDataCollection;
        [SerializeField] private List<LocationData> expertLocationDataCollection;
        [SerializeField] private LocationDefinition baseLocationPrefab;
        [SerializeField] private Transform baseLocationsParent;
        [SerializeField] private Transform expertLocationsParent;

        [Header("Game Field")]
        [SerializeField] private Transform gameFieldParent;
        [SerializeField] private PlayerLine[] lines = new PlayerLine[3];
        [SerializeField] private List<Sprite> artefactsSprites = new();
        
        [Header("Bank Infos")]
        [SerializeField] private List<BankWrapper> bankIdentifierByIndex = new();

        private readonly List<LocationDefinition> allAvailableLocations = new ();
        private LocationAnimation locationAnimation;

        private void Awake()
        {
            SetUpLocations();
        }

        public void CreateGameLocations()
        {
            for (int index = 0; index < SelectedLocations.Count; index++)
            {
                GameLocations[index].InitializeLocationDefinition(SelectedLocations[index].LocationData, true, 
                    bankIdentifierByIndex[index]);
                
                GameLocations[index].IsSelected = true;
                GameLocations[index].PlayerLine = lines[index];
            }
            
            AssignLocationOwner();

            foreach (LocationDefinition gameLocation in GameLocations)
            {
                gameLocation.SetPosition(gameLocation.PlayerLine.neutralPosition.position);
                OnNewLocationCreatedEvent?.Invoke(gameLocation);
            }
        }

        public void ResetSelectedLocations()
        {
            foreach (LocationDefinition locationDefinition in SelectedLocations)
            {
                locationDefinition.IsSelected = false;
            }
            
            SelectedLocations.Clear();
        }
        
        public void SetupSelectLocation(LocationDefinition locationDefinition)
        {
            if (locationDefinition.IsLocationBlocked)
                return;
            
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
            
            while (SelectedLocations.Count < 3)
            {
                int randomIndex = UnityEngine.Random.Range(0, allAvailableLocations.Count);
                
                SetupSelectLocation(allAvailableLocations[randomIndex]);
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
            GameLocations[0].AddPlayerPower(PlayerColorIdentifier.Blue, 5);
            GameLocations[0].AddPlayerPower(PlayerColorIdentifier.Red, 3);
            GameLocations[0].FinalizePowerAndDetermineWinner();
            GameLocations[0].Artifact.SetActive(true);
            GameLocations[0].Artifact.GetComponent<Image>().sprite = artefactsSprites[0];
            
            GameLocations[1].AddPlayerPower(PlayerColorIdentifier.Red, 5);
            GameLocations[1].AddPlayerPower(PlayerColorIdentifier.Blue, 3);
            GameLocations[1].FinalizePowerAndDetermineWinner();
            GameLocations[1].Artifact.SetActive(true);
            GameLocations[1].Artifact.GetComponent<Image>().sprite = artefactsSprites[1];

            GameLocations[2].AddPlayerPower(PlayerColorIdentifier.Blue, 4);
            GameLocations[2].AddPlayerPower(PlayerColorIdentifier.Red, 4);
            GameLocations[2].FinalizePowerAndDetermineWinner();
            GameLocations[2].Artifact.SetActive(true);
            GameLocations[2].Artifact.GetComponent<Image>().sprite = artefactsSprites[2];
        }

        private void SetUpLocations()
        {
            foreach (LocationData locationData in baseLocationDataCollection)
            {
                LocationDefinition locationDefinition = Instantiate(baseLocationPrefab, baseLocationsParent);
                locationDefinition.InitializeLocationDefinition(locationData, false);
                allAvailableLocations.Add(locationDefinition);
            }

            foreach (LocationData locationData in expertLocationDataCollection)
            {
                LocationDefinition locationDefinition = Instantiate(baseLocationPrefab, expertLocationsParent);
                locationDefinition.InitializeLocationDefinition(locationData, false);
                allAvailableLocations.Add(locationDefinition);
            }
        }

        private void CheckEnoughLocationsSelected()
        {
            if (SelectedLocations.Count == 3){
                MenuManager.instance.OpenMenu(MenuIdentifier.LocationSelectionMenu);
            }
        }
    }
}