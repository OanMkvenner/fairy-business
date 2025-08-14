using UnityEngine;
using UnityEngine.UI;

namespace UI.Menu
{
    public class PauseMenu : MenuElement
    {
        [SerializeField] private Button returnToStartScreen;
        
        [Header("Script References")]
        [SerializeField] private UiGraphCallback uiGraphCallback;

        private void Awake()
        {
            returnToStartScreen.onClick.AddListener(ReturnToStartScreen);
        }
        
        private void ReturnToStartScreen()
        {
            //Todo: Marie 14.08 Aufruf des Startmenüs
           // uiGraphCallback.callCustomUiEvent("");
        }
    }
}