using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DG.Tweening;
using UnityEngine;

namespace Animation.AnimationController
{
    public class AnimationScheduler : MonobehaviourSingletonCustom<AnimationScheduler>
    {
        private readonly List<AnimationJob> jobBuffer = new();
        private bool isRunning;

        public void Enqueue(AnimationJob job)
        {
            jobBuffer.Add(job);
            TryRun();
        }

        private async void TryRun()
        {
            if (isRunning)
                return;

            isRunning = true;

            while (jobBuffer.Count > 0)
            {
                // höchste aktuelle Priorität finden
                int highest = jobBuffer.Max(j => j.Priority);

                // alle Jobs mit dieser Priorität sammeln
                List<AnimationJob> batch = jobBuffer
                    .Where(j => j.Priority == highest)
                    .ToList();

                Debug.Log($"Starting batch with priority {highest}, count {batch.Count}");

                // Animationen starten
                foreach (var job in batch)
                {
                    job.AnimationFlow.StartAnimation();
                }

                // auf alle Sequenzen warten (parallel)
                Task[] tasks = batch
                    .Select(j => j.AnimationFlow.Sequence.AsyncWaitForCompletion())
                    .ToArray();

                await Task.WhenAll(tasks);

                Debug.Log($"Finished batch with priority {highest}");

                // fertige Jobs entfernen
                foreach (var job in batch)
                    jobBuffer.Remove(job);
            }

            isRunning = false;
        }
    }
}