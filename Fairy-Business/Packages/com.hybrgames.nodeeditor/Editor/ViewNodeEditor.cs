using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using XNode.UiStateGraph;
using XNode;
using XNodeEditor;
using UnityEditor.Events;
using UnityEngine.Events;

namespace XNodeEditor.UiStateGraph {
	[CustomNodeEditor(typeof(ViewNode))]
	public class ViewNodeEditor : NodeEditor {
		private ViewNode node;
		StateGraph graph;
        public override void OnCreate()
        {
			// Initialization
			if (node == null) {
				node = target as ViewNode;
				graph = node.graph as StateGraph;
			}
            base.OnCreate();
        }
		public override void OnHeaderGUI() {
			GUI.color = Color.white;
			if (node.assignedCanvasRef.gameObject != null)
				GUI.color = Color.green;
			else 
				GUI.color = Color.red;
			if (node.active) GUI.color = Color.blue;
			string title = target.name;
			if (node.assignedCanvasRef.gameObject == null)
				title = "DRAG AND DROP VIEW HERE";
			GUILayout.Label(title, NodeEditorResources.styles.nodeHeader, GUILayout.Height(30));
			GUI.color = Color.white;
			//Rename
			//name = "tset"

/*
			if(!Application.isPlaying && statenode.drb != null) {
				
				Debug.LogWarning("TAT2A");// NEED A SEPERATE EDITOR FOR STARTNODE or make a root-Node wich all inherit from!
				UnityEngine.UI.Button btn = statenode.drb;
				statenode.drb = null;
				// add new port
				// TODO this specific code would have to happen later, or at least the "adding a port" portion of it. Doing it here
				// results in a thrown error in console but it works for now anyway.
				string newPortName = btn.name;
				statenode.AddDynamicPorts(newPortName);

				// iterate backwards through listeners and remove empty events
				int _previousListenerCount = btn.onClick.GetPersistentEventCount();
				for (int listener_idx = _previousListenerCount - 1; listener_idx >= 0 ; listener_idx--)
				{
					// remove empty dangling listeners (can happen when removing nodes)
					if (btn.onClick.GetPersistentTarget(listener_idx) == null)
					{
						UnityEventTools.RemovePersistentListener(btn.onClick, listener_idx);
					}
					// remove listeners that point to this node already
					else if (btn.onClick.GetPersistentTarget(listener_idx) == statenode)
					{
						statenode.removeButtonReferenceOnly(btn);
						UnityEventTools.RemovePersistentListener(btn.onClick, listener_idx);
					}
					// remove listeners to other nodes (could be merged with above but 
					// lets keep it seperate for now until we know we dont need it to be seperate)
					else if (btn.onClick.GetPersistentMethodName(listener_idx) == "MoveAlongPort")
					{
						StateNode _targetNode = btn.onClick.GetPersistentTarget(listener_idx) as StateNode;
						_targetNode.removeButtonReferenceOnly(btn);
						UnityEventTools.RemovePersistentListener(btn.onClick, listener_idx);
					}
				}
				// add new listener to button
				UnityEventTools.AddStringPersistentListener(btn.onClick, new UnityAction<string>(statenode.MoveAlongPort), newPortName);
			}
*/

		}

		public override void OnBodyGUI() {
			//  dont draw anything yet if no node is assigned
			if (node.assignedCanvasRef.gameObject == null)
				return;
			base.OnBodyGUI();

			if (GUILayout.Button("Synchronize Button-Ports")) {
				if (node.assignedCanvasRef != null)
				{
					if (node.assignedCanvasRef.gameObject != null)
					{
						graph.addDragDropOperation(node.assignedCanvasRef.gameObject, node);
					}
				}
			};
			if (GUILayout.Button("Flip Active")) node.FlipActive();
			
			//UnityEngine.GameObject canvas = node.assignedCanvas;
		}
	}
}