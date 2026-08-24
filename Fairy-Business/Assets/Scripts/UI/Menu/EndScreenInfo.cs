
using TMPro;
using UnityEngine;
using UnityEngine.UI;


namespace UI.Menu
{
    public class EndScreenInfo : MonoBehaviour
    {
        [SerializeField] private Sprite WinIcon;
        [SerializeField] private Sprite defaultIcon;
        [SerializeField] private Image victoryPointsBackground;
        [Space]
        [SerializeField] private TextMeshProUGUI victoryPoints;

        public void Initialize(int victoryPoints, bool hasWon)
        {
            victoryPointsBackground.sprite = hasWon ? WinIcon : defaultIcon;
            this.victoryPoints.text = victoryPoints.ToString();
        }
    }
}