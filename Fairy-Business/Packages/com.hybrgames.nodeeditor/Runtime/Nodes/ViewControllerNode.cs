using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using XNode;

namespace XNode.UiStateGraph {
	[NodeWidth(160), NodeTint(131, 61, 113)]
	public class ViewControllerNode : UiStateNode {

		[Output] public ButtonNode viewTargetOverride;

		[HideInInspector]
		public ViewControllerMode controllerMode;

		public enum ViewControllerMode
		{
			DeactivateViews,
			KeepViewsAlive,
			DisableViewInteractions,
			EnableViewInteractions,
			ContinueIfInteractionsEnabled,
			//DeactivateSpecificView, //Not yet implemented. WARNING! ViewNodes "StopAllConnectedNodes()" stops 
									  //searching at ViewControllerNode's. If we implement this it might have to
									  //continue if the option is set to this OR we implement another node instead!! (better option)
		}

		override public void OnEnter(UiStateNode originNode, NodePort incomingPort) {
			NodePort viewtargetOverridePort = FindOutputOfName("viewTargetOverride");
			var targetViews = new List<ViewNode>();
			foreach (var item in originNode.registeredOriginViews)
			{
				targetViews.Add(item as ViewNode);
			}
			// if viewtargetOverridePort is connected, do all things on DIRECTLY connected views instead (path tracing could be added later..)
			List<NodePort> allOverrideConnections = new();
			bool targetOverrideActive = false;
			if (viewtargetOverridePort.IsConnected){
				targetOverrideActive = true;
				targetViews = new();
				allOverrideConnections = viewtargetOverridePort.GetConnections();
				foreach (var connection in allOverrideConnections)
				{
					ViewNode viewNode = connection.node as ViewNode;
					if (viewNode && viewNode != this){
						targetViews.Add(viewNode);
					} else {
						Debug.LogError("ViewControllerMode seems to have a non ViewNode connected to its viewTargetOverride. This is not supported currently!");
					}
				}
			}

			if (controllerMode == ViewControllerMode.DeactivateViews)
			{
				foreach (var viewNode in targetViews){
					viewNode.OnDeactivate();
				}
				if (!targetOverrideActive){
					// deactivate originNode directly, if it is a View itself
					ViewNode originView = originNode as ViewNode;
					if (originView && originView != this) {
						originView.OnDeactivate();
					}
				}

				base.OnEnter(originNode, incomingPort);
			}
			else if (controllerMode == ViewControllerMode.KeepViewsAlive) 
			{
				// cut view propagation by using self as new origin node.
				// but before doing so, we still need to propagate payloads anyway!
				payload = originNode.GetPayload();
				base.OnEnter(this, this.enterPort);
			}
			else if (controllerMode == ViewControllerMode.DisableViewInteractions) 
			{
				// deactivate all Views gathered in origins registeredOriginViews
				foreach (var viewNode in targetViews)
				{
					viewNode.DisableInteractions();
				}
				base.OnEnter(originNode, incomingPort);
			}
			else if (controllerMode == ViewControllerMode.EnableViewInteractions) 
			{
				
				// deactivate all Views gathered in origins registeredOriginViews
				foreach (var viewNode in targetViews)
				{
					viewNode.EnableInteractions();
				}
				base.OnEnter(originNode, incomingPort);
			}
			else if (controllerMode == ViewControllerMode.ContinueIfInteractionsEnabled) 
			{
				// only continue if at least one parent (or override) View is enabled
				bool atLeastOneViewEnabled = false;
				foreach (var viewNode in targetViews)
				{
					if (viewNode.CheckInteractionsEnabled()){
						atLeastOneViewEnabled = true;
					};
				}
				if (atLeastOneViewEnabled)
				{
					base.OnEnter(originNode, incomingPort);
				}
			} else {
				base.OnEnter(originNode, incomingPort);
			}

			// deactivate the node immideately after it has done its work
			OnDeactivate();
		}
		
	}
}