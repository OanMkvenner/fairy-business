using JetBrains.Annotations;
using Unity.Android.Gradle.Manifest;
using UnityEngine;
using UnityEngine.UI;
using Action = System.Action;

namespace UI.Menu
{
    public abstract class MenuElement : MonoBehaviour
    {
        public static event Action OnMenuClosed;
        public MenuIdentifier MenuIdentifier => menuIdentifier;
        
        [Space]
        [SerializeField] private MenuIdentifier menuIdentifier;
        
        [Header("UI Elements")]
        [SerializeField] private GameObject menuContent;
        
        [SerializeField] private Button closeButton;
        
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
            
            if(closeButton != null)
                closeButton.onClick.AddListener(CloseMenu);
            
            isOpen = true;
            menuContent.SetActive(true);
        }

        protected virtual void CloseMenu()
        {
            isOpen = false;
            OnMenuClosed?.Invoke();
            menuContent.SetActive(false);
        }
    }
}