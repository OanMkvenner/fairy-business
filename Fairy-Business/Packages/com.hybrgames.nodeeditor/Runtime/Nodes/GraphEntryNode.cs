using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using XNode;
using System.Linq;

namespace XNode.UiStateGraph {
	[NodeWidth(160), NodeTint(131, 61, 113)]
	public class GraphEntryNode : UiStateNode {
		[Output] public AnyNode externalEntry;

		public string oldName = "";

		public void InitOnCreate(){
			oldName = this.name;
			SubGraphNode subGraphNode = (graph as StateGraph).parentSubgraphNode;
			if (subGraphNode == null)
				return;
			// update portnames because we have a new Entry/Exit Port
			subGraphNode.TryUpdateSubgraphPorts();
		}

		private void OnDestroy() {
			SubGraphNode subGraphNode = (graph as StateGraph).parentSubgraphNode;
			if (subGraphNode == null)
				return;
			subGraphNode.TryUpdateSubgraphPorts();
		}

		public void MoveToFirstNode(UiStateNode originNode) {
			NodePort exitPort = GetPort("externalEntry");

			if (!exitPort.IsConnected) {
				Debug.LogWarning("Node isn't connected");
				return;
			}

			List<NodePort> allConnections = exitPort.GetConnections();
			foreach (var connection in allConnections)
			{
				UiStateNode node = connection.node as UiStateNode;
				node.OnEnter(originNode, connection);
			} 
			(graph as StateGraph).QueueRepaint();
		}

		public void RenamedNode(){
			StateGraph stateGraph = (graph as StateGraph);
			Node nodeWithSameName = stateGraph.nodes.Where(node => (node is GraphExitNode ||  node is GraphEntryNode) && node != this && node.name == this.name)
													.FirstOrDefault();
			if (nodeWithSameName != null) {
				if (nodeWithSameName is GraphExitNode){
					Debug.LogWarning("This Name is already used on another ExitNode within this Graph");
				} else {
					Debug.LogWarning("This Name is already used on another EntryNode within this Graph");
				}
				this.name = oldName;
				return;
			}
			if (stateGraph.parentSubgraphNode != null) {
				bool renameSuccessful = stateGraph.parentSubgraphNode.UpdateSinglePortname(oldName ,this.name, true);
				if (renameSuccessful) {
					oldName = this.name;
				} else {
					this.name = oldName;
				}
			}
		}

		public override object GetValue(NodePort port) {
			return this;
		}

		[Serializable]
		public class AnyNode { }
	}
}