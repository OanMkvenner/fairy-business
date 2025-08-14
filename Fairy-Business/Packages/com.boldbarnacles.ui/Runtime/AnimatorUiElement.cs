using DG.Tweening;
using UnityEngine;

public class AnimatorUiElement : MonoBehaviour {
    RectTransform tgtOverride = null;
    
    float hoverSize = 1.05f;
    public void HighlightOnHovering(){
        var tgtTrans = tgtOverride != null ? tgtOverride : GetComponent<RectTransform>();
        if (tgtTrans == null) return;

        DOTween.Kill(tgtTrans);
        tgtTrans.DOScale(new Vector3(hoverSize, hoverSize, 1.0f), 0.1f).SetRelative(false).SetEase(Ease.OutExpo).SetLink(tgtTrans.gameObject);
    }
    public void StopHighlightOnHovering(){
        var tgtTrans = tgtOverride != null ? tgtOverride : GetComponent<RectTransform>();
        if (tgtTrans == null) return;

        DOTween.Kill(tgtTrans);
        tgtTrans.DOScale(new Vector3(1.0f, 1.0f, 1.0f), 0.17f).SetRelative(false).SetEase(Ease.OutSine).SetLink(tgtTrans.gameObject);
    }
}