using System;
using System.Collections;
using Locations;
using Player;
using UI.Menu.BaseMenu;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Serialization;

namespace UI.Buttons
{
    [RequireComponent(typeof(EventTrigger))]
    public class LocationHoverButton : MonoBehaviour
    {
        [SerializeField] private bool inGameView;
        [SerializeField] private LocationDefinition locationDefinition;
        [SerializeField] private PlayerColor playerColor;
        
        private LineIdentifier line => locationDefinition.PlayerLine.line;
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
            LocationHoverManager.instance.HoveredLocation = locationDefinition;
            LocationHoverManager.instance.CurrentLine = line;
            LocationHoverManager.instance.CurrentPlayerColor = playerColor;
            MenuManager.instance.OpenMenu(MenuIdentifier.HoverSelectionMenu);
        }
        
        private IEnumerator LongPressDetection()
        {
            yield return new WaitForSeconds(longPressedTime); // Wartezeit für "Lang-Druck"
            isLongPressTriggered = true;
            // Lang-Druck-Aktion (z. B. spezielles Feature)
            LongPressedAction();
        }
    }
}