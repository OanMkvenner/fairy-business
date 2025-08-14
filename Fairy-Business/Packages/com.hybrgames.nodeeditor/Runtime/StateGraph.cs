using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using System.Linq;

namespace XNode.UiStateGraph {
	[CreateAssetMenu(fileName = "New Ui Graph", menuName = "Graph/Ui State Graph")]
	//[RequireNode(typeof(StartNode))] // i COULD re-implement this, but actually we dont NEED a StartNode on every Graph... lets keep it manual instead
	public class StateGraph : NodeGraph {

		// The current "active" node
		List<NodeDropOperation> playModeDragDropQueue = new List<NodeDropOperation>();
		List<NodeDropOperation> editorDragDropQueue = new List<NodeDropOperation>();

		public List<StartNode> _startNodes;
		public bool queuedRepaint = false;
		
    	public SubGraphNode parentSubgraphNode;

		public Node previouselySelectedNode = null;

		public List<StateGraph> GetConnectedStateGraphList(){
			List<StateGraph> graphList = new List<StateGraph>();
			graphList.Add(this);

			List<StateGraph> subGraphs = GetSubGraphList().ToList();
			int i = 0;
			while (i < subGraphs.Count)
			{
				var currentSubGraph = subGraphs[i];
				// avoid infinite looping
				if (!graphList.Contains(currentSubGraph))
				{
					graphList.Add(currentSubGraph);
					var additionalSubGraphs = currentSubGraph.GetSubGraphList();
					foreach (var graph in additionalSubGraphs)
					{
						subGraphs.Add(graph);
					}
				} else {
					Debug.LogWarning("Warning: SubGraph loop detected! Avoid using parent Graphs as Subgraphs. Hangups may occur!");
				}
				i++;
			}

			return graphList;
		}

		public void InitGraph(){
			ClearAllActive();
			DeactivateViewGameObjects();
			
			var startNodeList = nodes.FindAll(node => node is StartNode);
			_startNodes = new();
			foreach (var item in startNodeList)
			{
				_startNodes.Add((StartNode) item);
			}
		}
		public void OnStart(){
			// initialize all startnodes
			foreach (var item in _startNodes)
			{
				item.MoveToFirstNode();
			}
		}
		public List<T> GetAllNodesOfType<T>() where T: UiStateNode {
			var searchedNodeList = nodes.FindAll(node => node is T);
			List<T> searchedNodes = new();
			foreach (var item in searchedNodeList)
			{
				searchedNodes.Add(item as T);
			}
			return searchedNodes;
		}


		public void OnUpdate(){
			// handle Timers
			for (int i = 0; i < nodes.Count; i++) {
				UiTimerTick timerTick = nodes[i] as UiTimerTick;
				if (timerTick != null) {
					// if we want to run timers during editortime we might need something like EditorApplication.timeSinceStartup - lastFrame);
					timerTick.Tick(Time.unscaledDeltaTime);
				}
			}
		}

		public StateGraph[] GetSubGraphList(SubGraphNode excludingNode = null){
			return nodes.Where(node => node is SubGraphNode && node != excludingNode)
						.Where(x => (x as SubGraphNode).subGraph != null)
						.Select(x => (x as SubGraphNode).subGraph)
						.ToArray();
		}

		public bool RegisterNewSubgraphNode(SubGraphNode subGraphNode, StateGraph targetGraph){
			StateGraph[] subGraphs = GetSubGraphList(subGraphNode);
			if (targetGraph == null){
				return true;
			}
			if (targetGraph == this){
				Debug.LogError("Failed adding Subgraph: target Graph cant add itself as its own subGraph");
				return false;
			}
			if (!subGraphs.Contains(targetGraph)) {
				if (targetGraph.parentSubgraphNode == null
				 || targetGraph.parentSubgraphNode == subGraphNode) {
					targetGraph.parentSubgraphNode = subGraphNode;
					return true;
				} else {
					Debug.LogError("Failed adding Subgraph: target Graph can only have one Parent");
					return false;
				}
			} else {
				Debug.LogError("Failed adding Subgraph: target Graph is already added as a Subgraph of this Graph");
				return false;
			}
		}
		
		public Node SearchNodeOfNameAndType(string searchName, System.Type searchType){
			// enter "Entrance" node, defined by name of incomingPort (!?)
			Node foundNode = null;
			foreach (Node node in nodes.Where(node => node.GetType() == searchType)) { 
				if (searchName == node.name) {
					foundNode = node;
					break;
				}
			};
			return foundNode;
		}


