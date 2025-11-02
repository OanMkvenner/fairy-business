using System;
using DG.Tweening;
using UnityEngine;

namespace Animation
{
    public enum SequenceInsertType
    {
        None,
        Join,
        Append,
        Insert
    }
    
    [Serializable]
    public class AnimationSettings
    {
        [field: SerializeField] public float Duration { get; set; }
        [field: SerializeField] public Ease Ease { get; set; }
        [field: SerializeField] public Vector3 VectorValue { get; set; }
        [field: SerializeField] public bool AppendInterval { get; set; }
        [field: SerializeField] public float AppendIntervalTime { get; set; }
        [field: SerializeField] public SequenceInsertType SequenceInsertType { get; set; }

        public AnimationSettings(float duration, Ease ease, Vector3 vectorValue, SequenceInsertType sequenceInsertType)
        {
            Duration = duration;
            Ease = ease;
            VectorValue = vectorValue;
            SequenceInsertType = sequenceInsertType;
        }
    }
}