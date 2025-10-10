using System.Collections;
using DG.Tweening;
using Player;
using UnityEngine;
using UnityEngine.UI;
using Random = UnityEngine.Random;

namespace Animation
{
    public class ScanAnimation : MonoBehaviour
    {
        public Sequence Sequence => sequence;
        
        [SerializeField] private PlayerColor playerColor;
        [SerializeField] private ScanAction scanAction;

        [Space]
        [SerializeField] private Transform startPosition;

        [SerializeField] private Transform endPosition;
        [SerializeField] private Sprite activeSprite;
        [SerializeField] private Sprite inactiveSprite;
        [SerializeField] private GameObject playerObject;
        [SerializeField] private GameObject scanObject;

        private Sequence sequence;
        
        private void Awake()
        {
            GameSession.OnCardScanned += AnimateScannedCard;
        }

        private void OnDestroy()
        {
            GameSession.OnCardScanned -= AnimateScannedCard;
        }

        public void ResetUI()
        {
            playerObject.GetComponent<Image>().sprite = inactiveSprite;
        }

        private void AnimateScannedCard(PlayerColor playerColor, ScanAction scanAction)
        {
            if (playerColor != this.playerColor )
                return;
            
            if (scanAction != this.scanAction)
                return;
            
            sequence.Kill();
            
            scanObject.transform.position = startPosition.position;
            scanObject.transform.localScale = new Vector3(1, 1, 1);
            int randAngle = Random.Range(0, 360);
            scanObject.GetComponent<RectTransform>().rotation = Quaternion.Euler(0, 0, randAngle);
            scanObject.SetActive(true);
            playerObject.GetComponent<Image>().sprite = inactiveSprite;
            
            sequence = DOTween.Sequence();

            Tween moveToMiddleTween = scanObject.transform.DOMoveY(endPosition.position.y, 0.5f).SetEase(Ease.OutExpo);
            
            Tween shake = scanObject.transform.DOShakeScale(0.2f, 0.5f, 1, 90f, true, ShakeRandomnessMode.Harmonic);
            
            Tween rotate = scanObject.GetComponent<RectTransform>()
                .DORotate(new Vector3(0f, 0f, -360f), 0.5f, RotateMode.FastBeyond360)
                .SetEase(Ease.Linear).SetLoops(3, LoopType.Restart);
            
            Tween scale = scanObject.transform.DOScale(new Vector3(0, 0, 0), 0.9f);
            
            Tween shakePlayerObject = playerObject.transform.DOShakeScale(0.2f, 0.5f, 1, 90f, 
                true, ShakeRandomnessMode.Harmonic).OnStart(() => 
                playerObject.GetComponent<Image>().sprite = activeSprite);

            sequence.Join(moveToMiddleTween);
            sequence.Append(shake);
            sequence.Append(rotate);
            sequence.Join(scale);
            sequence.Insert(sequence.Duration() - 0.5f, shakePlayerObject);
        }
    }
}