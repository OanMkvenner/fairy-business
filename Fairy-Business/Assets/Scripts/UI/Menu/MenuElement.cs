using UnityEngine;
using UnityEngine.UI;

namespace UI.Menu
{
    public abstract class MenuElement : MonoBehaviour
    {
        public MenuIdentifier MenuIdentifier => menuIdentifier;
        
        [SerializeField] private MenuManager menuManager;
        [Space]
        [SerializeField] private MenuIdentifier menuIdentifier;
        
        [Header("UI Elements")]
        [SerializeField] private GameObject menuContent;
        
        [SerializeField] private Button closeButton;
        
        private bool isOpen = false;
        
        private void Start()
        {
            menuManager.RegisterMenuElement(this);
            menuContent.SetActive(false);
        }

        public virtual void OpenMenu()
        {
            if (isOpen)
                return;
            closeButton.onClick.AddListener(CloseMenu);
            
            isOpen = true;
            menuContent.SetActive(true);
        }

        protected virtual void CloseMenu()
        {
            isOpen = false;
            menuContent.SetActive(false);
        }
    }
}