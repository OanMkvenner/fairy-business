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
        [field: SerializeField] public bool AppendInterval { get; set; }
        [field: SerializeField] public float AppendIntervalTime { get; set; }
        [field: SerializeField] public SequenceInsertType SequenceInsertType { get; set; }
        
        private UIPosition uiEndPosition;

        public AnimationSettings(float duration, Ease ease, UIPosition uiEndPosition, SequenceInsertType sequenceInsertType)
        {
            Duration = duration;
            Ease = ease;
            uiEndPosition = uiEndPosition;
            SequenceInsertType = sequenceInsertType;
        }

        public Vector2 GetUIPosition(Canvas canvas)
        {
            return uiEndPosition.GetUIPosition(canvas);
        } 
    }
}