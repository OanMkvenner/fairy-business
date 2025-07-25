using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using TMPro;

public class FlipButton : MonoBehaviour
{
    public enum ActiveSide
    {
        undefined = -1,
        front = 0,
        back = 1,
    }
    public GameObject FrontContent;
    public GameObject BackContent;
    public Image FrontImage;
    public Image BackImage;
    public TMP_Text FrontText;
    public TMP_Text BackText;
    public ActiveSide activeSide = ActiveSide.front;
    public Ease easingFunction;
    
    ActiveSide visibleSide = ActiveSide.front;
    [SerializeField]
    Tweener currentTween;
    // Start is called before the first frame update
    void Start()
    {
        Invoke("LateStart", 0f);
        GetComponent<Button>().onClick.AddListener(ButtonClicked);
    }

    void LateStart(){
        SetSideInstant(activeSide);
        // done in late start to allow translation before disabling
        UpdateVisibleContentByRotation(true);
    }

    // Update is called once per frame
    void Update()
    {
        UpdateVisibleContentByRotation();
    }

    void UpdateVisibleContentByRotation(bool initalUpdate = false){
        Vector3 frontFacingVector = transform.rotation * Vector3.forward;
        if (frontFacingVector.z < 0 && (visibleSide == ActiveSide.front || initalUpdate))
        {
            visibleSide = ActiveSide.back;
            if (initalUpdate)
                activeSide = ActiveSide.back;
            FrontContent.SetActive(false);
            BackContent.SetActive(true);
        } else if (frontFacingVector.z > 0 && (visibleSide == ActiveSide.back || initalUpdate)){
            visibleSide = ActiveSide.front;
            if (initalUpdate)
                activeSide = ActiveSide.front;
            FrontContent.SetActive(true);
            BackContent.SetActive(false);
        }
    }

    public void SetSideInstant(ActiveSide side){
        // set active side
        activeSide = side;
        //stop old tween
        if (currentTween != null)
        {
            if (currentTween.IsActive())
            {
                currentTween.Kill();
            }
        }
        // set rotation instant
        transform.rotation = Quaternion.AngleAxis(activeSide == ActiveSide.front ? 0 : 180, Vector3.right);
    }

    // neeed abstraction for "AddListener(ButtonClicked)" above
    public void ButtonClicked(){
        SetSideWithAnim();
    }
    public void SetSideWithAnim(ActiveSide desiredSide = ActiveSide.undefined){
        if (desiredSide == ActiveSide.undefined)
        {
            if (activeSide == ActiveSide.front)
            {
                activeSide = ActiveSide.back;
            } else {
                activeSide = ActiveSide.front;
            }
        } else {
            activeSide = desiredSide;
        }

        // clear previous tween
        if (currentTween != null)
        {
            if (currentTween.IsActive())
            {
                currentTween.Kill();
            }
        }
        float targetRotation = activeSide == ActiveSide.front ? 0 : 180;
        
        currentTween = transform.DORotateQuaternion(Quaternion.AngleAxis(targetRotation, Vector3.right), 1.0f)
            .SetRelative(false)
            .SetEase(easingFunction);
    }
}
