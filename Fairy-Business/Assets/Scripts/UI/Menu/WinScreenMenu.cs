using System.Collections.Generic;
using Player;
using TMPro;
using UI.Menu.BaseMenu;
using UnityEngine;
using UnityEngine.UI;

namespace UI.Menu
{
    public class WinScreenMenu : MenuElement
    {
        [SerializeField] private Button returnToStartButton;
        
        [Space]
        [SerializeField] private TextMeshProUGUI victoryPointsPlayerBlue;
        [SerializeField] private TextMeshProUGUI gameStateTextPlayerBlue;

        [SerializeField] private TextMeshProUGUI victoryPointsPlayerRed;
        [SerializeField] private TextMeshProUGUI gameStateTextPlayerRed;

        private void Awake()
        {
            returnToStartButton.onClick.AddListener(ReturnToStartButton);
        }

        public override void OpenMenu()
        {
            base.OpenMenu();
            InitializeUI();
        }

        private void InitializeUI()
        {
            Dictionary<PlayerColor, int> victoryPoints = GameSession.instance.VictoryPointCounters;

            if (victoryPoints[PlayerColor.Red] > victoryPoints[PlayerColor.Blue]){
            
                gameStateTextPlayerRed.text = "Won!";
                gameStateTextPlayerBlue.text = "Lost!";
            
            } else if (victoryPoints[PlayerColor.Red] < victoryPoints[PlayerColor.Blue]){
            
                gameStateTextPlayerRed.text = "Lost!";
                gameStateTextPlayerBlue.text = "Won!";
            }

            victoryPointsPlayerBlue.text = $"Victory Points: {victoryPoints[PlayerColor.Blue]}";
            victoryPointsPlayerRed.text = $"Victory Points: {victoryPoints[PlayerColor.Red]}";
        }

        private void ReturnToStartButton()
        {
            CloseMenu();
            UiManager.CallbackUiEvent("MainMenu");
        }
    }
}