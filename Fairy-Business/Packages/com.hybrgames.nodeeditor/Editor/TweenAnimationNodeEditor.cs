using UnityEditor;
using UnityEngine;
using XNode.UiStateGraph;
using XNode;
using System.Linq;

namespace XNodeEditor.UiStateGraph {
	[CustomNodeEditor(typeof(TweenAnimationNode))]
	public class TweenAnimationNodeEditor : NodeEditor {
		private TweenAnimationNode node;
        public override void OnCreate()
        {
			// Initialization
			if (node == null) {
				node = target as TweenAnimationNode;
			}
            base.OnCreate();
        }
		public override void OnHeaderGUI() {
			CustomNodesHelper.DrawEnterExitPorts(node);
			GUI.color = Color.white;
			if (node.assignedTweenAnimationTarget.gameObject == null)
				GUI.color = Color.red;
			if (node.active) GUI.color = Color.blue;
			string title = target.name;
			if (node.assignedTweenAnimationTarget.gameObject == null)
				title = "DRAG GAMEOBJECT HERE";
			GUILayout.Label(title, NodeEditorResources.styles.nodeHeader, GUILayout.Height(30));
			GUI.color = Color.white;
		}

		public override void OnBodyGUI() {
			if (node.assignedTweenAnimationTarget.gameObject == null) return;
			base.OnBodyGUI();
			
			EditorGUILayout.LabelField(node.assignedTweenAnimationTarget.gameObject.name);
            GUILayout.Label(new GUIContent(node.tweenMode.ToString(), "Please set all settings in the inspector Window"), "BoldLabel");
		}


	}

	// overload Inspector
	[CustomEditor(typeof(TweenAnimationNode), true)]	
    [CanEditMultipleObjects]
    public class TweenAnimationNodeInspector : Editor {
		private TweenAnimationNode node;
        public override void OnInspectorGUI() {
			// Initialization
			if (node == null) {
				node = target as TweenAnimationNode;
			}
            serializedObject.Update();

            if (GUILayout.Button("Edit graph", GUILayout.Height(40))) {
                SerializedProperty graphProp = serializedObject.FindProperty("graph");
                NodeEditorWindow w = NodeEditorWindow.Open(graphProp.objectReferenceValue as XNode.NodeGraph);
                w.Home(); // Focus selected node
            }

            GUILayout.Space(EditorGUIUtility.singleLineHeight);
            GUILayout.Label("Node data", "BoldLabel");

			
            SerializedProperty tweenMode = serializedObject.FindProperty("tweenMode");
			NodeEditorGUILayout.PropertyField(tweenMode, new GUIContent("Tween Mode"), true);
            SerializedProperty easeMode = serializedObject.FindProperty("easeMode");
			NodeEditorGUILayout.PropertyField(easeMode, new GUIContent("Easing Mode"), true);

			string potentialError = node.GetPotentialError();
			if (potentialError == "") {
				string[] requiredDataFields = node.GetRequiredDataFields();
				if(requiredDataFields.Length > 0)
					node.x = EditorGUILayout.FloatField(new GUIContent(requiredDataFields[0]), node.x);
				if(requiredDataFields.Length > 1)
					node.y = EditorGUILayout.FloatField(new GUIContent(requiredDataFields[1]), node.y);
				if(requiredDataFields.Length > 2)
					node.z = EditorGUILayout.FloatField(new GUIContent(requiredDataFields[2]), node.z);
				if(requiredDataFields.Length > 3)
					node.k = EditorGUILayout.FloatField(new GUIContent(requiredDataFields[3]), node.k);

				if (node.tweenMode != TweenAnimationNode.TweenMode.HighlightHover)
				{
					SerializedProperty tweenToFromMode = serializedObject.FindProperty("tweenToFromMode");
					NodeEditorGUILayout.PropertyField(tweenToFromMode, new GUIContent("To/From Mode"), true);
				}

				//if (node.tweenToFromMode == node.ToFromMode.ToValue)
				//	node.toOrFrom = EditorGUILayout.FloatField(new GUIContent("To Multiplier"), node.toOrFrom);
				//else 
				//	node.toOrFrom = EditorGUILayout.FloatField(new GUIContent("From Multiplier"), node.toOrFrom);
				node.duration = EditorGUILayout.FloatField(new GUIContent("Over duration (s)"), node.duration);

			} else {
				GUIStyle style = new GUIStyle(EditorStyles.textArea);
        		style.wordWrap = true;
				EditorGUILayout.LabelField(potentialError, style);
			}
            serializedObject.ApplyModifiedProperties();
        }
    }
}