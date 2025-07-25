using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using XNode;

namespace XNode.UiStateGraph {
	public class UiStateNode : Node {
		[Input] public EnterNode enter;
		[Output] public ImmideateExitPort passThrough;

		protected UnityEngine.Object payload = null;

		// anytime a graph enters a new node, it remembers the last active view. This active node
		// now "belongs" to that view and gets deactivated as soon as the view gets destroyed. ALSO 
		// every time a graph enters a new ViewNode all currently remembered views get destroyed by default.
		public List<Node> registeredOriginViews { get; private set; }  = new List<Node>();

		[HideInInspector] public bool active = false;

		protected NodePort enterPort; // just the incoming port for the property "enter"

        protected override void Init()
        {
			base.Init();
			enterPort = FindInputOfName("enter");
		}
		virtual public void OnEnter(UiStateNode originNode, NodePort incomingPort) {
			// if this isnt a ViewNode, keep references to originating ViewNodes
			if(!(this is ViewNode)){
				foreach (var item in originNode.registeredOriginViews)
				{
					if (!registeredOriginViews.Contains(item))
						registeredOriginViews.Add(item);
				}
				// if originView itself is a ViewNode then keep reference to that as well
				ViewNode originView = originNode as ViewNode;
				if (originView && originView != this) {
					if (!registeredOriginViews.Contains(originNode))
						registeredOriginViews.Add(originNode);
				}
			}
			// transfer payload of previous node to new node, no matter its content.
			payload = originNode.GetPayload();
			// activate this node
			active = true;
			(graph as StateGraph).QueueRepaint();
			// move on immideately if possible
			ContinueImmideatelyIfPossible();
		}

		// Is called whenever you move through a Port to another Node.
		// Usually just calls OnDeactivate but can be overridden to avoid 
		// that call or do something else when leaving the node
		virtual public void OnLeave() {
			OnDeactivate();
		}

		virtual public void OnDeactivate() {
			if (!active) { 
				return; // dont need to leave, its not active at the moment
			}
			registeredOriginViews.Clear();
			active = false;
			(graph as StateGraph).QueueRepaint();
		}

		public void ContinueImmideatelyIfPossible() {
			MoveAlongPortSafely("passThrough", false);
			// DONT call OnLeave() automatically when continuing through PassThrough ports!
		}
		public void MoveAlongPort(string exitPortName) {
			if (MoveAlongPortSafely(exitPortName, true)){
				OnLeave();
			};
		}
		public void MoveAlongPortSupressingErrors(string exitPortName, bool ignoreActiveState = false) {
			if (MoveAlongPortSafely(exitPortName, reportErrors: false, ignoreActiveState: ignoreActiveState)){
				OnLeave();
			};
		}

		private bool MoveAlongPortSafely(string exitPortName, bool reportErrors = true, bool ignoreActiveState = false) {
			NodePort exitPort = FindOutputOfName(exitPortName);
			if (exitPort == null){
				if (reportErrors)
					Debug.LogWarning("Node: " + name + " - Port of name '" + exitPortName + "' not found");
				return false;
			}

			if (!ignoreActiveState && !active) {
				// for now lets always report this warning to find potential buggy behaviour
				Debug.LogWarning("Node: " + name + " - Node isn't active");
				return false;
			}

			if (!exitPort.IsConnected) {
				if (reportErrors)
					//Debug.LogWarning("Node: " + name + " - Node isn't connected");
				return false;
			}

			List<NodePort> allConnections = exitPort.GetConnections();
			foreach (var connection in allConnections)
			{
				UiStateNode node = connection.node as UiStateNode;
				node.OnEnter(this, connection);
			}
			return true;
		}

		public NodePort FindOutputOfName(string portName){
			foreach (NodePort port in Outputs) { 
				if (port.fieldName == portName) 
					return port; 
			};
			return null;
		}
		public NodePort FindInputOfName(string portName){
			foreach (NodePort port in Inputs) { 
				if (port.fieldName == portName) 
					return port; 
			};
			return null;
		}

		public void RemoveDynamicPorts(string name) {  
			RemoveDynamicPort(name);
		}

		// this function is called every time a node tries to access a value of one of its inputs.
		// depending on the type of port requested (as seen by its port.fieldName == "portname")
		// you should process the request and answer with an appropriate typed answer.
		//public override object GetValue(NodePort port) {
			/*
			// Get new a and b values from input connections. Fallback to field values if input is not connected
			float a = GetInputValue<float>("a", this.a);
			float b = GetInputValue<float>("b", this.b);

			// After you've gotten your input values, you can perform your calculations and return a value
			if (port.fieldName == "result")
				switch(mathType) {
					case MathType.Add: default: return a + b;
					case MathType.Subtract: return a - b;
					case MathType.Multiply: return a * b;
					case MathType.Divide: return a / b;
				}
			else if (port.fieldName == "sum") return a + b;
			else return 0f;
			*/
			
		//	return null; // might return "this" here on ViewNodes, might refer to partent node on other node types
		//}
		
		// returns whatever is currently saved in "payload". Could be overwritten if a node does it differently.
		public UnityEngine.Object GetPayload(){
			return payload;
		}
		// returns whatever is currently saved in "payload" and sets the current payload to "null"
		public UnityEngine.Object ConsumePayload(){
			var payloadReturn = payload;
			payload = null;
			return payloadReturn;
		}
		public void MoveAlongPortWithPayload(string portName,UnityEngine.Object i_payload){
			payload = i_payload;
			MoveAlongPort(portName);
		}

		public override object GetValue(NodePort port) {
			return this;
		}

		// this is used to find the real next Node. Some nodes might just be re-route points and are "jumped" directly without resembling a proper node.
		// E.g. when searching along a SubgGraphNodes, dont trat that SubGraphNode as a node but instead jump directly to the respective GraphEntryNode
		public virtual Node GetCorrectedNode(NodePort port) {
			return this;
		}
		// this is used to find the possible routes downstream. Override it if specific nodes get their outputs in a different way
		// E.g. route through SubgGraphNodes properly
		public virtual IEnumerable<NodePort> GetCorrectedOutputs() {
			return this.Outputs;
		}

		[Serializable]
		public class EnterNode { }
		[Serializable]
		public class ImmideateExitPort { }
		[Serializable]
		public class ButtonNode { }
		[Serializable]
		public class TargetNode { }
/*
		public class EnterNode { }
		[Serializable]
		public class ImmideateNode { }
		[Serializable]
		public class ButtonNode { }
		*/
	}
}