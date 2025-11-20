using System;
using Animation.Tweens;
using DG.Tweening;
using UnityEngine;

namespace Animation
{
    public class AnimationFlow : MonoBehaviour
    {
        public Sequence Sequence { get; private set; }
        public bool isPlaying { get; private set; }

        [SerializeField] private bool playOnEnable;
        
        private BaseTweenAnimation[] animations;
        

        private void Awake()
        {
            animations = GetComponents<BaseTweenAnimation>();
        }

        private void OnEnable()
        {
            if (!playOnEnable)
                return;
            
            StartAnimation();
        }
        
        public void StartAnimation()
        {
            Sequence?.Kill();
            
            Sequence = DOTween.Sequence();

            foreach (BaseTweenAnimation animation in animations)
            {
                Tween tween = animation.Play();
                
                AnimationSettings settings = animation.AnimationSettings;
                
                Action<Sequence, Tween> insertAction = GetSequenceType(settings.SequenceInsertType);

                insertAction(Sequence, tween);
                
                if (!settings.AppendInterval)
                    continue;

                if (settings.AppendIntervalTime <= 0)
                {
                    Debug.LogError($"The Append Interval Time is {settings.AppendIntervalTime}, therefore now Interval is added to the sequence");
                    continue;
                }

                Sequence.AppendInterval(settings.AppendIntervalTime);
            }
            
            Sequence.Play();
            isPlaying = true;

            Sequence.OnComplete(() => isPlaying = false);
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