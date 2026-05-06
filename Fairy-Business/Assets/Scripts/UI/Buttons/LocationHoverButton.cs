using System;
using System.Collections;
using Locations;
using Player;
using UI.Menu.BaseMenu;
using UnityEngine;
using UnityEngine.EventSystems;

namespace UI.Buttons
{
    [RequireComponent(typeof(EventTrigger))]
    public class LocationHoverButton : MonoBehaviour
    {
        public static event Action<LocationDefinition> LongPressDetectionEvent;
        
        [SerializeField] private bool inGameView;
        [SerializeField] private LocationDefinition locationDefinition;
        [SerializeField] private PlayerColorIdentifier playerColorIdentifier;
        
        private Coroutine longPressCoroutine;
        private bool isLongPressTriggered = false;
        private readonly float longPressedTime = 0.3f;

        public void OnPointerDown(BaseEventData eventData)
        {
            isLongPressTriggered = false;
            longPressCoroutine = StartCoroutine(LongPressDetection());
        }
        
        public void OnPointerUp(BaseEventData eventData)
        {
            MenuManager.instance.CloseMenu(MenuIdentifier.HoverSelectionMenu);
            
            if (longPressCoroutine != null)
            {
                StopCoroutine(longPressCoroutine);
                
                if (!isLongPressTriggered && !inGameView)
                {
                    // Kurz-Druck-Aktion (z. B. einfacher Klick)
                    LocationManager.instance.SetupSelectLocation(locationDefinition);
                }
            }
        }

        private void LongPressedAction()
        {
            LongPressDetectionEvent?.Invoke(locationDefinition);
        }
        
        private IEnumerator LongPressDetection()
        {
            yield return new WaitForSeconds(longPressedTime);
            isLongPressTriggered = true;
            LongPressedAction();
        }
    }
}