using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Animation.AnimationController
{
    public class AnimationScheduler : MonobehaviourSingletonCustom<AnimationScheduler>
    {
        private readonly List<AnimationJob> queuedJobs = new();
        private bool isRunning;

        public void AddAnimationJob(AnimationJob job)
        {
            queuedJobs.Add(job);
            _ = TryRunAsync();
        }

        private async Task TryRunAsync()
        {
            if (isRunning)
                return;

            isRunning = true;

            try
            {
                while (queuedJobs.Count > 0)
                {
                    // 1️⃣ höchste Priorität bestimmen
                    int highestPriority =
                        queuedJobs.Max(j => j.Priority);

                    // 2️⃣ Batch reservieren (WICHTIG)
                    List<AnimationJob> batch =
                        queuedJobs
                            .Where(j => j.Priority == highestPriority)
                            .ToList();

                    foreach (var job in batch)
                        queuedJobs.Remove(job);

                    // 3️⃣ Animationen parallel starten
                    List<Task> tasks =
                        batch
                            .Select(j => j.AnimationFlow.PlayAsync())
                            .ToList();

                    // 4️⃣ warten bis alle fertig sind
                    await Task.WhenAll(tasks);
                }
            }
            finally
            {
                isRunning = false;
            }
        }
    }
}