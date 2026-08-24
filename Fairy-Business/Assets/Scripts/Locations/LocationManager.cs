using System;
using System.Collections.Generic;
using System.Linq;
using Player;
using UI.Menu.BaseMenu;
using UnityEngine;

namespace Locations
{
    public class LocationManager : MonobehaviourSingletonCustom<LocationManager>
    {
        public static event Action<LocationDefinition> OnNewLocationCreatedEvent; 
        public List<LocationDefinition> SelectedLocations { get; private set; } = new ();
        
        public ModeIdentifier CurrentMode { get; set; } = ModeIdentifier.Base;
        
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
        
        [Header("Bank Infos")]
        [SerializeField] private List<BankWrapper> bankIdentifierByIndex = new();

        private readonly List<LocationDefinition> allAvailableLocations = new ();
        private LocationAnimation locationAnimation;
        private Sprite ArtefactSprite;

        private void Awake()
        {
            SetUpLocations();
        }

        public void CreateGameLocations()
        {
            GameSession.instance.GameHasStarted = true;
            
            for (int index = 0; index < SelectedLocations.Count; index++)
            {
                GameLocations[index].InitializeLocationDefinition(SelectedLocations[index].LocationData, GameSession.instance.GameHasStarted, 
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

        public bool CheckEnoughLocationsSelected()
        {
            return SelectedLocations.Count == 3;
        }
        
        public void ResetSelectedLocations()
        {
            foreach (LocationDefinition locationDefinition in allAvailableLocations)
            {
                locationDefinition.IsSelected = false;
            }
            
            SelectedLocations.Clear();
        }
        
        public void SetupSelectLocation(LocationDefinition locationDefinition)
        {
            if (SelectedLocations.Contains(locationDefinition))
            {
                SelectedLocations.Remove(locationDefinition);
                locationDefinition.IsSelected = false;
                
            } else {
                
                SelectedLocations.Add(locationDefinition);
                locationDefinition.IsSelected = true;
            }
            
            if(CheckEnoughLocationsSelected())
                MenuManager.instance.OpenMenu(MenuIdentifier.LocationSelectionMenu);;
        }

        public void PickRandomLocations()
        {
            if (CurrentMode == ModeIdentifier.None)
            {
                Debug.LogError("No Location mode set!");
                return;
            }
            
            SelectedLocations.Clear();

            List<LocationDefinition> locationsBasedOnMode =
                allAvailableLocations
                    .Where(ld => ld.LocationData.ModeIdentifier == CurrentMode)
                    .ToList();
            
            while (SelectedLocations.Count < 3)
            {
                int randomIndex = UnityEngine.Random.Range(0, locationsBasedOnMode.Count);
                
                SetupSelectLocation(locationsBasedOnMode[randomIndex]);
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
            
            GameLocations[1].AddPlayerPower(PlayerColorIdentifier.Red, 5);
            GameLocations[1].AddPlayerPower(PlayerColorIdentifier.Blue, 3);
            GameLocations[1].FinalizePowerAndDetermineWinner();

            GameLocations[2].AddPlayerPower(PlayerColorIdentifier.Blue, 4);
            GameLocations[2].AddPlayerPower(PlayerColorIdentifier.Red, 4);
            GameLocations[2].FinalizePowerAndDetermineWinner();
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
            
            //deactivate expert mode, bc its at first glance not visiable for the player
            expertLocationsParent.gameObject.SetActive(false);
        }
    }
}