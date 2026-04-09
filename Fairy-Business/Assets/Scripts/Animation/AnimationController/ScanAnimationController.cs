using Animation.AnimationEffects;
using Player;
using UnityEngine;
using UnityEngine.UI;

namespace Animation.AnimationController
{
    public class ScanAnimationController : BaseAnimationController
    {
        [SerializeField] private PlayerColor playerColor;
        [SerializeField] private ScanAction scanAction;
        
        [SerializeField] private BaseAnimationEffect changeColorEffect;
        [SerializeField] private Sprite defaultSprite;
        
        private Image image;

        protected override void Awake()
        {
            base.Awake();
            
            image = GetComponent<Image>();

            GameSession.OnCardScanned += OnCardScanned;
            GameSession.OnTurnReset += ResetAnimation;
        }

        private void OnDestroy()
        {
            GameSession.OnCardScanned -= OnCardScanned;
            GameSession.OnTurnReset -= ResetAnimation;
        }

        private void OnCardScanned(PlayerColor playerColor, ScanAction scanAction)
        {
            if (playerColor != this.playerColor || scanAction != this.scanAction)
                return;

            StartAnimations();
        }

        private void ResetAnimation()
        {
            image.sprite = defaultSprite;
        }
    }
}