using System.Collections.Generic;
using ComponentsHYBR.Utilities;
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
            Dictionary<PlayerColorIdentifier, int> victoryPoints = GameSession.instance.VictoryPointCounters;

            string activeLanguageCode = Localizer.instance.GetCurrentlySetLanguage();
            string win = Localizer.instance.TranslateToSpecificLanguage("winGame", 
                activeLanguageCode);
            string lost = Localizer.instance.TranslateToSpecificLanguage("looseGame", 
                activeLanguageCode);
            
            if (victoryPoints[PlayerColorIdentifier.Red] > victoryPoints[PlayerColorIdentifier.Blue]){
            
                gameStateTextPlayerRed.text = win;
                gameStateTextPlayerBlue.text = lost;
            
            } else if (victoryPoints[PlayerColorIdentifier.Red] < victoryPoints[PlayerColorIdentifier.Blue]){
            
                gameStateTextPlayerRed.text = lost;
                gameStateTextPlayerBlue.text = win;
            }
            
            string locationVictoryPoint = Localizer.instance.TranslateToSpecificLanguage("victoryPoints", 
                activeLanguageCode);

            victoryPointsPlayerBlue.text = $"{locationVictoryPoint}: {victoryPoints[PlayerColorIdentifier.Blue]}";
            victoryPointsPlayerRed.text = $"{locationVictoryPoint}: {victoryPoints[PlayerColorIdentifier.Red]}";
        }

        private void ReturnToStartButton()
        {
            CloseMenu();
            GameSession.instance.GameHasStarted = false;
            UiManager.CallbackUiEvent("MainMenu");
        }
    }
}