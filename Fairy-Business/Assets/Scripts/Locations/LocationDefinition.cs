using System;
using System.Collections.Generic;
using DG.Tweening;
using Player;
using TMPro;
using UI;
using UI.Buttons;
using UIExtensions;
using UnityEngine;

namespace Locations
{
    [RequireComponent((typeof(RectTransform)))]
    public class LocationDefinition : MonoBehaviour, ITweenAnimation {
        public LocationsType LocationType => locationType;
        public int VictoryPoints => victoryPoints;
        public LocationData LocationData { get; private set; }
        public PlayerLine PlayerLine { get; set; }

        public PlayerColor CurrentOwner;
        public List<LocationHoverButton> LocationHoverButtons => locationHoverButtons;

        [SerializeField] private TextMeshProUGUI description;
        [SerializeField] private List<LocationHoverButton> locationHoverButtons = new();
        [SerializeField] private LocationHoverButton locationHoverButton;
        [SerializeField] private SkewedImage locationImage;
        [SerializeField] private SkewedImage cardFrameImage;

        private Sprite imageEnabled;
        private Sprite imageDisabled;
        private string locationText;
        private LocationsType locationType;
        private int victoryPoints = 3;
        private LocationUI currenLocatioUI;

        private RectTransform rectTransform;

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
            rectTransform = GetComponent<RectTransform>();
        }

        public void InitializeLocationDefinition(LocationData data, bool isGameView)
        {
            this.LocationData = data;
            this.imageEnabled = data.imageEnabled;
            this.imageDisabled = data.imageDisabled;
            this.locationText = data.locationDescription;
            this.locationType = data.locationType;
            this.victoryPoints = data.VictoryPoints;
            //description.text = locationType.ToString();
            
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

        public void SetCardFrame(Sprite frame)
        {
            cardFrameImage.gameObject.SetActive(true);
            cardFrameImage.sprite = frame;
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
            
            CheckWinner();
        }
        
        public void SetPlayerPower(PlayerColor playerIdx, int newPower)
        {
            power[playerIdx] = newPower; // setzt den Wert direkt, egal ob er schon existiert
            CheckWinner();
        }
        
        public int GetPlayerPower(PlayerColor playerIdx){
            return power[playerIdx];
        }

        #region IAnimation

        public Tween MoveY(float y, float duration)
        {
            return rectTransform.DOMoveY(y, duration);
        }

        public Tween MoveX(float x, float duration)
        {
            return rectTransform.DOLocalMoveX(x, duration);
        }

        public Tween Rotate(float angle, float duration)
        {
            return rectTransform.DORotate(new Vector3(0, 0, angle), duration);
        }

        public void SkrewImageBottom()
        {
            if (CurrentOwner != PlayerColor.Neutral)
            {
                locationImage.TrapezSkewTop = 0.9f;
                cardFrameImage.TrapezSkewTop = 0.9f;
                
                locationImage.TrapezSkewBottom = 1.1f;
                cardFrameImage.TrapezSkewBottom = 1.1f;
            }
            else
            {
                locationImage.TrapezSkewTop = 1f;
                cardFrameImage.TrapezSkewTop = 1f;
            }
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