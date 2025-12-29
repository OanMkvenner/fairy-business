using UI.Menu.BaseMenu;
using UnityEngine;
using UnityEngine.UI;

namespace UI.Menu.Buttons
{
    [RequireComponent(typeof(Button))]
    public class OpenMenuButton : MonoBehaviour
    {
        [SerializeField] private MenuIdentifier identifier;
        private Button button;

        private void Awake()
        {
            button = GetComponent<Button>();
            
            button.onClick.AddListener(OpenMenu);
        }

        private void OnDestroy()
        {
            button.onClick.RemoveAllListeners();
        }

        private void OpenMenu()
        {
            MenuManager.instance.OpenMenu(identifier);
        }
    }
}