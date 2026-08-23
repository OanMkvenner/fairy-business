using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace UI.Menu.Buttons
{
    [RequireComponent(typeof(Button))]
    public class ChangeStateButton: MonoBehaviour
    {
        [SerializeField] private List<GameObject> objectsToActivate;
        [SerializeField] private List<GameObject> objectsToDeactivate;

        private void Awake()
        {
            if (!TryGetComponent(out Button button))
                return;
            
            button.onClick.AddListener(OnClick);
        }

        private void OnDestroy()
        {
            if (!TryGetComponent(out Button button))
                return;
            
            button.onClick.RemoveAllListeners();
        }

        private void OnClick()
        {
            foreach (GameObject objectToActivate in objectsToActivate)
            {
                objectToActivate.SetActive(true);
            }

            foreach (GameObject objectToDeactivate in objectsToDeactivate)
            {
                objectToDeactivate.SetActive(false);
            }
        }
    }
}