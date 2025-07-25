using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using XNode;
using System.Linq;
using UnityEditor;

namespace XNode.UiStateGraph {
	[NodeWidth(230), NodeTint(160, 140, 50)]
	public class SubGraphNode : UiStateNode {
    	public StateGraph subGraph;

		[HideInInspector]
		public StateGraph oldSubGraph = null;


		public void ExitGraphAlongPort(UiStateNode originNode, string exitName) {
			NodePort exitPort = GetOutputPort(exitName);
			if (exitPort == null)
			{
				Debug.LogError("Cant find SubGraph exit-port of Name:" + exitName);
				return;
			}
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

		public bool SetSubGraph(StateGraph targetGraph){
			if ((graph as StateGraph).RegisterNewSubgraphNode(this, targetGraph)){
				subGraph = targetGraph;
				oldSubGraph = targetGraph;
				TryUpdateSubgraphPorts();
				return true;
			} else {
				subGraph = oldSubGraph;
				//serialized vairable changed, show change
				(graph as StateGraph).QueueRepaint();
				return false;
			};
		}

		protected override void Init(){

		}
		
		public void TryUpdateSubgraphPorts(){
			if (subGraph != null)
			{
				UpdateInOutPortnames();
			}
		}
		public bool UpdateSinglePortname(string prevName, string newName, bool isInputPort)
		{
			if (prevName == newName)
				return false;

			foreach (NodePort port in Ports) { 
				if (port.fieldName == newName) {
					Debug.LogWarning("This Name is already used on another Entry/ExitNode within this Graph");
					return false;
				}
			};

			IEnumerable<NodePort> checkPorts;
			NodePort newPort;
			if (isInputPort)
			{
				checkPorts = Inputs;
				newPort = AddDynamicInput(typeof(EnterNode), fieldName: newName);

			} else {
				checkPorts = Outputs;
				newPort = AddDynamicOutput(typeof(ButtonNode), fieldName: newName);
			}

			NodePort portToChange = null;
			foreach (NodePort port in checkPorts) { 
				if (port.fieldName == prevName) {
					portToChange = port;
					break;
				}
			};
			if (newPort == null || portToChange == null)
			{
				Debug.LogWarning("Could not reconnect SubGraphNode port. Please check connections.");
				UpdateInOutPortnames();
				return true;
			}
			// move all current connections over to the new port
			{
				//portToChange.MoveConnections(newPort); // sadly this function is currently broken in XNode
				// so i re-implemented it manually to avoid self-made XNode-Plugin changes
				List<NodePort> connections = portToChange.GetConnections();
				int connectionCount = connections.Count;
				for (int i = 0; i < connectionCount; i++) {
					NodePort otherPort = connections[i];
					newPort.Connect(otherPort);
				}
				portToChange.ClearConnections();
			}

			// remove previous port
			RemoveDynamicPort(portToChange);
			return true;
		}
		public void UpdateInOutPortnames()
		{
			// cant update if no subgraph is connected
			if (subGraph == null)
				return;

			bool changedAtLeastOnePort = false;
			foreach (GraphEntryNode node in subGraph.nodes.Where(node => node.GetType() == typeof(GraphEntryNode)))
			{
				bool alreadyExists = false;
				foreach (NodePort port in Inputs) { 
					if (port.fieldName == node.name) {
						alreadyExists = true;
						break;
					}
				};
				if (!alreadyExists){
					AddDynamicInput(typeof(EnterNode), fieldName: node.name);
					changedAtLeastOnePort = true;
				}
			};
			foreach (GraphExitNode node in subGraph.nodes.Where(node => node.GetType() == typeof(GraphExitNode)))
			{
				bool alreadyExists = false;
				foreach (NodePort port in Outputs) { 
					if (port.fieldName == node.name) {
						alreadyExists = true;
						break;
					}
				};
				if (!alreadyExists){
					AddDynamicOutput(typeof(ButtonNode), fieldName: node.name);
					changedAtLeastOnePort = true;
				}
			};
			var dynamicPorts = DynamicPorts.ToArray();
			foreach (NodePort port in dynamicPorts) { 
				bool stillExists = false;
				foreach (var node in subGraph.nodes.Where(node => node.GetType() == typeof(GraphEntryNode) || node.GetType() == typeof(GraphExitNode)))
				{
					if (port.fieldName == node.name) {
						stillExists = true;
						break;
					}
				}
				if (!stillExists)
				{
					RemoveDynamicPort(port);
					changedAtLeastOnePort = true;
				}
			};
			if (changedAtLeastOnePort)
			{
				(graph as StateGraph).QueueRepaint();
			}
		}

		public override Node GetCorrectedNode(NodePort port) {
			if (subGraph == null) return null;
			if (port.IsInput)
			{
				return subGraph.SearchNodeOfNameAndType(port.fieldName, typeof(GraphEntryNode));
			} else {
				return subGraph.SearchNodeOfNameAndType(port.fieldName, typeof(GraphExitNode));
			}
		}

		override public void OnEnter(UiStateNode originNode, NodePort incomingPort) {
			GraphEntryNode enterNode = subGraph.SearchNodeOfNameAndType(incomingPort.fieldName, typeof(GraphEntryNode)) as GraphEntryNode;
			if (enterNode != null)
			{
				enterNode.MoveToFirstNode(originNode);
			}
		}
	}
}