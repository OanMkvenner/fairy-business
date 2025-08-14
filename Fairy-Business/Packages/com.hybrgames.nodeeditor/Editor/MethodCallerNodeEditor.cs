using UnityEditor;
using UnityEngine;
using XNode.UiStateGraph;
using XNode;

namespace XNodeEditor.UiStateGraph {
	[CustomNodeEditor(typeof(MethodCallerNode))]
	public class MethodCallerNodeEditor : NodeEditor {
		private MethodCallerNode node;
        public override void OnCreate()
        {
			// Initialization
			if (node == null) {
				node = target as MethodCallerNode;
			}
            base.OnCreate();
        }
		public override void OnHeaderGUI() {
			CustomNodesHelper.DrawEnterExitPorts(node);
			GUI.color = Color.white;
			if (node.assignedMethodTarget.gameObject == null)
				GUI.color = Color.red;
			if (node.active) GUI.color = Color.blue;
			string title = target.name;
			if (node.assignedMethodTarget.gameObject == null)
				title = "DRAG GAMEOBJECT HERE";
			GUILayout.Label(title, NodeEditorResources.styles.nodeHeader, GUILayout.Height(30));
			GUI.color = Color.white;
		}

		public override void OnBodyGUI() {
			if (node.assignedMethodTarget.gameObject == null) return;

			base.OnBodyGUI();

			EditorGUILayout.LabelField("Target Object: ", node.assignedMethodTarget.gameObject.name);
			EditorGUILayout.LabelField("Component: ", node.targetComponent);

			// use a self-built dropdown menu to postpone any method-List updates to when they are needed (on click)
			if (EditorGUILayout.DropdownButton(new GUIContent(node.methodToCall), FocusType.Passive, new GUILayoutOption[0])) {
				// Position is all wrong if we show the dropdown during the node draw phase.
				// Instead, add it to onLateGUI to display it later.
				NodeEditorWindow.current.onLateGUI += () => ShowContextMenuAtMouse(node);
			};

			//Old method for reference:
			//int index;
			//if (methods.Length > 0) {
			//	try
			//	{
			//		index = methods
			//			.Select((v, i) => new { Name = v, Index = i })
			//			.First(x => x.Name == methodCallerNode.methodToCallFullPath)
			//			.Index;
			//	}
			//	catch
			//	{
			//		index = 0;
			//	}
			//	methodCallerNode.setMethodToCall(EditorGUILayout.Popup(index, methods));
		}

		public void ShowContextMenuAtMouse(MethodCallerNode methodCallerNode) {
			// Initialize menu
			GenericMenu menu = new GenericMenu();

			string[] methods = methodCallerNode.GetMethodsOfTarget();

			// Add all enum display names to menu
			for (int i = 0; i < methods.Length; i++) {
				int index = i;
				menu.AddItem(new GUIContent(methods[i]), false, () => {
            		serializedObject.Update();
					methodCallerNode.setMethodToCall(index);
					// update serialized variables manually (since they are changed outside of editor scope)
            		serializedObject.FindProperty("methodToCall").stringValue = methodCallerNode.methodToCall;
            		serializedObject.FindProperty("methodToCallFullPath").stringValue = methodCallerNode.methodToCallFullPath;
            		serializedObject.FindProperty("targetComponent").stringValue = methodCallerNode.targetComponent;
           			serializedObject.ApplyModifiedProperties();
				});
			}

			// Display at cursor position
			Rect r = new Rect(Event.current.mousePosition, new Vector2(0, 0));
			menu.DropDown(r);
			
		}

	}
}