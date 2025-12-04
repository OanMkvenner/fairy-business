using System;
using System.Threading.Tasks;
using Animation.Tweens;
using DG.Tweening;
using UnityEngine;

namespace Animation
{
    public class AnimationFlow : MonoBehaviour
    {
        public bool IsPlaying { get; private set; }

        [SerializeField] private bool playOnEnable;

        private BaseTweenAnimation[] animations;
        private Sequence Sequence { get; set; }

        private void Awake()
        {
            animations = GetComponents<BaseTweenAnimation>();
        }

        private void OnEnable()
        {
            if (!playOnEnable)
                return;
            
            PlayAsync();
        }
        
        public Task PlayAsync()
        {
            var tcs = new TaskCompletionSource<bool>();

            Sequence?.Kill();
            Sequence = DOTween.Sequence();

            foreach (BaseTweenAnimation animation in animations)
            {
                Tween tween = animation.Play();
                AnimationSettings settings = animation.AnimationSettings;

                Action<Sequence, Tween> insertAction = GetSequenceType(settings.SequenceInsertType);
                insertAction(Sequence, tween);

                if (settings.AppendInterval && settings.AppendIntervalTime > 0)
                    Sequence.AppendInterval(settings.AppendIntervalTime);
            }

            IsPlaying = true;

            Sequence.OnComplete(() =>
            {
                IsPlaying = false;
                tcs.TrySetResult(true);
            });

            Sequence.Play();
            return tcs.Task;
        }

        private Action<Sequence, Tween> GetSequenceType(SequenceInsertType sequenceInsertType)
        {
            return sequenceInsertType switch
            {
                SequenceInsertType.Append => (seq, tween) => seq.Append(tween),
                SequenceInsertType.Join   => (seq, tween) => seq.Join(tween),
                _ => (seq, tween) => seq.Append(tween)
            };
        }
    }
}