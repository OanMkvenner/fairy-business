using TMPro;
using UI;
using UnityEngine;
using UnityEngine.UI;

namespace Locations
{
    [RequireComponent(typeof(Button))]
    public class LocationDefinition : MonoBehaviour {
        public LocationsType LocationType => locationType;

        public int VictoryPoints => victoryPoints;

        [SerializeField] private Image image;
        [SerializeField] private TextMeshProUGUI description;
        
        private Button selectionButton;
        private Sprite imageEnabled;
        private Sprite imageDisabled;
        private string locationText;
        private LocationsType locationType;
        private int victoryPoints = 3;
        private LocationUI currenLocatioUI;

        private bool isSelected;

        public bool IsSelected
        {
            get => isSelected;
            set
            {
                if (isSelected != value)
                {
                    isSelected = value;
                    UpdateVisuals();
                }
            }
        }

        private void Awake()
        {
            selectionButton = GetComponent<Button>();
            selectionButton.onClick.AddListener(OnButtonClicked);
        }

        public void InitializeLocationDefinition(LocationData data)
        {
            this.imageEnabled = data.imageEnabled;
            this.imageDisabled = data.imageDisabled;
            this.locationText = data.locationDescription;
            this.locationType = data.locationType;
            this.victoryPoints = data.VictoryPoints;
            description.text = locationType.ToString();
        }
        
        public void InitializeLocationUI(LocationUI locationUI)
        {
            currenLocatioUI = locationUI;
            currenLocatioUI.Init(Color.gray, imageEnabled, locationType.ToString(), locationText);
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