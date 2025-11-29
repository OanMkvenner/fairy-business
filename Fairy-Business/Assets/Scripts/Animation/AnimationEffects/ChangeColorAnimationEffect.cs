using UnityEngine;
using UnityEngine.UI;

namespace Animation.AnimationEffects
{
    [RequireComponent(typeof(Image))]
    public class ChangeColorAnimationEffect : BaseAnimationEffect
    {
        [SerializeField] private Color color;
        private Image image;

        private void Awake()
        {
            image = GetComponent<Image>();
        }

        public override void ApplyEffect()
        {
            image.color = color;
        }
    }
}