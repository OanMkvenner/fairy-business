using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace UI
{
    public class LocationUI : MonoBehaviour
    {
        [SerializeField] private Image background;
        [SerializeField] private Image locationImage;
        [SerializeField] private TextMeshProUGUI locationTitle;
        [SerializeField] private TextMeshProUGUI locationDescription;

        public void Init(Color backgroundColor, Sprite locationImage, string locationTitle, string locationDescription)
        {
            this.background.color = backgroundColor;
            this.locationImage.sprite = locationImage;
            this.locationTitle.text = locationTitle;
            this.locationDescription.text = locationDescription;
        }

    }
}