		// deactivate all view-gameobjects
		public void DeactivateViewGameObjects(){
			if (Application.isPlaying)
			{
				foreach (var node in nodes)
				{
					ViewNode currentNode = node as ViewNode;
					if (currentNode){
						if (currentNode.assignedCanvasRef != null)
						{
							if (currentNode.assignedCanvasRef.gameObject != null){
								currentNode.assignedCanvasRef.gameObject.GetComponent<Canvas>().enabled = false;
								currentNode.assignedCanvasRef.gameObject.GetComponent<GraphicRaycaster>().enabled = false;
							}
						}
					}
					QueueRepaint();
				}
			}
		}
		// deactivate all active graph elements to reset graph state
		public void ClearAllActive(){
			foreach (var node in nodes)
			{
				UiStateNode currentNode = node as UiStateNode;
				if (currentNode){
					currentNode.active = false;
				}
				QueueRepaint();
			}
		}

		// Repaint every time something relevant changes (e.g. OnEnter is called on any node)
		public void QueueRepaint(){
			#if (UNITY_EDITOR)
			if (XNodeEditor.NodeEditorWindow.current) {
				if (XNodeEditor.NodeEditorWindow.current.graph == this){
					// only repaint if this graph is currently actively shown
					queuedRepaint = true;
				};
			}
			#endif // End #if (UNITY_EDITOR)
		}

		public void addDragDropOperation(Object dropped_obj, Node hovered_node){
			if(Application.isPlaying) {
				playModeDragDropQueue.Add(new NodeDropOperation(dropped_obj, hovered_node));
			} else {
				editorDragDropQueue.Add(new NodeDropOperation(dropped_obj, hovered_node));
			}
		}

		public void PostProcessDropOperations(){
			if(Application.isPlaying) {
				// handle all in-playmode drag and drop operations and copy the same operations 
				// to dragDropQueueEditor, so they are added again after the play mode stops 
				// (needed because GameObjects get reset after playmode stops)
				foreach (var drag_drop_op in playModeDragDropQueue)
				{
					editorDragDropQueue.Add(drag_drop_op);
					handleDragDropOperation(drag_drop_op);
				}
				playModeDragDropQueue.Clear();
			} else {
				// handle all out-of-playmode drag and drop operations
				foreach (var drag_drop_op in editorDragDropQueue)
				{
					handleDragDropOperation(drag_drop_op);
				}
				editorDragDropQueue.Clear();
			}
		}

		void handleDragDropOperation(NodeDropOperation drag_drop_op){
			#if (UNITY_EDITOR)

			if (drag_drop_op == null) return;
			if (!drag_drop_op.dropped_obj) return;
			
			try {
				// Handle all dropped GameObjects
				GameObject dropped_gameobj = drag_drop_op.dropped_obj as GameObject;
				if (dropped_gameobj != null)
				{
					// handle button draggin into ViewNode
					if (dropped_gameobj.GetComponent<Button>() != null
					&& drag_drop_op.hovered_node is ViewNode) {
						AddButtonPort(dropped_gameobj, drag_drop_op.hovered_node);
					};

					// handle canvas dragging into ViewNode
					if (drag_drop_op.hovered_node is ViewNode) {
						AddViewCanvas(dropped_gameobj, drag_drop_op.hovered_node);
					};

					// handle object dragging into MethodCallerNode
					if (drag_drop_op.hovered_node is MethodCallerNode){
						AddMethodCaller(dropped_gameobj, drag_drop_op.hovered_node);
					};

					// handle object dragging into MethodCallerNode
					if (drag_drop_op.hovered_node is TweenAnimationNode){
						AddTweenAnimationTarget(dropped_gameobj, drag_drop_op.hovered_node);
					};

				// Handle all dropped non-GameObjects
				} else {
					// handle Graph dragging into SubGraph
					if (drag_drop_op.hovered_node is SubGraphNode){
						AddSubgraph(drag_drop_op.dropped_obj, drag_drop_op.hovered_node);
					};
				}


			} catch {
				Debug.LogError("Node Drag and Drop handling failed!");
			}
			#endif // End #if (UNITY_EDITOR)
		}
		
