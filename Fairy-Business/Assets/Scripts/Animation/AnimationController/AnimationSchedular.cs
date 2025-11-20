using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DG.Tweening;

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

    public class AnimationScheduler : MonobheaviourSingletonCustom<AnimationScheduler>
    {
        private readonly List<AnimationJob> jobs = new();
        private bool isRunning;

        public void AddJob(AnimationJob job)
        {
            jobs.Add(job);
            TryRun();
        }

        private async void TryRun()
        {
            if (isRunning) return; 
            
            isRunning = true;
            
            while (jobs.Count > 0)
            {
                int highestPriority = jobs.Max(j => j.Priority);

                // alle Jobs mit gleicher Priorität
                List<AnimationJob> samePriorityJobs = jobs.Where(j => j.Priority == highestPriority).ToList();

                // StartAnimation aufrufen
                foreach (AnimationJob job in samePriorityJobs)
                {
                    job.AnimationFlow.StartAnimation();
                }

                // Dann auf alle Sequenzen warten
                Task[] tasks = samePriorityJobs.Select(job => job.AnimationFlow.Sequence.AsyncWaitForCompletion()).ToArray();

                await Task.WhenAll(tasks);
                
                // Fertige Jobs aus der Liste entfernen
                foreach (var job in samePriorityJobs)
                    jobs.Remove(job);
            }
            
            isRunning = false;
        }
    }
}