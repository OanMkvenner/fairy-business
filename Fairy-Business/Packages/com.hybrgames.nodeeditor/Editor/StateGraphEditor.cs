using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using XNode.UiStateGraph;
using XNodeEditor;
using XNode;
using UnityEngine.UI;
using UnityEditor.Events;
using System;

namespace XNodeEditor.UiStateGraph {
	[CustomNodeGraphEditor(typeof(StateGraph))]
	public class StateGraphEditor : NodeGraphEditor {
		/// <summary> 
		/// Overriding GetNodeMenuName lets you control if and how nodes are categorized.
	    /// In this example we are sorting out all node types that are not in the XNode.UiStateGraph namespace.
		/// </summary>
		public override string GetNodeMenuName(System.Type type) {
			if (type.Namespace == "XNode.UiStateGraph") {
				if(type == typeof(UiStateNode)) return null; // UiStateNode is not supposed to be visible
				return base.GetNodeMenuName(type).Replace("X Node/Ui State Graph/", "New Node/");
			} else return null;
		}

		/// <summary> Controls graph type colors </summary>
		public override Color GetTypeColor(System.Type type) {
			if (type == typeof(UiStateNode.EnterNode)) return new Color(0.9f, 0.6f, 0.0f);
			else if (type == typeof(StartNode.AnyNode)) return new Color(1.0f, 1.0f, 0.3f);
			else if (type == typeof(GraphEntryNode.AnyNode)) return new Color(1.0f, 1.0f, 0.3f);
			else if (type == typeof(GraphExitNode.AnyNode)) return new Color(1.0f, 0.0f, 0.0f);
			else if (type == typeof(UiStateNode.ImmideateExitPort)) return new Color(0.0f, 0.55f, 0.95f);
			else if (type == typeof(UiStateNode.ButtonNode)) return new Color(0.3f, 0.75f, 0.3f);
			else if (type == typeof(UiStateNode.TargetNode)) return new Color(0.75f, 0.75f, 0.0f);
			else return base.GetTypeColor(type);
		}
		
        /// <summary> Override to display custom tooltips </summary>
        public override string GetPortTooltip(XNode.NodePort port) {
			
            Type portType = port.ValueType;
            return portType.Name;
        }

		/// <summary> Controls graph noodle colors </summary>
		//public override Gradient GetNoodleGradient(NodePort output, NodePort input) {
		//	//StateNode node = output.node as StateNode;
		//	Gradient baseGradient = base.GetNoodleGradient(output, input);
		//	//HighlightGradient(baseGradient, Color.yellow, output, (bool) node.GetValue(output));
		//	return baseGradient;
		//}

		
		public override void OnGUI() {

		}

		override public void OnDropObjects(UnityEngine.Object[] objects) {
			for (int i = 0; i < objects.Length; i++)
			{
				#if UNITY_EDITOR // in case we get an OnDropObjects event in Android (not sure if that can even happen)
				UnityEngine.Object dropped_obj = objects[i];
				if (dropped_obj != null && XNodeEditor.NodeEditorWindow.current != null) {
					StateGraph _stateGraph = XNodeEditor.NodeEditorWindow.current.graph as StateGraph;
					// drag and drop needs to happen later. Adding it now results in a UI-thread error. Not Good but also not Bad. Buts lets avoid it anyway
					_stateGraph.addDragDropOperation(dropped_obj, XNodeEditor.NodeEditorWindow.current.HoveredNode);
				}
				#endif

			}
		}

	}
}