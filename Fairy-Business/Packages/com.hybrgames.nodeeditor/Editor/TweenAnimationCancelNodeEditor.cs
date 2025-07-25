using UnityEditor;
using UnityEngine;
using XNode.UiStateGraph;
using XNode;
using System.Linq;

namespace XNodeEditor.UiStateGraph {
	[CustomNodeEditor(typeof(TweenAnimCancelNode))]
	public class TweenAnimCancelNodeEditor : NodeEditor {
		private TweenAnimCancelNode node;
        public override void OnCreate()
        {
			// Initialization
			if (node == null) {
				node = target as TweenAnimCancelNode;
			}
            base.OnCreate();
        }
		public override void OnHeaderGUI() {
			GUI.color = Color.white;
			if (node.active) GUI.color = Color.blue;
			string title = target.name;
			GUILayout.Label(title, NodeEditorResources.styles.nodeHeader, GUILayout.Height(30));
			GUI.color = Color.white;
		}

		public override void OnBodyGUI() {
			base.OnBodyGUI();
		}
	}
}