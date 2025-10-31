using System;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

namespace Animation
{
    public class AnimationFlow : MonoBehaviour
    {
        public Sequence Sequence { get; private set; }

        [SerializeField] private List<BaseAnimation> animations;

        public void StartAnimation()
        {
            Sequence?.Kill();
            
            Sequence = DOTween.Sequence();

            foreach (BaseAnimation animation in animations)
            {
                Tween tween = animation.Play();
                
                AnimationSettings settings = animation.AnimationSettings;
                
                Action<Sequence, Tween> insertAction = GetSequenceType(settings.SequenceInsertType, settings.InsertAtTime);
                
                insertAction(Sequence, tween);
            }
            
            Sequence.Play();
        }

        private Action<Sequence, Tween> GetSequenceType(SequenceInsertType sequenceInsertType, float insertTime = 0f)
        {
            return sequenceInsertType switch
            {
                SequenceInsertType.Append => (seq, tween) => seq.Append(tween),
                SequenceInsertType.Join   => (seq, tween) => seq.Join(tween),
                SequenceInsertType.Insert => (seq, tween) => seq.Insert(insertTime, tween),
                SequenceInsertType.AppendInterval => (seq, tween) => seq.AppendInterval(insertTime),
                _ => (seq, tween) => seq.Append(tween)
            };
        }
    }
}