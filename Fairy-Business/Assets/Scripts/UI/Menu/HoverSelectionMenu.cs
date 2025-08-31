using System.Collections.Generic;
using Locations;
using Player;
using UI.Menu.BaseMenu;
using UnityEngine;

namespace UI.Menu
{
    public class HoverSelectionMenu : MenuElement
    {
        [SerializeField] protected List<Transform> hoverParent;
        [SerializeField] protected LocationUI locationUIPrefab;

        public override void OpenMenu()
        {
            base.OpenMenu();
            InitializeUI();
        }

        private void InitializeUI( )
        {
            LocationDefinition locationDefinition = LocationHoverManager.instance.HoveredLocation;
            PlayerColor playerColor = LocationHoverManager.instance.CurrentPlayerColor;
            int index = LocationHoverManager.instance.LineIndex();
            
            LocationUI locationUI = Instantiate(locationUIPrefab, hoverParent[index]);
            locationDefinition.InitializeLocationUI(locationUI);

            if (playerColor == PlayerColor.Red)
            {
                //Turn UI around
                locationUI.GetComponent<RectTransform>().localScale = new Vector3(-1f, -1f, 1f);
            }
        }

        public override void CloseMenu()
        {
            base.CloseMenu();
            
            foreach (Transform parent in hoverParent)
            {
                foreach (Transform child in parent.transform) {
                    Destroy(child.gameObject);
                }
            }
        }
    }
}