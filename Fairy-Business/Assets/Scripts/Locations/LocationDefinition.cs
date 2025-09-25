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
    public class LocationDefinition : MonoBehaviour, ITweenAnimation {
        public LocationsType LocationType => locationType;
        public int VictoryPoints => victoryPoints;
        public LocationData LocationData { get; private set; }
        public PlayerLine PlayerLine { get; set; }

        public PlayerColor CurrentOwner;

        [SerializeField] private Image image;
        [SerializeField] private TextMeshProUGUI description;
        [SerializeField] private Image backgroundColor;
        [SerializeField] public List<LocationHoverButton> locationHoverButtons = new();
        [SerializeField] private LocationHoverButton locationHoverButton;

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
            description.text = locationType.ToString();

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

        public void SetBackgroundColor(Color color)
        {
            backgroundColor.color = color;
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
                image.sprite = imageEnabled;
                return;
            }
            
            image.sprite = imageDisabled;
        }
    }
}