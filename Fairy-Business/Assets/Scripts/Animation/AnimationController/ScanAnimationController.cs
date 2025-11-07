using Animation.Position;
using Player;
using UnityEngine;

namespace Animation.AnimationController
{
    public class ScanAnimationController : BaseAnimationController<ScanAnimationController>
    {
        [SerializeField] private PlayerColor playerColor;
        [SerializeField] private ScanAction scanAction;
        [SerializeField] private GameObject animatedObject;

        private Canvas canvas;

        protected override void Awake()
        {
            base.Awake();

            canvas = GetComponent<Canvas>();
            
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