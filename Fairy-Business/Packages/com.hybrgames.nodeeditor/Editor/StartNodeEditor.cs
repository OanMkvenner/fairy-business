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
	[CustomNodeEditor(typeof(StartNode))]
	public class StartNodeEditor : NodeEditor {
		private StartNode node;
        public override void OnCreate()
        {
			// Initialization
			if (node == null) {
				node = target as StartNode;
			}
            base.OnCreate();
        }
		public override void OnHeaderGUI() {
			GUI.color = Color.white;
			string title = target.name;
			GUILayout.Label(title, NodeEditorResources.styles.nodeHeader, GUILayout.Height(30));
		}

		public override void OnBodyGUI() {
            // Iterate through serialized properties, search for "start" and draw it like the Inspector (But with ports)
            SerializedProperty iterator = serializedObject.GetIterator();
            bool enterChildren = true;
            while (iterator.NextVisible(enterChildren)) {
                enterChildren = false;
				if (iterator.name == "start")
                	NodeEditorGUILayout.PropertyField(iterator, true);
            }
		}
	}	
	// overload Inspector
	[CustomEditor(typeof(StartNode), true)]	
    [CanEditMultipleObjects]
    public class StartNodeInspector : Editor {
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
					NodeEditorWindow w = NodeEditorWindow.Open(graph.parentSubgraphNode.graph as XNode.NodeGraph);
					w.SelectNode(graph.parentSubgraphNode as Node, false);
					w.Home(); // Focus selected node
				}
			}

            serializedObject.ApplyModifiedProperties();
        }
    }
}