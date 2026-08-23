using System.Collections.Generic;
using UnityEngine;

namespace UI.Menu.Buttons
{
    public class ChangeStateButton: BaseButton
    {
        [SerializeField] private List<GameObject> objectsToActivate;
        [SerializeField] private List<GameObject> objectsToDeactivate;


        protected override void OnClick()
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