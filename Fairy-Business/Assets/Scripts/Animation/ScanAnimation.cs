using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

namespace Animation
{
    public class ScanAnimation : MonoBehaviour
    {
        [SerializeField] private Button _button;
        [SerializeField] private Transform startPosition;
        [SerializeField] private Sprite activeSprite;
        [SerializeField] private Sprite inactiveSprite;
        [SerializeField] private GameObject playerObject;
        [SerializeField] private GameObject scanObject;

        private Sequence sequence;

        private void Awake()
        {
            _button.onClick.AddListener(AnimateScannedCard);
        }

        private void OnDestroy()
        {
            _button.onClick.RemoveAllListeners();
        }

        private void AnimateScannedCard()
        {
            sequence.Kill();
            
            scanObject.transform.position = startPosition.position;
            scanObject.transform.localScale = new Vector3(1, 1, 1);
            int randAngle = Random.Range(0, 360);
            scanObject.GetComponent<RectTransform>().rotation = Quaternion.Euler(0, 0, randAngle);
            scanObject.SetActive(true);
            playerObject.GetComponent<Image>().sprite = inactiveSprite;
            
            sequence = DOTween.Sequence();
            
            Vector2 center = new Vector2(Screen.width / 2f, Screen.height / 2f);

            Tween moveToMiddleTween = scanObject.transform.DOMoveY(center.y, 0.5f).SetEase(Ease.OutExpo);
            
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