using Locations;
using Player;
using TMPro;
using UnityEditor;
using UnityEngine;

namespace UI.Gameplay
{
    [RequireComponent(typeof(TextMeshProUGUI))]
    public class PlayerPowerUI : MonoBehaviour
    {
        [SerializeField] private LineIdentifier line;
        [SerializeField] private PlayerColor playerColor;
        private TextMeshProUGUI text;

        private void Awake()
        {
            text = GetComponent<TextMeshProUGUI>();

            LocationDefinition.OnNewPowerAddedEvent += SetTurnPoints;
        }

        private void OnDestroy()
        {
            LocationDefinition.OnNewPowerAddedEvent -= SetTurnPoints;
        }

        private void SetTurnPoints(PlayerColor currentPlayerColor, LocationDefinition locationDefinition)
        {
            
            if (playerColor != currentPlayerColor)
                return;
            
            if (locationDefinition.PlayerLine.line != this.line)
                return;
            
            int power = locationDefinition.GetPlayerPower(currentPlayerColor);
            text.text = power.ToString();
            
            Debug.Log($"{currentPlayerColor} {locationDefinition.PlayerLine.line} {power}");
        }
    }
}