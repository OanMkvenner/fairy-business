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

        [SerializeField] private Button debugButton;

        private Sequence sequence;
        
        private void Awake()
        {
            GameSession.OnCardScanned += AnimateScannedCard;
            debugButton.onClick.AddListener(()=> AnimateScannedCard(PlayerColor.Blue, ScanAction.CreditCard));
        }

        private void OnDestroy()
        {
            GameSession.OnCardScanned -= AnimateScannedCard;
            debugButton.onClick.RemoveAllListeners();
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

            Vector3 newScale = new Vector3(playerObject.transform.localScale.x, playerObject.transform.localScale.y,
                playerObject.transform.localScale.z);
            
            Tween fly = scanObject.transform.DOMove(playerObject.transform.position, 0.9f);
            Tween rotate = scanObject.transform.DORotate(playerObject.transform.eulerAngles, 0.9f);
            
            Tween scaleDown = scanObject.transform.DOScale(newScale, 0.9f);

            sequence.Join(moveToMiddleTween);
            sequence.Append(shake);
            sequence.Append(fly);
            sequence.Join(rotate);
            sequence.Join(scaleDown);
        }
    }
}