using System;
using Animation.Position;
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
        [field: SerializeField] public Transform UiEndPosition { get; set; }
        [field: SerializeField] public SequenceInsertType SequenceInsertType { get; set; }
        [field: SerializeField] public bool AppendInterval { get; set; }
        [field: SerializeField] public float AppendIntervalTime { get; set; }

        public AnimationSettings(float duration, Ease ease, Transform uiEndPosition, SequenceInsertType sequenceInsertType)
        {
            Duration = duration;
            Ease = ease;
            UiEndPosition = uiEndPosition;
            SequenceInsertType = sequenceInsertType;
        }
    }
}