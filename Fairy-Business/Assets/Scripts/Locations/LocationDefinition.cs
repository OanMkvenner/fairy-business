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
        public LocationsType LocationType => locationType;
        public int VictoryPoints => victoryPoints;
        public LocationData LocationData { get; private set; }
        public PlayerLine PlayerLine { get; set; }

        public PlayerColor CurrentOwner
        {
            get => currentOwner;
            set
            {
                if (currentOwner == value)
                    return;

                currentOwner = value;
                
                locationImage.sprite = currentOwner == PlayerColor.Neutral ? imageDisabled : imageEnabled;

                OnCurrentOwnerChangedEvent?.Invoke(this);
            }
        }

        public List<LocationHoverButton> LocationHoverButtons => locationHoverButtons;

        [SerializeField] private TextMeshProUGUI description;
        [SerializeField] private List<LocationHoverButton> locationHoverButtons = new();
        [SerializeField] private LocationHoverButton locationHoverButton;
        [SerializeField] private Image locationImage;

        private Sprite imageEnabled;
        private Sprite imageDisabled;
        private string locationText;
        private LocationsType locationType;
        private int victoryPoints = 3;
        private LocationUI currenLocatioUI;
        private PlayerColor currentOwner = PlayerColor.None;

        public RectTransform RectTransform { get; private set; }

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
            this.locationText = data.locationDescription;
            this.locationType = data.locationType;
            
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
            currenLocatioUI.Init(Color.gray, imageEnabled, locationType.ToString(), locationText);
        }
        
        public void AddPlayerPower(PlayerColor playerIdx, int newPower)
        {
            if (power.ContainsKey(playerIdx))
            {
                power[playerIdx] += newPower; // zum bestehenden Wert addieren
            }
            else
            {
                power[playerIdx] = newPower; // neuen Eintrag anlegen
            }
            
            power[playerIdx] = Math.Max(0, power[playerIdx]);
            
            OnNewPowerAddedEvent?.Invoke(playerIdx, this);
            
            CheckWinner();
        }
        
        /// <summary>
        /// Use this method only, if you want to overwrite existing power.
        /// </summary>
        /// <param name="playerIdx"></param>
        /// <param name="newPower"></param>
        public void SetPlayerPower(PlayerColor playerIdx, int newPower)
        {
            power[playerIdx] = newPower; // setzt den Wert direkt, egal ob er schon existiert
            OnNewPowerAddedEvent?.Invoke(playerIdx, this);
            CheckWinner();
        }
        
        public int GetPlayerPower(PlayerColor playerIdx){
            return power[playerIdx];
        }

        #region IAnimation

        public Tween MoveY(float y, float duration)
        {
            return RectTransform.DOMoveY(y, duration);
        }

        public Tween MoveX(float x, float duration)
        {
            return RectTransform.DOLocalMoveX(x, duration);
        }

        public Tween Rotate(float angle, float duration)
        {
            return RectTransform.DORotate(new Vector3(0, 0, angle), duration);
        }

        #endregion

        public void SetPosition(Vector3 position)
        {
            transform.position = position;
        }
        
        private void CheckWinner()
        {
            int blue = power.ContainsKey(PlayerColor.Blue) ? power[PlayerColor.Blue] : 0;
            int red = power.ContainsKey(PlayerColor.Red) ? power[PlayerColor.Red] : 0;
            int neutral = power.ContainsKey(PlayerColor.Neutral) ? power[PlayerColor.Neutral] : 0;

            // Falls Neutral nicht vorkommt, wird sein Wert einfach 0 sein.

            if (blue == red)
            {
                CurrentOwner = PlayerColor.Neutral;
            }
            else if (red > blue && red > neutral)
            {
                CurrentOwner = PlayerColor.Red;
            }
            else if (blue > red && blue > neutral)
            {
                CurrentOwner = PlayerColor.Blue;
            }

            Debug.Log($"{LocationData.locationType}+{CurrentOwner}");
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