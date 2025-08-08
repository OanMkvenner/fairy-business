using UnityEngine;

namespace UI.Menu
{
    public abstract class MenuElement : MonoBehaviour
    {
        public MenuIdentifier MenuIdentifier => menuIdentifier;
        
        protected MenuIdentifier menuIdentifier;

        [SerializeField] private MenuManager menuManager;
        [SerializeField] private GameObject menuContent;
        private bool isOpen = false;

        private void Awake()
        {
            menuManager.RegisterMenuElement(this);
        }

        public void OpenMenu()
        {
            isOpen = true;
            menuContent.SetActive(true);
        }

        protected void CloseMenu()
        {
            isOpen = false;
            menuContent.SetActive(false);
        }
    }
}