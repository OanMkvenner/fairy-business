using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using XNode;
using System.Linq;

namespace XNode.UiStateGraph {
	[NodeWidth(160), NodeTint(131, 61, 113)]
	public class GraphExitNode : UiStateNode {
		[Input] public AnyNode exitGraph;

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

		SubGraphNode GetParentSubgraphNode(){
			SubGraphNode subGraphNode = (graph as StateGraph).parentSubgraphNode;
			if (subGraphNode == null)
			{
				Debug.LogWarning("This Graph is not assigned to any SubGraphNode. There is no parent Graph to 'exit' to");
				return null;
			}
			return (graph as StateGraph).parentSubgraphNode;
		}

		override public void OnEnter(UiStateNode originNode, NodePort incomingPort) {
			GetParentSubgraphNode().ExitGraphAlongPort(originNode, this.name);
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
				bool renameSuccessful = stateGraph.parentSubgraphNode.UpdateSinglePortname(oldName ,this.name, false);
				if (renameSuccessful) {
					oldName = this.name;
				} else {
					this.name = oldName;
				}
			}
		}

		public override IEnumerable<NodePort> GetCorrectedOutputs() {
			SubGraphNode subGraphNode = GetParentSubgraphNode();
			if (subGraphNode != null){
				NodePort exitPort = subGraphNode.GetOutputPort(name);
				if (exitPort != null)
				{
					yield return exitPort;
				}
			}
		}

		[Serializable]
		public class AnyNode { }
	}
}