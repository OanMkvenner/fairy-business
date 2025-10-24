using DG.Tweening;
using UnityEngine;

namespace Animation
{
    [RequireComponent(typeof(RectTransform))]
    public class NewTurnAnimation : MonoBehaviour
    {
        [SerializeField] private float duration = 0.6f;
        [SerializeField] private Transform targetPosition;
        
        private RectTransform rectTransform;
        private Transform defaultTransform;
        
        private Sequence sequence;

        private void Awake()
        {
            defaultTransform = transform;
            rectTransform = GetComponent<RectTransform>();
        }

        public void StartAnimation()
        {
            rectTransform.position = defaultTransform.position;
            sequence?.Kill();

            Tween move = rectTransform.DOMove(targetPosition.position, duration).SetEase(Ease.InOutBack);
            Tween moveBack = rectTransform.DOMove(defaultTransform.position, duration).SetEase(Ease.OutQuint);
            
            sequence = DOTween.Sequence();
            
            sequence.Append(move);
            sequence.AppendInterval(1f);
            sequence.Append(moveBack);
        }
    }
}