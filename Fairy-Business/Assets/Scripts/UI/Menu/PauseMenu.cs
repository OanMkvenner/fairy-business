using UnityEngine;
using UnityEngine.UI;

namespace UI.Menu
{
    public class PauseMenu : MenuElement
    {
        [SerializeField] private Button returnToStartScreen;
        

        private void Awake()
        {
            returnToStartScreen.onClick.AddListener(ReturnToStartScreen);
        }
        
        private void ReturnToStartScreen()
        {
            UiManager.CallbackUiEvent("MainMenu");
        }
    }
}