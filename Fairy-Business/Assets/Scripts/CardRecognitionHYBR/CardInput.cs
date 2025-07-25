using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;

public class CardInput : MonoBehaviour {
    ScanResult scanInputOverride = null;

    ScanResult currentlyEvaluatingResult = null;
    ScanResult lastScanResult = null;
    public Image ScanProgressImage1;
    public Image ScanProgressImage2;
    
    public bool blockScanning = false;
    public float timeUntilSameCardIsAllowedAgain = 0; // set in inspector
    public float timeUntilEvaluationReset = 0.4f;
    public float timeUntilEvaluationAccepted = 1.0f;

    public UnityEvent<ScanResult> onStartEvaluation = new UnityEvent<ScanResult>();
    public UnityEvent<ScanResult> onUpdateCardEvaluation = new UnityEvent<ScanResult>();
    public UnityEvent<ScanResult> onAcceptCardEvaluation = new UnityEvent<ScanResult>();
    public UnityEvent onCancelCurrentEvaluation = new UnityEvent();
    public UnityEvent onResetScanProgressVisuals = new UnityEvent();
    public UnityEvent<float> onStartScanProgressVisuals = new UnityEvent<float>();

    void Update() {
        CheckNewCardReceived();     
    }

    //Sequence scanProgressSequence = null;
    public void StartScanProgressVisuals(float expectedScanDuration){
        //scanProgressSequence = DOTween.Sequence();
        //scanProgressSequence
        //    .Append(ScanProgressImage1.DOFillAmount(0.5f, expectedScanDuration))
        //    .Join(ScanProgressImage2.DOFillAmount(0.5f, expectedScanDuration));
    }
    public void ResetScanProgressVisuals(){
        onResetScanProgressVisuals.Invoke();
        //if (scanProgressSequence != null && scanProgressSequence.IsActive()){
        //    scanProgressSequence.Kill();
        //}
        //ScanProgressImage1.fillAmount = 0;
        //ScanProgressImage2.fillAmount = 0;
    }

    public void CancelCurrentEvaluation(){
        ResetScanProgressVisuals();
        //ShowRedFlash();
        onCancelCurrentEvaluation.Invoke();
    }
    public void StartEvaluation(ScanResult scanResult, float expectedScanDuration){
        ResetScanProgressVisuals();
        onStartEvaluation.Invoke(scanResult);
        onStartScanProgressVisuals.Invoke(expectedScanDuration);
    }
    public void UpdateCardEvaluation(ScanResult scanResult, float percentDone){
        //ScanProgressImage.fillAmount = percentDone;
    }
    public void AcceptCardEvaluation(ScanResult scanResult){
        ResetScanProgressVisuals();
        //ShowWhiteFlash();
        onAcceptCardEvaluation.Invoke(scanResult);
    }

    public void PreprocessResult(ref ScanResult result){
        // possible preprocessing of the result
    }

    float evaluationStart = float.PositiveInfinity;
    float evaluationSuccessTime = float.PositiveInfinity;
    float evaluationResetTime = float.PositiveInfinity;
    float timeToAllowSameCardAgain = 0;
    public void CheckNewCardReceived(){
        ScanResult newScanResult;
        // check Scan override
        if (scanInputOverride != null)
        {
            newScanResult = scanInputOverride;
        } else {
            newScanResult = CameraOpencvLib.GetNewScanResult();
        }
        // check scan effects
        if (newScanResult != null)
        {
            PreprocessResult(ref newScanResult);
            if (blockScanning)
            {
                Debug.LogWarning("scanning found card but is blocked");
                return;
            }
            // not currently evaluating, or currently evaluating different card than scanned
            if (currentlyEvaluatingResult == null || (currentlyEvaluatingResult != null && currentlyEvaluatingResult.name != newScanResult.name))
            {
                // cancel current evaluation target, if applicable
                if (currentlyEvaluatingResult != null)
                {
                    evaluationResetTime = float.PositiveInfinity;
                    currentlyEvaluatingResult = null;
                    CancelCurrentEvaluation();
                }
                // check if this card was scanned recently, and if enough time has passed since then.
                if (lastScanResult != null && newScanResult.name == lastScanResult.name)
                {
                    if (timeToAllowSameCardAgain > Time.time)
                    {
                        // same card recently scanned, ignore scan
                        return;
                    }
                }
                evaluationResetTime = Time.time + timeUntilEvaluationReset;
                evaluationSuccessTime = Time.time + timeUntilEvaluationAccepted; // time until evaluation is finished
                evaluationStart = Time.time;
                currentlyEvaluatingResult = newScanResult;
                float duration = (evaluationSuccessTime - evaluationStart);
                StartEvaluation(currentlyEvaluatingResult, duration);
            }
            // continue current evaluation
            else if (currentlyEvaluatingResult != null && currentlyEvaluatingResult.name == newScanResult.name)
            {
                evaluationResetTime = Time.time + timeUntilEvaluationReset;
                currentlyEvaluatingResult = newScanResult; // update result stats
                float percentDone = (Time.time - evaluationStart) / (evaluationSuccessTime - evaluationStart);
                UpdateCardEvaluation(currentlyEvaluatingResult, percentDone);

                if (evaluationSuccessTime < Time.time)
                {
                    AcceptCardEvaluation(currentlyEvaluatingResult);

                    // remember last scanned card
                    lastScanResult = currentlyEvaluatingResult;
                    timeToAllowSameCardAgain = Time.time + timeUntilSameCardIsAllowedAgain; // time until same card is allowed again
                    // reset current evaluation
                    evaluationStart = float.PositiveInfinity;
                    evaluationSuccessTime = float.PositiveInfinity;
                    evaluationResetTime = float.PositiveInfinity;
                    currentlyEvaluatingResult = null;
                }
            } else {
                Debug.LogError("forgot a case?");
            }
            
        } else {
            // too much time passed since last successful scan, cancel evaluation
            if (evaluationResetTime < Time.time)
            {
                evaluationResetTime = float.PositiveInfinity;
                currentlyEvaluatingResult = null;
                CancelCurrentEvaluation();
            }
        }
    }
    private void ResetBlockLastCardTimer()
    {
        timeToAllowSameCardAgain = Time.time;
    }

}