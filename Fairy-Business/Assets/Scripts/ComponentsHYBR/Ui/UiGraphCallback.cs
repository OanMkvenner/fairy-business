using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using System;
using System.Reflection;
using System.Linq;

public class UiGraphCallback : MonoBehaviour
{
    public List<XNode.UiStateGraph.UiStateNode> targetNodes;

    //public void callDefaultUiEvent(){
    //    callCustomUiEvent("DefaultCallback");
    //}
    public void callCustomUiEvent(string payload, bool onlyCallIfNodeIsActive = false, bool executeImmideately = false){
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
            if (executeImmideately) {
                uiStateNode.MoveAlongPortSupressingErrors(payload, ignoreActiveState: !onlyCallIfNodeIsActive);
            } else {
                onCalled.AddListener(delegate{uiStateNode.MoveAlongPortSupressingErrors(payload, ignoreActiveState: !onlyCallIfNodeIsActive);});
            }
        }
		Debug.Log("CustomEventSent: " + payload);
        
        if (!executeImmideately) {
            onCalled.Invoke();
        }
    }
    public void callCustomUiEventImmideately(string payload, bool onlyCallIfNodeIsActive = false){
        callCustomUiEvent(payload, onlyCallIfNodeIsActive, true);
    }

    void cleanNullReferences(){
        for(var i = targetNodes.Count - 1; i > -1; i--)
        {
            if (targetNodes[i] == null)
            targetNodes.RemoveAt(i);
        }
    }

}
