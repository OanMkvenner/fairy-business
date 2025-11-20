namespace Animation.AnimationController
{
    public class NewTurnAnimationController : BaseAnimationController
    {
        protected override void Awake()
        {
            base.Awake();
            GameSession.OnTurnReset += Fodd;
        }

        private void OnDestroy()
        {
            GameSession.OnTurnReset -= Fodd;
        }

        private void Fodd()
        {
            Sounds.instance.Play("NewTurn");
            StartAnimations();
        }
    }
}