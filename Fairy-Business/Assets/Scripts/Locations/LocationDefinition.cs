using UI;
using UnityEngine;

namespace Locations
{
    public class LocationDefinition : MonoBehaviour {
        public LocationsType LocationType => locationType;

        public int VictoryPoints => victoryPoints;
        

        private Sprite imageEnabled;
        private Sprite imageDisabled;
        private string locationText;
        private LocationsType locationType;
        private int victoryPoints = 3;
        private LocationUI currenLocatioUI;

        public void InitializeLocationDefinition(LocationData data)
        {
            this.imageEnabled = data.imageEnabled;
            this.imageDisabled = data.imageDisabled;
            this.locationText = data.locationDescription;
            this.locationType = data.locationType;
            this.victoryPoints = data.VictoryPoints;
            
            UpdateVisuals();
        }

        public void CopyFrom(LocationDefinition locDef){
            imageEnabled = locDef.imageEnabled;
            imageDisabled = locDef.imageDisabled;
            locationText = locDef.locationText;
            locationType = locDef.locationType;
        }

        public void UpdateVisuals(){
            GetComponent<FlipButton>().FrontImage.sprite = imageEnabled;
            GetComponent<FlipButton>().BackImage.sprite = imageDisabled;
            GetComponent<FlipButton>().FrontText.text = locationText;
            GetComponent<FlipButton>().BackText.text = locationText;
        }

        public void InitializeLocationUI(LocationUI locationUI)
        {
            currenLocatioUI = locationUI;
            currenLocatioUI.Init(Color.gray, imageEnabled, locationType.ToString(), locationText);
        }
    
    }
}