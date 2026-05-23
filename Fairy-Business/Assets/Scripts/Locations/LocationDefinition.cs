using System;
using System.Collections.Generic;
using Animation;
using Player;
using TMPro;
using UI;
using UI.Buttons;
using UnityEngine;
using UnityEngine.UI;

namespace Locations
{
    [RequireComponent((typeof(RectTransform)), (typeof(MoveRotateObject)))]
    public class LocationDefinition : MonoBehaviour
    {
        public static event Action<PlayerColorIdentifier, LocationDefinition> OnNewPowerAddedEvent;
        public static event Action<LocationDefinition> OnCurrentOwnerChangedEvent;
        public static event Action<LocationsIdentifier, bool> OnLocationSelectedEvent;
        public LocationsIdentifier LocationIdentifier => LocationData.LocationIdentifier;
        public int VictoryPoints => LocationData.VictoryPoints;
        public bool AreVictoryPointsApplied { get; set; }
        public bool IsLocationBlocked { get; private set; }
        public LocationData LocationData { get; private set; }
        public PlayerLine PlayerLine { get; set; }
        public BankWrapper BankWrapper { get; private set; }
        public RectTransform RectTransform { get; private set; }
        public PlayerColorIdentifier CurrentOwner
        {
            get => currentOwner;
            set
            {
                if (currentOwner == value)
                    return;

                // save last owner
                LastOwner = currentOwner;

                currentOwner = value;
                
                //Commented out bc of artefacts now moving and not locations anymore
                locationImage.sprite = currentOwner == PlayerColorIdentifier.Neutral ? imageDisabled : imageEnabled;

                OnCurrentOwnerChangedEvent?.Invoke(this);
            }
        }
        public MoveRotateObject MoveObject { get; private set; }
        public PlayerColorIdentifier LastOwner { get; private set; }
        public List<LocationHoverButton> LocationHoverButtons => locationHoverButtons;
        [field: SerializeField] public Image Artifact { get; private set; }

        [SerializeField] private TextMeshProUGUI description;
        [SerializeField] private List<LocationHoverButton> locationHoverButtons = new();
        [SerializeField] private LocationHoverButton locationHoverButton;
        [SerializeField] private Image locationImage;
        [SerializeField] private Image blockingIcon;

        private Sprite imageEnabled;
        private Sprite imageDisabled;
        private LocationUI currenLocatioUI;
        private PlayerColorIdentifier currentOwner = PlayerColorIdentifier.None;
        private Dictionary<PlayerColorIdentifier, int> power = new();
        
        private bool isSelected;
        private LocationsIdentifier CouplingLocationIdentifier => LocationData.CouplingIdentifier;

        public bool IsSelected
        {
            get => isSelected;
            set
            {
                isSelected = value;
                UpdateVisuals();
                
                OnLocationSelectedEvent?.Invoke(CouplingLocationIdentifier, value);
            }
        }

        private void Awake()
        {
            RectTransform = GetComponent<RectTransform>();
            MoveObject = GetComponent<MoveRotateObject>();

            OnLocationSelectedEvent += BlockSelection;
        }

        private void OnDestroy()
        {
            OnLocationSelectedEvent -= BlockSelection;
        }

        public void InitializeLocationDefinition(LocationData data, bool isGameView, BankWrapper bankWrapper = null)
        {
            this.LocationData = data;
            this.imageEnabled = data.imageEnabled;
            this.imageDisabled = data.imageDisabled;
            
            if(bankWrapper != null) this.BankWrapper = bankWrapper;

            UpdateVisuals();

            if (isGameView)
            {
                foreach (LocationHoverButton locationHoverButton in locationHoverButtons)
                {
                    locationHoverButton.gameObject.SetActive(true);
                }
                
                SetArtefactSprite();
            }
            else
            {
                locationHoverButton.gameObject.SetActive(true);
            }
        }
        
        public void InitializeLocationUI(LocationUI locationUI)
        {
            currenLocatioUI = locationUI;
            
            string activeLanguageCode = Localizer.instance.GetCurrentlySetLanguage();
            string locationTitle = Localizer.instance.TranslateToSpecificLanguage(LocationData.localizationTitleText, 
                activeLanguageCode);
            
            string locationDescription = Localizer.instance.TranslateToSpecificLanguage(LocationData.localizationDescriptionText, 
                activeLanguageCode);
            
            currenLocatioUI.Init(imageEnabled, locationTitle, locationDescription);
        }
        
        public void AddPlayerPower(PlayerColorIdentifier playerIdx, int newPower)
        {
            if (power.ContainsKey(playerIdx))
                power[playerIdx] += newPower;
            else
                power[playerIdx] = newPower;

            power[playerIdx] = Math.Max(0, power[playerIdx]);
            
            OnNewPowerAddedEvent?.Invoke(playerIdx, this);
        }

        public void SetPlayerPower(PlayerColorIdentifier playerIdx, int newPower)
        {
            power[playerIdx] = Math.Max(0, newPower);
            OnNewPowerAddedEvent?.Invoke(playerIdx, this);
        }

        public int GetPlayerPower(PlayerColorIdentifier playerIdx){
            return power[playerIdx];
        }

        /// <summary>
        /// Call this once after all power values were assigned.
        /// </summary>
        public void FinalizePowerAndDetermineWinner()
        {
            int blue = power.ContainsKey(PlayerColorIdentifier.Blue) ? power[PlayerColorIdentifier.Blue] : 0;
            int red = power.ContainsKey(PlayerColorIdentifier.Red) ? power[PlayerColorIdentifier.Red] : 0;
            int neutral = power.ContainsKey(PlayerColorIdentifier.Neutral) ? power[PlayerColorIdentifier.Neutral] : 0;

            PlayerColorIdentifier winner;

            if (blue == red)
                winner = PlayerColorIdentifier.Neutral;
            else if (red > blue && red > neutral)
                winner = PlayerColorIdentifier.Red;
            else if (blue > red && blue > neutral)
                winner = PlayerColorIdentifier.Blue;
            else
                winner = PlayerColorIdentifier.Neutral;

            CurrentOwner = winner;
        }

        public void SetPosition(Vector3 position)
        {
            transform.position = position;
        }

        private void SetArtefactSprite()
        {
            Artifact.enabled = true;
            Artifact.sprite = LocationData.artefactIcon;
        }
        
        /// <summary>
        /// Toggles the blocked state of a location based on the given identifier and block flag.
        /// </summary>
        /// <param name="locationIdentifier">The location identifier to match against this instance.</param>
        /// <param name="allow">If true, blocks the location; if false, unblocks it.</param>
        private void BlockSelection(LocationsIdentifier locationIdentifier, bool allow)
        {
            if (LocationData == null)
            {
                Debug.LogError("Location Data is null, could be bc this Location Definition exists in the scene but " +
                               "was not initialized yet!", this);
                return;
            }

            if (locationIdentifier != LocationIdentifier)
                return;
            
            blockingIcon.enabled = allow;
            IsLocationBlocked = allow;
        }
        
        private void UpdateVisuals(){

            if (isSelected)
            {
                locationImage.sprite = imageEnabled;
                blockingIcon.enabled = false;
                IsLocationBlocked = false;
                return;
            }
            
            locationImage.sprite = imageDisabled;
        }
    }
}