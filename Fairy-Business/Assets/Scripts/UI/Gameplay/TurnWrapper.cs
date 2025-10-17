using UnityEngine;
using UnityEngine.UI;

namespace UI.Gameplay
{
    [RequireComponent(typeof(Image))]
    public class TurnWrapper : MonoBehaviour
    {
        [SerializeField] private Color futureTurnColor;
        [SerializeField] private Color currentTurnColor;
        
        private Image turnImage;

        private void Awake()
        {
            turnImage = GetComponent<Image>();
        }

        public void FutureTurnOn()
        {
            turnImage.color = futureTurnColor;
        }

        public void CurrentTurnOn()
        {
            turnImage.color = currentTurnColor;
        }
    }
}