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
            Sounds.instance.Play("WinScreen");
            InitializeUI();
        }

        private void InitializeUI()
        {
            Dictionary<PlayerColor, int> victoryPoints = GameSession.instance.VictoryPointCounters;

            string activeLanguageCode = Localizer.instance.GetCurrentlySetLanguage();
            string win = Localizer.instance.TranslateToSpecificLanguage("winGame", 
                activeLanguageCode);
            string lost = Localizer.instance.TranslateToSpecificLanguage("looseGame", 
                activeLanguageCode);
            
            if (victoryPoints[PlayerColor.Red] > victoryPoints[PlayerColor.Blue]){
            
                gameStateTextPlayerRed.text = win;
                gameStateTextPlayerBlue.text = lost;
            
            } else if (victoryPoints[PlayerColor.Red] < victoryPoints[PlayerColor.Blue]){
            
                gameStateTextPlayerRed.text = lost;
                gameStateTextPlayerBlue.text = win;
            }
            
            string locationVictoryPoint = Localizer.instance.TranslateToSpecificLanguage("victoryPoints", 
                activeLanguageCode);

            victoryPointsPlayerBlue.text = $"{locationVictoryPoint}: {victoryPoints[PlayerColor.Blue]}";
            victoryPointsPlayerRed.text = $"{locationVictoryPoint}: {victoryPoints[PlayerColor.Red]}";
        }

        private void ReturnToStartButton()
        {
            CloseMenu();
            UiManager.CallbackUiEvent("MainMenu");
        }
    }
}