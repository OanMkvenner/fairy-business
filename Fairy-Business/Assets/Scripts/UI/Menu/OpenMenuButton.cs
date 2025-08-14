using UnityEngine;
using UnityEngine.UI;

namespace UI.Menu
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

        private void OpenMenu()
        {
            MenuManager.OpenMenu(identifier);
        }
    }
}