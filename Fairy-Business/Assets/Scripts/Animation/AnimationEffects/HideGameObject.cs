using UnityEngine;

namespace Animation.AnimationEffects
{
    public class HideGameObject : BaseAnimationEffect
    {
        [SerializeField] private GameObject objectToHide;
        
        public override void ApplyEffect()
        {
            objectToHide.SetActive(false);
        }
    }
}