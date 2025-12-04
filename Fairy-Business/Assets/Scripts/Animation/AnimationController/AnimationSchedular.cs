using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Animation.AnimationController
{
    public class AnimationScheduler : MonobehaviourSingletonCustom<AnimationScheduler>
    {
        private readonly List<AnimationJob> jobs = new();

        public void AddAnimationJob(AnimationJob job)
        {
            jobs.Add(job);
            TryRun();
        }

        private async void TryRun()
        {
            while (jobs.Count > 0)
            {
                int highestPriority = jobs.Max(job => job.Priority);

                List<AnimationJob> highestPriorityJobs =
                    jobs.Where(job => job.Priority == highestPriority).ToList();

                // Start animations + collect tasks
                List<Task> tasks = highestPriorityJobs.Select(job => job.AnimationFlow.PlayAsync()).ToList();

                // wait for them to finish
                await Task.WhenAll(tasks);

                // remove jobs
                foreach (AnimationJob job in highestPriorityJobs)
                {
                    jobs.Remove(job);
                }
            }
        }
    }
}