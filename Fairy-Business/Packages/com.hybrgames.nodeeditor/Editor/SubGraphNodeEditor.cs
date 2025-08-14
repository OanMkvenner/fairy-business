using UnityEditor;
using UnityEngine;
using XNode.UiStateGraph;
using XNode;
using System.Linq;

namespace XNodeEditor.UiStateGraph {
	[CustomNodeEditor(typeof(SubGraphNode))]
	public class SubGraphNodeEditor : NodeEditor {
		private SubGraphNode node;
		private StateGraph graph;
        public override void OnCreate()
        {
			// Initialization
			if (node == null) {
				node = target as SubGraphNode;
				graph = node.graph as StateGraph;
			}
            base.OnCreate();
        }
		public override void OnHeaderGUI() {
			if (node.subGraph != null)
				GUI.color = Color.green;
			else 
				GUI.color = Color.red;
			if (node.active) GUI.color = Color.blue;
			string title = target.name;
			if (node.subGraph == null)
				title = "SET TARGET SUBGRAPH";
			else {
				title = node.subGraph.name;
			}
			GUILayout.Label(title, NodeEditorResources.styles.nodeHeader, GUILayout.Height(30));
			GUI.color = Color.white;
		}

		public override void OnBodyGUI() {
            serializedObject.Update();
			
			// check if subGraph variable was changed in XNode environment
            SerializedProperty subGraph = serializedObject.FindProperty("subGraph");
			StateGraph newGraph = subGraph.objectReferenceValue as StateGraph;
			if (node.oldSubGraph != newGraph) {
				node.SetSubGraph(newGraph);
			}
			
			base.OnBodyGUI();
/*
			// manually draw all properties except "enter" and "passThrough" (basically do what base.OnBodyGUI(); does)
			// NOT NEEDED ANYMORE! we actually hide enter and passThrough by default now.
            string[] excludes = { "m_Script", "graph", "position", "ports", "enter", "passThrough" };
            // Iterate through serialized properties and draw them like the Inspector (But with ports)
            SerializedProperty iterator = serializedObject.GetIterator();
            bool enterChildren = true;
            while (iterator.NextVisible(enterChildren)) {
                enterChildren = false;
                if (excludes.Contains(iterator.name)) continue;
                NodeEditorGUILayout.PropertyField(iterator, true);
            }

            // Iterate through dynamic ports and draw them in the order in which they are serialized
            foreach (XNode.NodePort dynamicPort in target.DynamicPorts) {
                if (NodeEditorGUILayout.IsDynamicPortListPort(dynamicPort)) continue;
                NodeEditorGUILayout.PortField(dynamicPort);
            }

            serializedObject.ApplyModifiedProperties();
*/
			if (subGraph.objectReferenceValue != null){
				if (GUILayout.Button("Edit SubGraph", GUILayout.Height(23))) {
					NodeEditorWindow w = NodeEditorWindow.Open(subGraph.objectReferenceValue as XNode.NodeGraph);
					StateGraph subStateGraph = subGraph.objectReferenceValue as StateGraph;
					if (subStateGraph._startNodes.Count > 0){
						Node nodeToJumpto = subStateGraph._startNodes[0];
						if (subStateGraph.previouselySelectedNode != null)
							nodeToJumpto = subStateGraph.previouselySelectedNode;
						w.SelectNode(nodeToJumpto, false);
						w.Home(); // Focus selected node
					}
				}
			}
		}
	}

	
	// overload Inspector
	[CustomEditor(typeof(SubGraphNode), true)]	
    [CanEditMultipleObjects]
    public class SubGraphNodeInspector : Editor {
		private SubGraphNode node;

        public override void OnInspectorGUI() {
			// Initialization
			if (node == null) {
				node = target as SubGraphNode;
			}

            serializedObject.Update();
			
            SerializedProperty subGraph = serializedObject.FindProperty("subGraph");

            if (GUILayout.Button("Edit graph", GUILayout.Height(40))) {
                SerializedProperty graphProp = serializedObject.FindProperty("graph");
                NodeEditorWindow w = NodeEditorWindow.Open(graphProp.objectReferenceValue as XNode.NodeGraph);
                w.Home(); // Focus selected node
            }
			if (subGraph.objectReferenceValue != null){
				if (GUILayout.Button("Edit SubGraph", GUILayout.Height(40))) {
					NodeEditorWindow w = NodeEditorWindow.Open(subGraph.objectReferenceValue as XNode.NodeGraph);
					StateGraph subStateGraph = subGraph.objectReferenceValue as StateGraph;
					if (subStateGraph._startNodes.Count > 0){
						Node nodeToJumpto = subStateGraph._startNodes[0];
						if (subStateGraph.previouselySelectedNode != null)
							nodeToJumpto = subStateGraph.previouselySelectedNode;
						w.SelectNode(nodeToJumpto, false);
						w.Home(); // Focus selected node
					}
				}
			}

            GUILayout.Space(EditorGUIUtility.singleLineHeight);
            GUILayout.Label("Node data", "BoldLabel");

			// check if subGraph variable was changed in inspector
			StateGraph newGraph = subGraph.objectReferenceValue as StateGraph;
			if (node.oldSubGraph != newGraph) {
				node.SetSubGraph(newGraph);
			}

			NodeEditorGUILayout.PropertyField(subGraph, new GUIContent("Sub Graph"), true);

            serializedObject.ApplyModifiedProperties();
        }
    }
}