		#if (UNITY_EDITOR)
		// editor functionality required for StateGraph drag and drop resolving
		public void AddMethodCaller(GameObject dropped_obj, Node hovered_node){
			MethodCallerNode methodcallerNode = hovered_node as MethodCallerNode;
			if (methodcallerNode == null) return; // not hovering a node or not hovering proper Node type

			// add target to node
			GuidComponent guidComp = getGuidComponentSafely(dropped_obj);
			methodcallerNode.AddMethodCallerTarget(new GuidReference(guidComp));
		}
		public void AddTweenAnimationTarget(GameObject dropped_obj, Node hovered_node){
			TweenAnimationNode tweenAnimationNode = hovered_node as TweenAnimationNode;
			if (tweenAnimationNode == null) return; // not hovering a node or not hovering proper Node type

			// add a TransformReseter to make sure the tweened object always returns to its original position
			TransformReseter transResetComp = dropped_obj.GetComponent<TransformReseter>();
			if (transResetComp == null) {
				dropped_obj.AddComponent<TransformReseter>();
				Debug.LogWarning("Dragged Object had no TransformReseter. Added one for you to make sure the object"
					+ " always returns to its original position. You can remove it again if you dont want that behaviour");
			}
			// add target to node
			GuidComponent guidComp = getGuidComponentSafely(dropped_obj);
			tweenAnimationNode.AddTweenAnimationTarget(new GuidReference(guidComp));
		}
		public void AddSubgraph(Object dropped_obj, Node hovered_node){
			SubGraphNode subgraphNode = hovered_node as SubGraphNode;
			if (subgraphNode == null) return; // not hovering a node or not hovering proper Node type

			if (dropped_obj.GetType() != typeof(StateGraph)) {
				Debug.LogWarning("Dragged Object is no StateGraph. Only StateGraphs allowed as SubGraphs currently.");
				return;
			}
			StateGraph stateGraph = dropped_obj as StateGraph;
			subgraphNode.SetSubGraph(stateGraph);
		}
		public void AddViewCanvas(GameObject dropped_obj, Node hovered_node){
			ViewNode view_node = hovered_node as ViewNode;
			if (view_node == null) return;

			// handle canvas dragging
			if (dropped_obj.GetComponent<CanvasGroup>() == null)
			{	
				Debug.LogWarning("GameObject missing CanvasGroup, adding it now...");
				dropped_obj.AddComponent<CanvasGroup>();
			}
			if (dropped_obj.GetComponent<ViewNodeReferencer>() == null)
			{
				Debug.LogWarning("GameObject missing ViewNodeReferencer, adding it now...");
				dropped_obj.AddComponent<ViewNodeReferencer>();
			}
			if (Application.isPlaying) dropped_obj.GetComponent<ViewNodeReferencer>().AddTargetViewNode(view_node);

			GuidComponent guidComp = getGuidComponentSafely(dropped_obj);
			view_node.AddCanvasReference(new GuidReference(guidComp));
			// rename node header to canvas name
			view_node.name = dropped_obj.name;

			view_node.UpdateButtonPorts();
		}

