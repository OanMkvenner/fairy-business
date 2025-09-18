using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace UI.Gameplay
{
    public class ActionIconUI : MonoBehaviour
    {
        [SerializeField] private Sprite emptySprite;
        [SerializeField] private Sprite fullSprite;
        [SerializeField] private List<Image> icons;

        private int spriteIndex;

        private void ResetUI()
        {
            foreach (Image icon in icons)
            {
                icon.sprite = emptySprite;
            }
            
            spriteIndex = 0;
        }

        private void FillIcon()
        {
            icons[spriteIndex].sprite = fullSprite;
            spriteIndex++;
        }
    }
}