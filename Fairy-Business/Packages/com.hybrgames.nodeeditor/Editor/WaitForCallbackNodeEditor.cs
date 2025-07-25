using UnityEngine;
using XNode.UiStateGraph;
using System.Linq;
using System.Collections.Generic;

namespace XNodeEditor.UiStateGraph {
	[CustomNodeEditor(typeof(WaitForCallbackNode))]
	public class WaitForCallbackNodeEditor : NodeEditor {
		private WaitForCallbackNode node;
        public override void OnCreate()
        {
			// Initialization
			if (node == null) {
				node = target as WaitForCallbackNode;
			}
            base.OnCreate();
        }
		public override void OnHeaderGUI() {
			GUI.color = Color.white;
			//if (node.atLeastOneCallerAssigned == false)
			//	GUI.color = Color.red;
			if (node.active) GUI.color = Color.blue;
			string title = target.name;
			GUILayout.Label(title, NodeEditorResources.styles.nodeHeader, GUILayout.Height(30));
			GUI.color = Color.white;
		}

		public override void OnBodyGUI() {
			base.OnBodyGUI();

			if (GUILayout.Button("Add Port")) {
				// modify the set portname so it always starts with an Uppercase letter and has no empty spaces
				string modifiedPortname = node.newPort;
        		modifiedPortname = string.Concat(modifiedPortname.Where(c => !char.IsWhiteSpace(c)));
				modifiedPortname = char.ToUpper(modifiedPortname[0]) + modifiedPortname.Substring(1);
				node.newPort = modifiedPortname;
				NodeEditorWindow.current.onLateGUI += () => node.AddDynamicOutput(typeof(UiStateNode.ButtonNode), fieldName: node.newPort);
			};
			if (GUILayout.Button("Clear Unconnected Ports")) {
				List<string> portsToDelete = new();
				foreach (var port in node.DynamicPorts)
				{
					if (port.ConnectionCount <= 0){
						portsToDelete.Add(port.fieldName);
					}
				}
				foreach (var item in portsToDelete)
				{
					NodeEditorWindow.current.onLateGUI += () => node.RemoveDynamicPort(item);
					
				}
			};

		}

	}
}