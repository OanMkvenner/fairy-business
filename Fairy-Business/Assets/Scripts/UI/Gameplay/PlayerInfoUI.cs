using Locations;
using Player;
using TMPro;
using UnityEngine;

namespace UI.Gameplay
{
    public class PlayerInfoUI : MonoBehaviour
    {
        [SerializeField] private LineIdentifier line;
        [SerializeField] private PlayerColor playerColor;
        [SerializeField] private TextMeshProUGUI power;
        [SerializeField] private TextMeshProUGUI description;

        private void Awake()
        {
            LocationDefinition.OnNewPowerAddedEvent += SetTurnPoints;
        }

        private void OnDestroy()
        {
            LocationDefinition.OnNewPowerAddedEvent -= SetTurnPoints;
        }

        private void AssignLocationKeyword(LocationDefinition locationDefinition)
        {
            description.text = locationDefinition.LocationData.locationDescription;
        }
        
        private void SetTurnPoints(PlayerColor currentPlayerColor, LocationDefinition locationDefinition)
        {
            if (playerColor != currentPlayerColor)
                return;
            
            if (locationDefinition.PlayerLine.line != this.line)
                return;
            
            int power = locationDefinition.GetPlayerPower(currentPlayerColor);
            this.power.text = power.ToString();
        }
    }
}