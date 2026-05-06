using Player;
using UnityEngine;
using UnityEngine.UI;

namespace Animation.AnimationController
{
    public class ScanAnimationController : BaseAnimationController
    {
        [SerializeField] private PlayerColorIdentifier playerColorIdentifier;
        [SerializeField] private ScanAction scanAction;
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

        private void OnCardScanned(PlayerColorIdentifier playerColorIdentifier, ScanAction scanAction)
        {
            if (playerColorIdentifier != this.playerColorIdentifier || scanAction != this.scanAction)
                return;

            StartAnimations();
        }

        private void ResetAnimation()
        {
            image.sprite = defaultSprite;
        }
    }
}