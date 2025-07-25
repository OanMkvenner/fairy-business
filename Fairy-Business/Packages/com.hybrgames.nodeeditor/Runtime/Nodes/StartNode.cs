using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using XNode;

namespace XNode.UiStateGraph {
	//[CreateNodeMenu(null)]
	[NodeWidth(120), NodeTint(130, 100, 50)]
	public class StartNode : UiStateNode {
		[Output] public AnyNode start;

		public bool checkExitConnected(){
			NodePort exitPort = GetPort("start");
			return exitPort.IsConnected;
		}

		public void MoveToFirstNode() {
			NodePort exitPort = GetPort("start");

			if (!exitPort.IsConnected) {
				return;
			}

			List<NodePort> allConnections = exitPort.GetConnections();
			foreach (var connection in allConnections)
			{
				UiStateNode node = connection.node as UiStateNode;
				node.OnEnter(node, connection);
			} 
		}

		public override object GetValue(NodePort port) {
			return this;
		}

		[Serializable]
		public class AnyNode { }
	}
}