namespace Animation.AnimationController
{
    public class NewTurnAnimationController : BaseAnimationController
    {
        protected override void Awake()
        {
            base.Awake();
            GameSession.OnTurnReset += StartTurnAnimation;
        }

        private void OnDestroy()
        {
            GameSession.OnTurnReset -= StartTurnAnimation;
        }

        private void StartTurnAnimation()
        {
            Sounds.instance.Play("NewTurn");
            StartAnimations();
        }
    }
}