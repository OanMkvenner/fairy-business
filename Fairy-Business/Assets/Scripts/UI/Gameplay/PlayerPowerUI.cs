using Locations;
using Player;
using TMPro;
using UnityEngine;

namespace UI.Gameplay
{
    public class PlayerPowerUI : MonoBehaviour
    {
        [SerializeField] private int line;
        [SerializeField] private TextMeshProUGUI redPower;
        [SerializeField] private TextMeshProUGUI bluePower;

        private void Awake()
        {
            GameSession.OnSpyCardPlayed += SetPower;
        }

        private void OnDestroy()
        {
            GameSession.OnSpyCardPlayed -= SetPower;
        }

        private void SetPower(LocationDefinition locationDefinition, int currentLine)
        {
            if (line != currentLine)
                return;
            
            redPower.text = locationDefinition.GetPlayerPower(PlayerColor.Red).ToString();
            bluePower.text = locationDefinition.GetPlayerPower(PlayerColor.Blue).ToString();
        }

    }
}