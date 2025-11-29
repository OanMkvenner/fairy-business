using Animation.AnimationEffects;
using Player;
using UnityEngine;
using Color = UnityEngine.Color;
using Image = UnityEngine.UI.Image;

namespace Animation.AnimationController
{
    public class ScanAnimationController : BaseAnimationController
    {
        [SerializeField] private PlayerColor playerColor;
        [SerializeField] private ScanAction scanAction;
        
        [SerializeField] private BaseAnimationEffect changeColorEffect;
        [SerializeField] private Color color;
        [SerializeField] private Image image;

        protected override void Awake()
        {
            base.Awake();

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
            image.color = color;
        }
    }
}