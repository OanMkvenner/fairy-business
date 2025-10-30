using System;
using DG.Tweening;
using UnityEngine;

namespace Animation
{
    [Serializable]
    public struct AnimationSettings
    {
        public float Duration { get; set; }
        public Ease Ease { get; set; }
        public Vector3 VectorValue { get; set; }

        public AnimationSettings(float duration, Ease ease, Vector3 vectorValue)
        {
            Duration = duration;
            Ease = ease;
            VectorValue = vectorValue;
        }
    }
}