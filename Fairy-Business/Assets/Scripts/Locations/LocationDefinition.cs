using System;
using System.Collections.Generic;
using DG.Tweening;
using Player;
using TMPro;
using UI;
using UI.Buttons;
using UnityEngine;
using UnityEngine.UI;

namespace Locations
{
    [RequireComponent((typeof(RectTransform)))]
    public class LocationDefinition : MonoBehaviour, ITweenAnimation
    {
        public static event Action<PlayerColor, LocationDefinition> OnNewPowerAddedEvent;
        public static event Action<LocationDefinition> OnCurrentOwnerChangedEvent;
        public LocationsIdentifier LocationIdentifier => LocationData.LocationIdentifier;
        public int VictoryPoints => LocationData.VictoryPoints;
        public LocationData LocationData { get; private set; }
        public PlayerLine PlayerLine { get; set; }
        public RectTransform RectTransform { get; private set; }

        public PlayerColor CurrentOwner
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
                //locationImage.sprite = currentOwner == PlayerColor.Neutral ? imageDisabled : imageEnabled;
                
                if(currentOwner != PlayerColor.Neutral)
                {
                    locationImage.sprite = imageEnabled;
                }
                else
                {
                    locationImage.sprite = imageDisabled;
                }


                OnCurrentOwnerChangedEvent?.Invoke(this);
            }
        }
        
        public PlayerColor LastOwner { get; private set; }

        public List<LocationHoverButton> LocationHoverButtons => locationHoverButtons;

        [field: SerializeField] public GameObject Artfecat { get; set; }

        [SerializeField] private TextMeshProUGUI description;
        [SerializeField] private List<LocationHoverButton> locationHoverButtons = new();
        [SerializeField] private LocationHoverButton locationHoverButton;
        [SerializeField] private Image locationImage;

        private Sprite imageEnabled;
        private Sprite imageDisabled;
        private LocationUI currenLocatioUI;
        private PlayerColor currentOwner = PlayerColor.None;
        private Dictionary<PlayerColor, int> power = new();
        private bool isSelected;

        public bool IsSelected
        {
            get => isSelected;
            set
            {
                isSelected = value;
                UpdateVisuals();
            }
        }

        private void Awake()
        {
            RectTransform = GetComponent<RectTransform>();
        }

        public void InitializeLocationDefinition(LocationData data, bool isGameView)
        {
            this.LocationData = data;
            this.imageEnabled = data.imageEnabled;
            this.imageDisabled = data.imageDisabled;

            UpdateVisuals();

            if (isGameView)
            {
                foreach (LocationHoverButton locationHoverButton in locationHoverButtons)
                {
                    locationHoverButton.gameObject.SetActive(true);
                }
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
            
            currenLocatioUI.Init(Color.gray, imageEnabled, locationTitle, locationDescription);
        }
        
        public void AddPlayerPower(PlayerColor playerIdx, int newPower)
        {
            if (power.ContainsKey(playerIdx))
                power[playerIdx] += newPower;
            else
                power[playerIdx] = newPower;

            power[playerIdx] = Math.Max(0, power[playerIdx]);
            
            OnNewPowerAddedEvent?.Invoke(playerIdx, this);
        }

        public void SetPlayerPower(PlayerColor playerIdx, int newPower)
        {
            power[playerIdx] = Math.Max(0, newPower);
        }

        /// <summary>
        /// Call this once after all power values were assigned.
        /// </summary>
        public void FinalizePowerAndDetermineWinner()
        {
            int blue = power.ContainsKey(PlayerColor.Blue) ? power[PlayerColor.Blue] : 0;
            int red = power.ContainsKey(PlayerColor.Red) ? power[PlayerColor.Red] : 0;
            int neutral = power.ContainsKey(PlayerColor.Neutral) ? power[PlayerColor.Neutral] : 0;

            PlayerColor winner;

            if (blue == red)
                winner = PlayerColor.Neutral;
            else if (red > blue && red > neutral)
                winner = PlayerColor.Red;
            else if (blue > red && blue > neutral)
                winner = PlayerColor.Blue;
            else
                winner = PlayerColor.Neutral;

            CurrentOwner = winner;
        }

        
        public int GetPlayerPower(PlayerColor playerIdx){
            return power[playerIdx];
        }

        #region IAnimation

        public Tween MoveY(float y, float duration)
        {
            return Artfecat.GetComponent<RectTransform>().DOMoveY(y, duration);
        }

        public Tween MoveX(float x, float duration)
        {
            return Artfecat.GetComponent<RectTransform>().DOLocalMoveX(x, duration);
        }

        public Tween Rotate(float angle, float duration)
        {
            return Artfecat.GetComponent<RectTransform>().DORotate(new Vector3(0, 0, angle), duration);
        }

        #endregion

        public void SetPosition(Vector3 position)
        {
            transform.position = position;
        }
        
        private void UpdateVisuals(){

            if (isSelected)
            {
                locationImage.sprite = imageEnabled;
                return;
            }
            
            locationImage.sprite = imageDisabled;
        }
    }
}