using ComponentsHYBR.Utilities;
using UnityEngine;
using Action = System.Action;

namespace UI.Menu.BaseMenu
{
    public abstract class MenuElement : MonoBehaviour
    {
        public static event Action OnMenuClosed;
        public MenuIdentifier MenuIdentifier => menuIdentifier;
        
        [Space]
        [SerializeField] private MenuIdentifier menuIdentifier;
        
        [Header("UI Elements")]
        [SerializeField] private GameObject menuContent;
        
        private bool isOpen = false;
        
        private void Start()
        {
            MenuManager.instance.RegisterMenuElement(this);
            menuContent.SetActive(false);
        }

        public virtual void OpenMenu()
        {
            if (isOpen)
                return;
            
            isOpen = true;
            menuContent.SetActive(true);
            
            Sounds.instance.Play("MenuOpening");
        }

        public virtual void CloseMenu()
        {
            isOpen = false;
            OnMenuClosed?.Invoke();
            menuContent.SetActive(false);
            
            Sounds.instance.Play("MenuClosing");
        }
    }
}