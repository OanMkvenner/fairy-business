using System;
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
        [SerializeField] private Button closeButton;
        [SerializeField] private GameObject menuContent;
        
        private bool isOpen = false;

        private void Awake()
        {
            closeButton.onClick.AddListener(CloseMenu);
        }

        private void Start()
        {
            menuManager.RegisterMenuElement(this);
            menuContent.SetActive(false);
        }

        public virtual void OpenMenu()
        {
            if (isOpen)
                return;
            
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