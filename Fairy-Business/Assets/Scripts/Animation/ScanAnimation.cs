using System;
using DG.Tweening;
using Player;
using UnityEngine;
using UnityEngine.UI;

namespace Animation
{
    public class ScanAnimation : MonoBehaviour
    {
        public Sequence Sequence => sequence;
        
        [SerializeField] private PlayerColor playerColor;
        [SerializeField] private ScanAction scanAction;

        [SerializeField] private Transform middleScreenPosition;
        [SerializeField] private Transform defaultPosition;
        
        [Header("Sprites")] 
        [SerializeField] private Sprite activeSprite;
        [SerializeField] private Sprite inactiveSprite;

        [Header("Objects to Animate")] 
        [SerializeField] private GameObject creditCard;

        private Sequence sequence;
        private Image cardImage;
        private RectTransform cardTransform;

        private const float InitialScale = 1f;
        private const float EnlargedScale = 1.5f;

        private void Awake()
        {
            GameSession.OnCardScanned += OnCardScanned;
            CacheComponents();
        }

        private void Start()
        {
            InitializeCard();
        }

        private void OnDestroy()
        {
            GameSession.OnCardScanned -= OnCardScanned;
        }

        public void ResetUI()
        {
            cardImage.sprite = inactiveSprite;
        }
        
        private void CacheComponents()
        {
            cardTransform = creditCard.GetComponent<RectTransform>();
            cardImage = creditCard.GetComponent<Image>();
        }

        private void InitializeCard()
        {
            creditCard.transform.position = defaultPosition.position;
            creditCard.transform.localScale = Vector3.one * InitialScale;
            cardImage.sprite = inactiveSprite;
        }

        private void OnCardScanned(PlayerColor playerColor, ScanAction scanAction)
        {
            if (playerColor != this.playerColor || scanAction != this.scanAction)
                return;

            PlayScanAnimation();
        }

        private void PlayScanAnimation()
        {
            sequence?.Kill();

            cardTransform.localScale = Vector3.one * EnlargedScale;
            cardImage.sprite = activeSprite;

            sequence = DOTween.Sequence()
                .Append(cardTransform.DOLocalMove(middleScreenPosition.position, 0.5f).SetEase(Ease.OutExpo))
                .Append(cardTransform.DOShakeScale(0.2f, 0.5f, 1, 90f, true, ShakeRandomnessMode.Harmonic))
                .Append(cardTransform.DOMove(defaultPosition.position, 0.9f))
                .Join(cardTransform.DORotate(defaultPosition.eulerAngles, 0.9f))
                .Join(cardTransform.DOScale(Vector3.one * InitialScale, 0.9f));
        }
    }
}
