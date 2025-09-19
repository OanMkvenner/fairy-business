using System.Collections.Generic;
using Player;
using UnityEngine;
using UnityEngine.UI;

namespace UI.Gameplay
{
    public class ActionIconUI : MonoBehaviour
    {
        [SerializeField] private Sprite emptySprite;
        [SerializeField] private Sprite fullSprite;
        [SerializeField] private List<Image> icons;

        [SerializeField] private PlayerColor currentPlayerColor;

        private int spriteIndex;

        private void Awake()
        {
            GameSession.OnTurnReset += ResetUI;
            GameSession.OnCardScanned += FillIcon;
        }

        private void OnDestroy()
        {
            GameSession.OnTurnReset -= ResetUI;
            GameSession.OnCardScanned -= FillIcon;
        }

        private void ResetUI()
        {
            foreach (Image icon in icons)
            {
                icon.sprite = emptySprite;
            }
            
            spriteIndex = 0;
        }

        private void FillIcon(PlayerColor playerColor)
        {
            if (playerColor != currentPlayerColor)
                return;

            if (spriteIndex >= icons.Count)
                return;
            
            icons[spriteIndex].sprite = fullSprite;
            spriteIndex++;
        }
    }
}