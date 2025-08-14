using UnityEngine;

namespace XNodeEditor.UiStateGraph {
    public class CustomNodesHelper {
        public static void DrawEnterExitPorts(XNode.UiStateGraph.UiStateNode node){
            GUILayout.Space(6);
            GUILayout.BeginHorizontal();
            NodeEditorGUILayout.PortField(GUIContent.none, node.enterPort, GUILayout.MinWidth(0f));
            NodeEditorGUILayout.PortField(GUIContent.none, node.passthroughPort, GUILayout.MinWidth(0f));
            GUILayout.EndHorizontal();
            GUILayout.Space(-27);
        }
    }
}