		public void AddButtonPort(GameObject dropped_obj, Node hovered_node, bool addPortAsWell = true){
			ViewNode view_node = hovered_node as ViewNode;
			if (view_node == null) return; // not hovering a node or not hovering proper Node type

			Button btn = dropped_obj.GetComponent<Button>();
			if (btn == null) return;

			// add new port
			string newPortName = btn.name;
			if (addPortAsWell)
			{
				NodePort _newPort = view_node.AddDynamicOutput(typeof(UiStateNode.ButtonNode), fieldName: newPortName);
			}

			// iterate backwards through listeners and remove empty events
			int _previousListenerCount = btn.onClick.GetPersistentEventCount();
			for (int listener_idx = _previousListenerCount - 1; listener_idx >= 0 ; listener_idx--)
			{
				// clean up empty dangling listeners (can happen when removing nodes)
				if (btn.onClick.GetPersistentTarget(listener_idx) == null)
				{
					UnityEditor.Events.UnityEventTools.RemovePersistentListener(btn.onClick, listener_idx);
				}
				// remove listeners that point to this node already
				else if (btn.onClick.GetPersistentTarget(listener_idx) == view_node)
				{
					view_node.removeButtonReferenceOnly(btn);
					UnityEditor.Events.UnityEventTools.RemovePersistentListener(btn.onClick, listener_idx);
				}
				// could remove listeners to other nodes if needed, but CAREFUL this also removes connections to multiple instances of the same ViewNode
				//else if (btn.onClick.GetPersistentMethodName(listener_idx) == "MoveAlongPort")
				//{
				//	ViewNode _targetNode = btn.onClick.GetPersistentTarget(listener_idx) as ViewNode;
				//	_targetNode.removeButtonReferenceOnly(btn);
				//	UnityEditor.Events.UnityEventTools.RemovePersistentListener(btn.onClick, listener_idx);
				//}
			}
			// add new listener to button
			UnityEditor.Events.UnityEventTools.AddStringPersistentListener(btn.onClick, new UnityAction<string>(view_node.MoveAlongPort), newPortName);

			//UnityEditor.Events.UnityEventTools.AddPersistentListener(btn.onClick, new UnityAction(view_node.MoveNext));
			//UnityEditor.Events.UnityEventTools.AddObjectPersistentListener(btn.onClick, new UnityAction<UnityEngine.Object>(view_node.MoveAlongSpecifiedPort), view_node as UnityEngine.Object);
#if UNITY_EDITOR
			// setting this to dirty, so it gets saved properly in e.g. Prefab instances
			UnityEditor.EditorUtility.SetDirty(dropped_obj);
			UnityEditor.PrefabUtility.RecordPrefabInstancePropertyModifications(dropped_obj.GetComponent<Button>());
#endif
		}
		public void AddToggleableButtonPort(GameObject dropped_obj, Node hovered_node, bool addPortAsWell = true){
			ViewNode view_node = hovered_node as ViewNode;
			if (view_node == null) return; // not hovering a node or not hovering proper Node type

			ToggleableButton btn = dropped_obj.GetComponent<ToggleableButton>();
			if (btn == null) return;

			{
				// add new port
				string newPortName = btn.name + "ChangedOn";
				if (addPortAsWell)
				{
					NodePort _newPort = view_node.AddDynamicOutput(typeof(UiStateNode.ButtonNode), fieldName: newPortName);
				}
				// iterate backwards through listeners and remove empty events
				int _previousListenerCount = btn.onChangedOn.GetPersistentEventCount();
				for (int listener_idx = _previousListenerCount - 1; listener_idx >= 0 ; listener_idx--)
				{
					// clean up empty dangling listeners (can happen when removing nodes)
					if (btn.onChangedOn.GetPersistentTarget(listener_idx) == null)
					{
						UnityEditor.Events.UnityEventTools.RemovePersistentListener(btn.onChangedOn, listener_idx);
					}
					// remove listeners that point to this node already
					else if (btn.onChangedOn.GetPersistentTarget(listener_idx) == view_node)
					{
						//view_node.removeButtonReferenceOnly(btn);
						UnityEditor.Events.UnityEventTools.RemovePersistentListener(btn.onChangedOn, listener_idx);
					}
				}
				// add new listener to button
				UnityEditor.Events.UnityEventTools.AddStringPersistentListener(btn.onChangedOn, new UnityAction<string>(view_node.MoveAlongPort), newPortName);
			}
			{
				// add new port
				string newPortName = btn.name + "ChangedOff";
				if (addPortAsWell)
				{
					NodePort _newPort = view_node.AddDynamicOutput(typeof(UiStateNode.ButtonNode), fieldName: newPortName);
				}
				// iterate backwards through listeners and remove empty events
				int _previousListenerCount = btn.onChangedOff.GetPersistentEventCount();
				for (int listener_idx = _previousListenerCount - 1; listener_idx >= 0 ; listener_idx--)
				{
					// clean up empty dangling listeners (can happen when removing nodes)
					if (btn.onChangedOff.GetPersistentTarget(listener_idx) == null)
					{
						UnityEditor.Events.UnityEventTools.RemovePersistentListener(btn.onChangedOff, listener_idx);
					}
					// remove listeners that point to this node already
					else if (btn.onChangedOff.GetPersistentTarget(listener_idx) == view_node)
					{
						//view_node.removeButtonReferenceOnly(btn);
						UnityEditor.Events.UnityEventTools.RemovePersistentListener(btn.onChangedOff, listener_idx);
					}
				}
				UnityEditor.Events.UnityEventTools.AddStringPersistentListener(btn.onChangedOff, new UnityAction<string>(view_node.MoveAlongPort), newPortName);
			}
#if UNITY_EDITOR
			// setting this to dirty, so it gets saved properly in e.g. Prefab instances
			UnityEditor.EditorUtility.SetDirty(dropped_obj);
			UnityEditor.PrefabUtility.RecordPrefabInstancePropertyModifications(dropped_obj.GetComponent<ToggleableButton>());
#endif
		}
		
		public GuidComponent getGuidComponentSafely(GameObject dropped_obj){
				GuidComponent guidComp = dropped_obj.GetComponent<GuidComponent>();
				if (guidComp == null) {
					dropped_obj.AddComponent<GuidComponent>();
					guidComp = dropped_obj.GetComponent<GuidComponent>();
					Debug.LogWarning("Dragged Object had no GuidComponent. Added one for you.");
				}
				return guidComp;
		}

		#endif // End #if (UNITY_EDITOR)
		

		class NodeDropOperation {
			public Object dropped_obj;
			public Node hovered_node;
			public NodeDropOperation(Object dropped_obj_i, Node hovered_node_i){
				dropped_obj = dropped_obj_i;
				hovered_node = hovered_node_i;
			}
		}
	}
}