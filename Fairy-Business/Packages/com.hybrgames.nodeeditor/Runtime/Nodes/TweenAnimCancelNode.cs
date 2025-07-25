using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Reflection;

using System.Linq;
using System;
using DG.Tweening;

namespace XNode.UiStateGraph {

	[NodeWidth(146), NodeTint(255, 80, 50)]
	public class TweenAnimCancelNode : UiStateNode {
		[Output] public TargetNode targetAnims;

		override protected void Init(){
		}

		override public void OnEnter(UiStateNode originNode, NodePort incomingPort) {
			base.OnEnter(originNode, incomingPort);
			
			TweenAnimCancelNode.CancelAllConnectedTweenAnimationNodes(currentNode: this, animationTargetsPort: "targetAnims");
			this.OnDeactivate();
		}

		static public void CancelAllConnectedTweenAnimationNodes(UiStateNode currentNode, string animationTargetsPort){
			// cancel all currently running animations that are connected to the "targetAnims" port
			NodePort exitPort = currentNode.GetOutputPort(animationTargetsPort);
			if (!exitPort.IsConnected) {
				return;
			}
			List<NodePort> allConnections = exitPort.GetConnections();
			foreach (var connection in allConnections)
			{
				TweenAnimationNode node = connection.node as TweenAnimationNode;
				if (node != null)
				{
					// cancel animations WITHOUT moving along the "onFinished" Port on them
					node.OnDeactivate();
				}
			} 
		}
		
		override public void OnLeave(){
			base.OnDeactivate();
		}

		override public void OnDeactivate(){
			base.OnDeactivate();
		}

	}
}