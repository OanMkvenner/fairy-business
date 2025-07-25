using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using XNode.UiStateGraph;
using XNode;
using XNodeEditor;
using UnityEditor.Events;
using System.Linq;

namespace XNodeEditor.UiStateGraph {
	[CustomNodeEditor(typeof(GraphExitNode))]
	public class GraphExitNodeEditor : NodeEditor {
		private GraphExitNode node;
		public override void OnHeaderGUI() {
			// Initialization
			if (node == null) {
				node = target as GraphExitNode;
			}
			GUI.color = Color.white;
			string title = target.name;
			GUILayout.Label(title, NodeEditorResources.styles.nodeHeader, GUILayout.Height(30));
		}

		public override void OnBodyGUI() {
            serializedObject.Update();

            // Iterate through serialized properties, search for "enter" and draw it like the Inspector (But with ports)
            SerializedProperty iterator = serializedObject.GetIterator();
            bool enterChildren = true;
            while (iterator.NextVisible(enterChildren)) {
                enterChildren = false;
				if (iterator.name == "exitGraph")
                	NodeEditorGUILayout.PropertyField(iterator, true);
            }

			// add a button for jumping to parent graph straight away
            StateGraph graph = serializedObject.FindProperty("graph").objectReferenceValue as StateGraph;
			if (graph.parentSubgraphNode != null) {
				if (GUILayout.Button("Jump to Parent graph", GUILayout.Height(23))) {
					graph.previouselySelectedNode = target;
					NodeEditorWindow w = NodeEditorWindow.Open(graph.parentSubgraphNode.graph as XNode.NodeGraph);
					w.SelectNode(graph.parentSubgraphNode as Node, false);
					w.Home(); // Focus selected node
				}
			}

            serializedObject.ApplyModifiedProperties();
		}
		
		// whenever an Entry/Exit Node is renamed, update the corresponding port names on the connected subgraph
		public override void OnRename() {
			node.RenamedNode();
		}

        public override void OnCreate()
        {
			// Initialization
			if (node == null) {
				node = target as GraphExitNode;
			}
            base.OnCreate();
			// initialize Node
			node.InitOnCreate();
        }
    
	}
	
	// overload Inspector
	[CustomEditor(typeof(GraphExitNode), true)]	
    [CanEditMultipleObjects]
    public class GraphExitNodeInspector : Editor {
        public override void OnInspectorGUI() {
            serializedObject.Update();
            SerializedProperty graphProp = serializedObject.FindProperty("graph");
			StateGraph graph = graphProp.objectReferenceValue as StateGraph;
            if (GUILayout.Button("Edit graph", GUILayout.Height(40))) {
                NodeEditorWindow w = NodeEditorWindow.Open(graphProp.objectReferenceValue as XNode.NodeGraph);
                w.Home(); // Focus selected node
            }
			if (graph.parentSubgraphNode != null) {
				if (GUILayout.Button("Edit Parent graph", GUILayout.Height(40))) {
					graph.previouselySelectedNode = target as Node;
					NodeEditorWindow w = NodeEditorWindow.Open(graph.parentSubgraphNode.graph as XNode.NodeGraph);
					w.SelectNode(graph.parentSubgraphNode as Node, false);
					w.Home(); // Focus selected node
				}
			}

            serializedObject.ApplyModifiedProperties();
        }
    }

}