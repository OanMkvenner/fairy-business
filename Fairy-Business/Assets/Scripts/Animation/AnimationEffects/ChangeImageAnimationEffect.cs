using UnityEngine;
using UnityEngine.UI;

namespace Animation.AnimationEffects
{
    [RequireComponent(typeof(Image))]
    public class ChangeImageAnimationEffect : BaseAnimationEffect
    {
        [SerializeField] private Sprite sprite;
        
        private Image image;

        private void Awake()
        {
            image = GetComponent<Image>();
        }
        
        public override void ApplyEffect()
        {
            image.sprite = sprite;
        }
    }
}