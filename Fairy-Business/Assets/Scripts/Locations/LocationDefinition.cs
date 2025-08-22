using System.Collections.Generic;
using DG.Tweening;
using Player;
using TMPro;
using UI;
using UnityEngine;
using UnityEngine.UI;

namespace Locations
{
    [RequireComponent(typeof(Button),(typeof(RectTransform)))]
    public class LocationDefinition : MonoBehaviour, ITweenAnimation {
        public LocationsType LocationType => locationType;

        public int VictoryPoints => victoryPoints;
        
        public LocationData LocationData { get; private set; }
        
        public PlayerLine PlayerLine { get; set; }
        
        public Dictionary<PlayerColor, int> power = new();
        public PlayerColor currentOwner;

        [SerializeField] private Image image;
        [SerializeField] private TextMeshProUGUI description;
        [SerializeField] private Image backgroundColor;
        
        private Button selectionButton;
        private Sprite imageEnabled;
        private Sprite imageDisabled;
        private string locationText;
        private LocationsType locationType;
        private int victoryPoints = 3;
        private LocationUI currenLocatioUI;
        
        private RectTransform rectTransform;

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
            selectionButton = GetComponent<Button>();
            rectTransform = GetComponent<RectTransform>();
            selectionButton.onClick.AddListener(OnButtonClicked);
        }

        public void InitializeLocationDefinition(LocationData data)
        {
            this.LocationData = data;
            this.imageEnabled = data.imageEnabled;
            this.imageDisabled = data.imageDisabled;
            this.locationText = data.locationDescription;
            this.locationType = data.locationType;
            this.victoryPoints = data.VictoryPoints;
            description.text = locationType.ToString();
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
        
        public void SetPlayerPower(PlayerColor playerIdx, int newPower){
            power[playerIdx] = newPower;
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

        private void OnButtonClicked()
        {
            LocationManager.instance.SetupSelectLocation(this);
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