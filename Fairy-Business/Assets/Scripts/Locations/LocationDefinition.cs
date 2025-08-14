using UI;
using UnityEngine;

namespace Locations
{
    public class LocationDefinition : MonoBehaviour {
   
        public Sprite imageEnabled;
        public Sprite imageDisabled;
        public string locationText;
        public LocationsType locationType;
        public int VPGainedOnScorePhase = 3;
        private LocationUI currenLocatioUI;

        private void Start() {
            UpdateFlipButton();
        }

        public void CopyFrom(LocationDefinition locDef){
            imageEnabled = locDef.imageEnabled;
            imageDisabled = locDef.imageDisabled;
            locationText = locDef.locationText;
            locationType = locDef.locationType;
        }

        public void UpdateFlipButton(){
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