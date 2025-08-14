using UnityEditor;
using UnityEngine;
using XNode.UiStateGraph;
using XNode;

namespace XNodeEditor.UiStateGraph {
	[CustomNodeEditor(typeof(ViewControllerNode))]
	public class ViewControllerNodeEditor : NodeEditor {
		private ViewControllerNode node;
        public override void OnCreate()
        {
			// Initialization
			if (node == null) {
				node = target as ViewControllerNode;
			}
            base.OnCreate();
        }
		public override void OnHeaderGUI() {
			CustomNodesHelper.DrawEnterExitPorts(node);
			GUI.color = Color.white;
			string title = target.name;
			if (node.controllerMode == ViewControllerNode.ViewControllerMode.DeactivateViews) {
				title = "View Deactivator";
				GUI.color = new Color(1.0f, 0.25f, 0.2f);
			}
			if (node.controllerMode == ViewControllerNode.ViewControllerMode.KeepViewsAlive) {
				title = "View Adder";
				GUI.color = Color.yellow;
			}

			GUILayout.Label(title, NodeEditorResources.styles.nodeHeader, GUILayout.Height(30));
			GUI.color = Color.white;
		}

		public override void OnBodyGUI() {
			base.OnBodyGUI();

			EditorGUILayout.LabelField("Controller Mode ");

			// use a self-built dropdown menu to postpone any method-List updates to when they are needed (on click)
			if (EditorGUILayout.DropdownButton(new GUIContent(node.controllerMode.ToString()), FocusType.Passive, new GUILayoutOption[0])) {
				// Position is all wrong if we show the dropdown during the node draw phase.
				// Instead, add it to onLateGUI to display it later.
				NodeEditorWindow.current.onLateGUI += () => ShowContextMenuAtMouse(node);
			};

		}

		public static void ShowContextMenuAtMouse(ViewControllerNode viewControllerNode) {
			// Initialize menu
			GenericMenu menu = new GenericMenu();

			 
			string[] enumNames = System.Enum.GetNames(typeof(ViewControllerNode.ViewControllerMode));

			// Add all enum display names to menu
			for (int i = 0; i < enumNames.Length; i++) {
				int index = i;
				menu.AddItem(new GUIContent(enumNames[i]), false, () => viewControllerNode.controllerMode = (ViewControllerNode.ViewControllerMode)index);
			}

			// Display at cursor position
			Rect r = new Rect(Event.current.mousePosition, new Vector2(0, 0));
			menu.DropDown(r);
		}

	}
}