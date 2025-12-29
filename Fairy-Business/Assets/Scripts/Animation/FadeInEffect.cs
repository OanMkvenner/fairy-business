using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

namespace Animation
{
    [RequireComponent(typeof(Image))]
    public class FadeInEffect : MonoBehaviour
    {
        [SerializeField] private float duration = 0.5f;
        [SerializeField] private float alphaValue = 1f;
        
        private Image image;
        private Sequence sequence;

        private void Awake()
        {
            image = GetComponent<Image>();
            image.color = new Color(image.color.r, image.color.g, image.color.b, 0);
        }

        private void OnEnable()
        {
            if(sequence != null)
                sequence.Kill();
            
            sequence = DOTween.Sequence();
       
            Tween fadeIn = image.DOFade(alphaValue, duration);
            
            sequence.Append(fadeIn);
        }

        private void OnDisable()
        {
            if(sequence != null)
                sequence.Kill();
            
            sequence = DOTween.Sequence();
       
            Tween fadeIn = image.DOFade(0, 0);
            
            sequence.Append(fadeIn);
        }
    }
}