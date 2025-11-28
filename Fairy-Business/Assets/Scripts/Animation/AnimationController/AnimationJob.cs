namespace Animation.AnimationController
{
    public class AnimationJob
    {
        public int Priority { get; }
        public AnimationFlow AnimationFlow { get; }

        public AnimationJob(AnimationFlow animationFlow, int priority)
        {
            Priority = priority;
            AnimationFlow = animationFlow;
        }
    }
}