using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DG.Tweening;
using UnityEngine;

namespace Animation.AnimationController
{
    public class AnimationScheduler : MonobehaviourSingletonCustom<AnimationScheduler>
    {
        private readonly List<AnimationJob> jobs = new();

        public void AddJob(AnimationJob job)
        {
            jobs.Add(job);
            TryRun();
        }

        private async void TryRun()
        {
            while (jobs.Count > 0)
            {
                int highestPriority = jobs.Max(j => j.Priority);

                // alle Jobs mit gleicher Priorität
                List<AnimationJob> samePriorityJobs = jobs.Where(j => j.Priority == highestPriority).ToList();

                // StartAnimation aufrufen
                foreach (AnimationJob job in samePriorityJobs)
                {
                    Debug.Log($"Starting job with priority {job.Priority}");
                    job.AnimationFlow.StartAnimation();
                }

                // Dann auf alle Sequenzen warten
                Task[] tasks = samePriorityJobs.Select(job => job.AnimationFlow.Sequence.AsyncWaitForCompletion()).ToArray();

                await Task.WhenAll(tasks);
                
                // Fertige Jobs aus der Liste entfernen
                foreach (var job in samePriorityJobs)
                    jobs.Remove(job);
            }
        }
    }
}