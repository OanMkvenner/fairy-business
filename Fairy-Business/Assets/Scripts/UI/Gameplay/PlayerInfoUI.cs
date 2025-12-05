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
        [SerializeField] private TextMeshProUGUI keyword;

        private void Awake()
        {
            LocationDefinition.OnNewPowerAddedEvent += SetTurnPoints;
            LocationManager.OnNewLocationCreatedEvent += AssignLocationKeyword;
        }

        private void OnDestroy()
        {
            LocationDefinition.OnNewPowerAddedEvent -= SetTurnPoints;
            LocationManager.OnNewLocationCreatedEvent -= AssignLocationKeyword;
        }

        private void AssignLocationKeyword(LocationDefinition locationDefinition)
        {
            if (locationDefinition.PlayerLine.line != line)
                return;
            
            keyword.text = locationDefinition.LocationData.keyword;
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