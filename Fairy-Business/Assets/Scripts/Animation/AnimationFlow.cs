using System;
using DG.Tweening;
using UnityEngine;

namespace Animation
{
    public class AnimationFlow : MonoBehaviour
    {
        public Sequence Sequence { get; private set; }

        [SerializeField] private bool playOnEnable;
        private BaseAnimation[] animations;

        private void Awake()
        {
            animations = GetComponents<BaseAnimation>();
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

            foreach (BaseAnimation animation in animations)
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