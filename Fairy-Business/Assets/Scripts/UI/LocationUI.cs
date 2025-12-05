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
        [SerializeField] private TextMeshProUGUI description;

        public void Init(Color backgroundColor, Sprite locationImage, string locationTitle, string description)
        {
            this.background.color = backgroundColor;
            this.locationImage.sprite = locationImage;
            this.locationTitle.text = locationTitle;
            this.description.text = description;
        }
    }
}