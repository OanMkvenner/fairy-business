using UnityEngine;
using DG.Tweening;

public class FadeInOutHelper : MonoBehaviour
{
    public FadeIn fadeIn;
    public FadeOut fadeOut;

    private Sequence currentSequence;

    public enum FadeIn
    {
        PopIn = 0,
        Instant = 1,
        MoveLeftSmooth = 2,
        MoveLeftPunch = 3,
        MoveRightSmooth = 4,
        MoveRightPunch = 5,
    }
    public enum FadeOut
    {
        PopOut = 0,
        Instant = 1,
        MoveLeftSmooth = 2,
        MoveLeftPunch = 3,
        MoveRightSmooth = 4,
        MoveRightPunch = 5,
    }

    // Start is called before the first frame update
    void Start()
    {
        currentSequence = DOTween.Sequence();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void CancelCurrentSequence(){
        if (currentSequence != null)
        {
            currentSequence.Kill(true);
        }
        currentSequence = DOTween.Sequence();
    }

    public void StartFadeIn(){
        this.gameObject.SetActive(true);
        CancelCurrentSequence();
        
        switch (fadeIn)
        {
            case FadeIn.PopIn:
                var popinEffect = this.transform.DOScale(new Vector3(0,0,0), 0.7f)
                    .From(isRelative: false)
                    .SetEase(Ease.OutQuint);
                currentSequence.Append(popinEffect);
            break;
            case FadeIn.Instant:
                // do nothing, just rely on "SetActive" in the OnComplete later
            break;
            case FadeIn.MoveLeftSmooth:
                var moveL1Effect = this.transform.DOLocalMoveX(400, 0.7f)
                    .From(isRelative: false)
                    .SetEase(Ease.OutQuint);
                currentSequence.Append(moveL1Effect);
            break;
            case FadeIn.MoveLeftPunch:
                var moveL2Effect = this.transform.DOLocalMoveX(400, 0.7f)
                    .From(isRelative: false)
                    .SetEase(Ease.InQuint);
                currentSequence.Append(moveL2Effect);
            break;
            case FadeIn.MoveRightSmooth:
                var moveR1Effect = this.transform.DOLocalMoveX(-400, 0.7f)
                    .From(isRelative: false)
                    .SetEase(Ease.OutQuint);
                currentSequence.Append(moveR1Effect);
            break;
            case FadeIn.MoveRightPunch:
                var moveR2Effect = this.transform.DOLocalMoveX(-400, 0.7f)
                    .From(isRelative: false)
                    .SetEase(Ease.InQuint);
                currentSequence.Append(moveR2Effect);
            break;
            default:

            break;
        }
        currentSequence.Play();
    }
    public Sequence StartFadeOutReturnSequence(bool destroyAfter){
        CancelCurrentSequence();

        switch (fadeOut)
        {
            case FadeOut.PopOut:
                var popinEffect = this.transform.DOScale(new Vector3(0,0,0), 0.7f)
                    .SetRelative(false)
                    .SetEase(Ease.OutQuint);
                currentSequence.Append(popinEffect);
            break;
            case FadeOut.Instant:
                // do nothing, just rely on "SetActive" in the OnComplete later
            break;
            case FadeOut.MoveLeftSmooth:
                var moveL1Effect = this.transform.DOLocalMoveX(-400, 0.7f)
                    .SetRelative(false)
                    .SetEase(Ease.InQuint);
                currentSequence.Append(moveL1Effect);
            break;
            case FadeOut.MoveLeftPunch:
                var moveL2Effect = this.transform.DOLocalMoveX(-400, 0.7f)
                    .SetRelative(false)
                    .SetEase(Ease.OutSine);
                currentSequence.Append(moveL2Effect);
            break;
            case FadeOut.MoveRightSmooth:
                var moveR1Effect = this.transform.DOLocalMoveX(400, 0.7f)
                    .SetRelative(false)
                    .SetEase(Ease.InQuint);
                currentSequence.Append(moveR1Effect);
            break;
            case FadeOut.MoveRightPunch:
                var moveR2Effect = this.transform.DOLocalMoveX(400, 0.7f)
                    .SetRelative(false)
                    .SetEase(Ease.OutSine);
                currentSequence.Append(moveR2Effect);
            break;
            default:

            break;
        }

        if (destroyAfter)
        {
            currentSequence.OnComplete(() =>{
                Destroy(this.gameObject);
            });
        } else {
            currentSequence.OnComplete(() =>{
                this.gameObject.SetActive(false);
            });
        }
        return currentSequence;
    }
    
    // for button OnClick access
    public void StartFadeOut(bool destroyAfter){
        StartFadeOutReturnSequence(destroyAfter);
    }

}
