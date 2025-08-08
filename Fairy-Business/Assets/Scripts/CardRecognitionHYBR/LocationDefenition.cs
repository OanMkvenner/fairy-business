using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;

public class LocationDefenition : MonoBehaviour {
   
    public Sprite imageEnabled;
    public Sprite imageDisabled;
    public string locationText;
    public LocationsType locationType;
    public int VPGainedOnScorePhase = 3;
    public void CopyFrom(LocationDefenition locDef){
        imageEnabled = locDef.imageEnabled;
        imageDisabled = locDef.imageDisabled;
        locationText = locDef.locationText;
        locationType = locDef.locationType;
    }
    private void Start() {
        UpdateFlipButton();
    }
    public void UpdateFlipButton(){
        GetComponent<FlipButton>().FrontImage.sprite = imageEnabled;
        GetComponent<FlipButton>().BackImage.sprite = imageDisabled;
        GetComponent<FlipButton>().FrontText.text = locationText;
        GetComponent<FlipButton>().BackText.text = locationText;
    }
}