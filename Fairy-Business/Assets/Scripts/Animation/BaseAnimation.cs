using DG.Tweening;
using UnityEngine;

namespace Animation
{
    public abstract class BaseAnimation : MonoBehaviour, IUIAnimation
    {
        [field: SerializeField] protected AnimationSettings AnimationSettings {get; set;}
        protected abstract Tween PlayAnimation();
        
        public Tween Play()
        {
            return PlayAnimation();
        }
    }
}