using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using System;
using System.Reflection;
using System.Linq;
using UnityEngine.UI;

// simple payload just containing a single int value
public class IntegerPayload : PayloadComponent
{
    public int value = 0;
}

// all payload classes should inherit from this class
public class PayloadComponent : MonoBehaviour
{
}

public class ButtonContainer : MonoBehaviour
{
    public List<XNode.UiStateGraph.UiStateNode> targetNodes;
    GameObject[] exampleButtons;

    // Start is called before the first frame update
    void Awake()
    {
        // TODO disable example-buttons and keep their data as reference!
        exampleButtons = GetComponentsInChildren<UnityEngine.UI.Button>().Select(x => x.gameObject).ToArray();

        foreach (var exampleButton in exampleButtons)
        {
            exampleButton.SetActive(false);
        }

        if (exampleButtons.Length <= 0)
        {
            Debug.LogWarning("ButtonContainer must contain at least one example button");
        }

    }

    // creates a new Button GameObject from one of the saved templates
    public GameObject CreateDynamicButton(int exampleButtonIndex = 0, float desiredOffset = 10.0f){
        if (exampleButtons.Length <= exampleButtonIndex)
        {
            Debug.LogError("No exampleButton available in ButtonContainer");
            return null;
        }
        GameObject exampleButton = exampleButtons[exampleButtonIndex];

        // get maximum Y position to set new button below the lowest added button.
        GameObject[] previouselyAddedButtons = GetComponentsInChildren<UnityEngine.UI.Button>().Select(x => x.gameObject).Where(x => x.activeSelf).ToArray();

        // lowestY needs to start at the highest point within ButtonContainer at the beginning. 
        //Can‘t use 0 because everything is relative to the center of the ButtonContainer for some reason...
        float lowestY = (transform as RectTransform).rect.yMax; 
        foreach (var button in previouselyAddedButtons)
        {
            RectTransform trans = button.transform as RectTransform;
            float newLowestY = trans.localPosition.y - trans.rect.yMax * trans.localScale.y;
            if (newLowestY < lowestY)
            {
                lowestY = newLowestY;
            };
        }
        float finalY = lowestY - desiredOffset - (exampleButton.transform as RectTransform).rect.yMax * exampleButton.transform.localScale.y;

        //Debug.Log("finalY = " + finalY);

        GameObject createdButton = (GameObject)Instantiate(exampleButton, parent: this.transform, instantiateInWorldSpace: false);
        // set position for newly created button (slightly below the lowest button within this container)
        Vector3 newpos = exampleButton.transform.localPosition;
        if (previouselyAddedButtons.Length > 0)
            newpos.y = finalY;
        createdButton.transform.localPosition = newpos;

        // set initial values for new button
        createdButton.name = exampleButton.name; // remove "(clone)" from buttonname
        createdButton.SetActive(true);

        UnityEngine.UI.Button buttonComponent = createdButton.GetComponent<UnityEngine.UI.Button>();
        buttonComponent.onClick.AddListener(delegate{this.ButtonClicked(createdButton);});

        return createdButton;
    }

    public void ButtonClicked(UnityEngine.GameObject button){
        UnityEngine.Object payload = button.GetComponent<PayloadComponent>();
        string portName = this.name;
        foreach (var uiStateNode in targetNodes)
        {
            if (uiStateNode != null)
            {
                uiStateNode.MoveAlongPortWithPayload(portName, payload);
            }
        }
    }

    public void ClearAllButtonsButFirst()
    {
        GameObject[] buttons = GetComponentsInChildren<UnityEngine.UI.Button>().Select(x => x.gameObject).ToArray();

        //Debug.Log("buttons = " + buttons.Length);

        for (int i = 0; i < buttons.Length; i++)
        {
            buttons[i].SetActive(false);
            Destroy(buttons[i]);
        }
    }

    /*
    public void callDefaultUiEvent(){
        callCustomUiEvent("DefaultCallback");
    }
    public void callCustomUiEvent(string payload){
        cleanNullReferences();
        UnityEvent onCalled = new UnityEvent();
        // Normalize event call string.
        // (remove all whitespaces to avoid confusion because Port names are split at Camelcase positions for readability)
        string modifiedPortname = payload;
        modifiedPortname = string.Concat(modifiedPortname.Where(c => !char.IsWhiteSpace(c)));
        modifiedPortname = char.ToUpper(modifiedPortname[0]) + modifiedPortname.Substring(1);
        payload = modifiedPortname;
        foreach (var uiStateNode in targetNodes)
        {
            //new UnityAction(uiStateNode.MoveAlongPort)
            onCalled.AddListener(delegate{uiStateNode.MoveAlongPortSupressingErrors(payload);});
        }
		Debug.Log("CustomEventSent: " + payload);
        onCalled.Invoke();
    }
    */

    void cleanNullReferences(){
        for(var i = targetNodes.Count - 1; i > -1; i--)
        {
            if (targetNodes[i] == null)
            targetNodes.RemoveAt(i);
        }
    }
    public Button GetButtonByPayload(IntegerPayload payload){
        IntegerPayload[] childButtons = GetComponentsInChildren<IntegerPayload>();
        foreach (var childButton in childButtons)
        {
            if (childButton.value == payload.value)
            {
                return childButton.GetComponent<Button>();
            }
        }
        Debug.LogError("Buttoncontainer didnt find desired payloaded button!");
        return null;
    }

}
