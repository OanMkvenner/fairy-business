using UI.Menu.BaseMenu;
using UnityEngine;
using UnityEngine.UI;

namespace UI.Menu.Buttons
{
    [RequireComponent(typeof(Button))]
    public class CloseMenuButton : MonoBehaviour
    {
        [SerializeField] private MenuIdentifier menuIdentifier;
        private Button closeButton;
        
        private void Awake()
        {
            closeButton = GetComponent<Button>();
            closeButton.onClick.AddListener(CloseMenu);
        }

        private void OnDestroy()
        {
            closeButton.onClick.RemoveAllListeners();
        }

        private void CloseMenu()
        {
            MenuManager.instance.CloseMenu(menuIdentifier);
        }
    }
}