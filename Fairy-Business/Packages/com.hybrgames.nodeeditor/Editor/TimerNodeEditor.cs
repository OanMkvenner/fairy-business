using UnityEngine;
using XNode.UiStateGraph;
using System.Linq;

namespace XNodeEditor.UiStateGraph {
	[CustomNodeEditor(typeof(TimerNode))]
	public class TimerNodeEditor : NodeEditor {
		private TimerNode node;
        public override void OnCreate()
        {
			// Initialization
			if (node == null) {
				node = target as TimerNode;
			}
            base.OnCreate();
        }
		public override void OnHeaderGUI() {
			GUI.color = Color.white;
			if (node.active) GUI.color = Color.blue;
			string title = target.name;
			GUILayout.Label(title, NodeEditorResources.styles.nodeHeader, GUILayout.Height(30));
			GUI.color = Color.white;

			// example for drawing a red circle
			//Rect dotRect = GUILayoutUtility.GetLastRect();
			//dotRect.size = new Vector2(16, 16);
			//dotRect.y += 6;
			//GUI.color = Color.red;
			//GUI.DrawTexture(dotRect, NodeEditorResources.dot);
			//GUI.color = Color.white;
		}

		public override void OnBodyGUI() {
			base.OnBodyGUI();
		}

	}
}