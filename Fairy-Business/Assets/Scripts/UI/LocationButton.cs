using System.Collections;
using Locations;
using UI.Menu.BaseMenu;
using UnityEngine;
using UnityEngine.EventSystems;

namespace UI
{
    [RequireComponent(typeof(LocationDefinition))]
    public class LocationButton : MonoBehaviour
    {
        private Coroutine longPressCoroutine;
        private bool isLongPressTriggered = false;
        private float longPressedTime = 0.3f;
        private LocationDefinition locationDefinition;

        private void Awake()
        {
            locationDefinition = GetComponent<LocationDefinition>();
        }

        public void OnPointerDown(BaseEventData eventData)
        {
            isLongPressTriggered = false;
            longPressCoroutine = StartCoroutine(LongPressDetection());
        }
        
        public void OnPointerUp(BaseEventData eventData)
        {
            MenuManager.instance.CloseMenu(MenuIdentifier.SimpleSelectionMenu);
            
            if (longPressCoroutine != null)
            {
                StopCoroutine(longPressCoroutine);
                
                if (!isLongPressTriggered)
                {
                    // Kurz-Druck-Aktion (z. B. einfacher Klick)
                    LocationManager.instance.SetupSelectLocation(locationDefinition);
                }
            }
        }
        
        private IEnumerator LongPressDetection()
        {
            yield return new WaitForSeconds(longPressedTime); // Wartezeit für "Lang-Druck"
            isLongPressTriggered = true;
            // Lang-Druck-Aktion (z. B. spezielles Feature)
            LocationManager.instance.HoveredLocation = locationDefinition;
            MenuManager.instance.OpenMenu(MenuIdentifier.SimpleSelectionMenu);
        }
    }
}