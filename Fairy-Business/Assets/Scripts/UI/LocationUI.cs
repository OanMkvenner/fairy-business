using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace UI
{
    public class LocationUI : MonoBehaviour
    {
        [SerializeField] private Image locationBackground;
        [SerializeField] private Image artifactImage;
        [SerializeField] private TextMeshProUGUI locationTitle;
        [SerializeField] private TextMeshProUGUI artifactTitle;
        [SerializeField] private TextMeshProUGUI keyword;

        public void Init(Sprite locationBackgroundImage, Sprite artifact, string artifactName, string locationTitle, string keyword)
        {
            this.locationBackground.sprite = locationBackgroundImage;
            this.artifactImage.sprite = artifact;
            this.locationTitle.text = locationTitle;
            this.artifactTitle.text = artifactName;
            this.keyword.text = keyword;
        }
    }
}