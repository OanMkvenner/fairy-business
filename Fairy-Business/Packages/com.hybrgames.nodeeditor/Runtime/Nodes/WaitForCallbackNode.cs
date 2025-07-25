using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Reflection;

using UnityEditor;
using System.Linq;
using System;
using UnityEngine.Events;

namespace XNode.UiStateGraph {
	[NodeTint(131, 40, 40)]
	public class WaitForCallbackNode : UiStateNode {

		public string newPort = "NewPortName";

		// this Node's functionality is mostly implemented during drag'n'drop operations in the StateGraph
        protected override void Init()
        {
            base.Init();
			newPort = "NewPortName";
        }

		public List<string> GetAllCallbackPortNames(){
			List<string> callbackPortNames = new();
			foreach (NodePort port in Outputs) { 
				if (port.fieldName != "PassThrough") {
					callbackPortNames.Add(port.fieldName);
				} else {
					Debug.LogError("test PassthroughPort is actually a port with nam,e, good to know... delete this message");
				}
			};
			return callbackPortNames;
		}
		
		public void callCustomUiEvent(string payload, bool onlyCallIfNodeIsActive = false, bool executeImmideately = false){
			UnityEvent onCalled = new UnityEvent();
			// Normalize event call string.
			// (remove all whitespaces to avoid confusion because Port names are split at Camelcase positions for readability)
			string modifiedPortname = payload;
			modifiedPortname = string.Concat(modifiedPortname.Where(c => !char.IsWhiteSpace(c)));
			modifiedPortname = char.ToUpper(modifiedPortname[0]) + modifiedPortname.Substring(1);
			payload = modifiedPortname;

			//new UnityAction(uiStateNode.MoveAlongPort)
			if (executeImmideately) {
				this.MoveAlongPortSupressingErrors(payload, ignoreActiveState: !onlyCallIfNodeIsActive);
			} else {
				onCalled.AddListener(delegate{this.MoveAlongPortSupressingErrors(payload, ignoreActiveState: !onlyCallIfNodeIsActive);});
			}
			Debug.Log("CustomEventSent: " + payload);
			
			if (!executeImmideately) {
				onCalled.Invoke();
			}
		}
		public void callCustomUiEventImmideately(string payload, bool onlyCallIfNodeIsActive = false){
			callCustomUiEvent(payload, onlyCallIfNodeIsActive, true);
		}
    
	}
}