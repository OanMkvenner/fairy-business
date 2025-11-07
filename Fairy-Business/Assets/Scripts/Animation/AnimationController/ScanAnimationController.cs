using Player;
using UnityEngine;

namespace Animation.AnimationController
{
    public class PlayerScanAnimationController : BaseAnimationController<PlayerScanAnimationController>
    {
        [SerializeField] private PlayerColor playerColor;
        [SerializeField] private ScanAction scanAction;
        
        protected override void Awake()
        {
            base.Awake();
            
            GameSession.OnCardScanned += OnCardScanned;
        }

        private void OnDestroy()
        {
            GameSession.OnCardScanned -= OnCardScanned;
        }

        private void OnCardScanned(PlayerColor playerColor, ScanAction scanAction)
        {
            if (playerColor != this.playerColor || scanAction != this.scanAction)
                return;
            
            StartAnimations();
        }
    }
}