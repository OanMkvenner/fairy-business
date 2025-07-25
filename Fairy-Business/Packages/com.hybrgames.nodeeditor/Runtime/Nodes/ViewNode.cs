using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using XNode;
using System.Linq;

namespace XNode.UiStateGraph {
	[NodeTint(42, 98, 122)]
	public class ViewNode : UiStateNode
	{
		[HideInInspector]
		public UnityEngine.UI.Button drb = null;
		[HideInInspector]
		public GuidReference assignedCanvasRef = new GuidReference();

        protected override void Init()
        {
			base.Init();
			tweenAnimators = null;
			transformReseters = null;
			canvasGroupReseters = null;
		}
		public void EnableCanvasInstant(GameObject canvasObject)
		{
			if (Application.isPlaying)
			{
				CenterCanvas(canvasObject);
				StartAllAutoAnimations(canvasObject);

				canvasObject.GetComponent<Canvas>().enabled = true;
				canvasObject.GetComponent<GraphicRaycaster>().enabled = true;
			}
		}
		public void DisableCanvasInstant(GameObject canvasObject)
		{
			//CanvasGroup cnvsGrp = assignedCanvas.GetComponent<CanvasGroup>();
			//cnvsGrp.alpha = 0;
			if (Application.isPlaying)
			{
				canvasObject.GetComponent<Canvas>().enabled = false;
				canvasObject.GetComponent<GraphicRaycaster>().enabled = false;

				// stop all running animations
				StopAllAnimations(canvasObject);
				
				//CanvasGroup cnvsGrp = assignedCanvas.GetComponent<CanvasGroup>();
				//cnvsGrp.alpha = 1;
				// reset positions of canvas itself and all its child objects that have a TransformReset component
				ResetAllObjectPositions(canvasObject);
				TransformReseter transReset = canvasObject.GetComponent<TransformReseter>();
				if (!transReset){
					// if it has no TransformReseter, just shove it out of the picture to make sure no contained objects stay visible
					RectTransform canvsRect = canvasObject.GetComponent<RectTransform>();
					canvsRect.localPosition = Vector3.right * 640;
				}
				CanvasGroupReseter cnvsGrpReset = canvasObject.GetComponent<CanvasGroupReseter>();
				if (cnvsGrpReset){
					cnvsGrpReset.ResetCanvasGroup();
				}

			}
		}

		public void FlipActive()
		{
			if (active)
			{
				OnDeactivate();
			}
			else
			{
				OnEnter(this, this.enterPort);
			}
		}

		public void DisableInteractions(){
			if (Application.isPlaying)
			{
				assignedCanvasRef.gameObject.GetComponent<CanvasGroup>().blocksRaycasts = false;
			}
		}
		public void EnableInteractions(){
			if (Application.isPlaying)
			{
				assignedCanvasRef.gameObject.GetComponent<CanvasGroup>().blocksRaycasts = true;
			}
		}
		public bool CheckInteractionsEnabled(){
			return assignedCanvasRef.gameObject.GetComponent<CanvasGroup>().blocksRaycasts;
		}

		override public void OnEnter(UiStateNode originNode, NodePort incomingPort)
		{
			// copy list so it cant get modified while working on it
			var originRegisteredViews = new List<Node>(originNode.registeredOriginViews);
			// deactivate all Views gathered in origins registeredOriginViews
			foreach (var registeredOriginView in originRegisteredViews)
			{
				ViewNode viewNode = registeredOriginView as ViewNode;
				if (viewNode && viewNode != this) {
					viewNode.OnDeactivate();
				}
			}
			// deactivate originNode directly, if it is a View itself
			ViewNode originView = originNode as ViewNode;
			if (originView && originView != this) {
				originView.OnDeactivate();
			}

			// if this view is currently still active, deactivate it first to reset all connected Tweensers/timers/...
			if (active)
				OnDeactivate();

			if (assignedCanvasRef.gameObject != null)
			{
				EnableCanvasInstant(assignedCanvasRef.gameObject);
				EnableInteractions();
			} else {
				Debug.LogWarning("No Canvas connected to this node");
				return;
			}

			base.OnEnter(originNode, incomingPort);
		}

		public void CenterCanvas(GameObject canvasObject) {
			// reset Canvas position
			RectTransform canvsRect = canvasObject.GetComponent<RectTransform>();
			// and then set position to be center of screen (keeping original z layer)
			float z = canvsRect.localPosition.z;
			canvsRect.localPosition = new Vector3(0, 0, z);
		}
		List<TransformReseter> transformReseters = null;
		List<CanvasGroupReseter> canvasGroupReseters = null;
		public void ResetAllObjectPositions(GameObject canvasObject) {
			// reset positions of all childs that have a TransformReseter
			// this INCLUDES the canvasObject itself!
			if (transformReseters == null)
			{
				// this only queries them ONCE! this means newly generated elements need to "register" themselves differently. No implementationd for that YET
				transformReseters = Utilities.GetComponentsInChildrenIncludingDisabled<TransformReseter>(canvasObject.transform);
			}
			foreach (var reseter in transformReseters)
			{
				reseter.ResetTransform();
			}
			if (canvasGroupReseters == null)
			{
				// this only queries them ONCE! this means newly generated elements need to "register" themselves differently. No implementationd for that YET
				canvasGroupReseters = Utilities.GetComponentsInChildrenIncludingDisabled<CanvasGroupReseter>(canvasObject.transform);
			}
			foreach (var reseter in canvasGroupReseters)
			{
				reseter.ResetCanvasGroup();
			}
		}

		List<TweenAnimator> tweenAnimators = null;
		public void StartAllAutoAnimations(GameObject canvasObject) {
			if (tweenAnimators == null)
			{
				// this only queries them ONCE! this means newly generated elements need to "register" themselves differently. No implementationd for that YET
				tweenAnimators = Utilities.GetComponentsInChildrenIncludingDisabled<TweenAnimator>(canvasObject.transform);
			}
			foreach (var animator in tweenAnimators)
			{
				animator.OnViewEnter();
			}

			// reset Canvas position
			RectTransform canvsRect = canvasObject.GetComponent<RectTransform>();
			// and then set position to be center of screen (keeping original z layer)
			float z = canvsRect.localPosition.z;
			canvsRect.localPosition = new Vector3(0, 0, z);
		}
		public void StopAllAnimations(GameObject canvasObject) {
			if (tweenAnimators == null)
			{
				// this only queries them ONCE! this means newly generated elements need to "register" themselves differently. No implementationd for that YET
				tweenAnimators = Utilities.GetComponentsInChildrenIncludingDisabled<TweenAnimator>(canvasObject.transform);
			}
			foreach (var animator in tweenAnimators)
			{
				animator.OnViewExit();
			}
		}

		public void StopAllConnectedNodes() {
			List<UiStateNode> connectedNodeList = GetAllConnectedNodes();
			foreach (var connectedNode in connectedNodeList)
			{
				connectedNode.OnDeactivate();
			}
		}

		public List<UiStateNode> GetAllConnectedNodes() {
			List<UiStateNode> nodeList = new List<UiStateNode>();
			UiStateNode currentNode = this;
			int idx = -1;
			int listLength = 0;
			while (currentNode != null)
			{
				IEnumerable<NodePort> portIterator = currentNode.GetCorrectedOutputs();
				foreach (var targetPort in portIterator)
				{
					List<NodePort> allConnections = targetPort.GetConnections();
					foreach (var incomingPort in allConnections)
					{
						UiStateNode newNode = incomingPort.node as UiStateNode;
						newNode = newNode.GetCorrectedNode(incomingPort) as UiStateNode;

						// if newNode is UiStateNode but not ViewNode type...
						if (newNode != null && !(newNode is ViewNode) && !(newNode is ViewControllerNode)) {
							//... and is not in the list yet, add it to the list. (will never add itself because of ViewNode check)
							if (!nodeList.Contains(newNode)) {
								nodeList.Add(newNode);
								listLength++;
							}
						}

					}
				}
				// iterate to next Node if possible
				idx++;
				if (idx < listLength)
					currentNode = nodeList[idx];
				else
					break;
				if (idx > 10000) {
					Debug.LogError("Iterative Loop-escape GetAllConnectedNodes! Check GetAllConnectedNodes() for buggy behavior");
					break;
				}
			}

			return nodeList;
		}

		override public void OnLeave() {
			// catch any OnLeave calls resulting from button activations.
			// and keep the View active instead of calling OnDeactivate
		}


		override public void OnDeactivate() {
			// deactivate all belonging Nodes, no matter this nodes state
			StopAllConnectedNodes();

			// only Disable canvas if its currently active
			if (active) {
				if (assignedCanvasRef.gameObject != null)
				{
					DisableCanvasInstant(assignedCanvasRef.gameObject);
				} else {
					Debug.LogWarning("No Canvas connected to this node");
				}
			}
			base.OnDeactivate();
		}

	#if (UNITY_EDITOR)
		public void UpdateButtonPorts(){
			GameObject canvasObject = assignedCanvasRef.gameObject;

			List<GameObject> allContainedToggleableButtons = new List<GameObject>();
			List<GameObject> allContainedButtons = new List<GameObject>();
			foreach(Transform trans in canvasObject.transform) {
				if (trans.GetComponent<Button>() != null)
				{
					allContainedButtons.Add(trans.gameObject);
				}
				if (trans.GetComponent<ButtonContainer>() != null) {
					ButtonContainer buttonCont = trans.GetComponent<ButtonContainer>();
					allContainedButtons.Add(trans.gameObject);
					// add node reference to target UiGraphCallback
					if (!buttonCont.targetNodes.Contains(this)) {
						buttonCont.targetNodes.Add(this);
					}
				} 
				if (trans.GetComponent<ToggleableButton>() != null)
				{
					allContainedToggleableButtons.Add(trans.gameObject);
				}
			}

			//WARNING check if this goes deeper than 1! it  shouldnt

			StateGraph stateGraph = graph as StateGraph;
			bool changedAtLeastOnePort = false;
			NodePort lastNewlyAddedPort = null;
			//AddButton
			foreach (var button in allContainedButtons)
			{
				string newPortName = button.name;
				// add click listener to Button without adding the port yet
				stateGraph.AddButtonPort(button.gameObject, this, addPortAsWell: false);
				// check if the port already exists
				bool alreadyExists = false;
				foreach (NodePort port in Outputs) { 
					if (port.fieldName == newPortName) {
						alreadyExists = true;
						break;
					}
				};
				if (!alreadyExists){
					// add dynamic output manually if needed
					lastNewlyAddedPort = this.AddDynamicOutput(typeof(UiStateNode.ButtonNode), fieldName: newPortName);
					changedAtLeastOnePort = true;
				}
			}
			foreach (var button in allContainedToggleableButtons)
			{
				// add on/off click listener to Button without adding the port yet
				stateGraph.AddToggleableButtonPort(button.gameObject, this, addPortAsWell: false);
				{
					string newPortName = button.name + "ChangedOn";
					// check if the port already exists
					bool alreadyExists = false;
					foreach (NodePort port in Outputs) { 
						if (port.fieldName == newPortName) {
							alreadyExists = true;
							break;
						}
					};
					if (!alreadyExists){
						// add dynamic output manually if needed
						lastNewlyAddedPort = this.AddDynamicOutput(typeof(UiStateNode.ButtonNode), fieldName: newPortName);
						changedAtLeastOnePort = true;
					}
				}
				{
					string newPortName = button.name + "ChangedOff";
					// check if the port already exists
					bool alreadyExists = false;
					foreach (NodePort port in Outputs) { 
						if (port.fieldName == newPortName) {
							alreadyExists = true;
							break;
						}
					};
					if (!alreadyExists){
						// add dynamic output manually if needed
						lastNewlyAddedPort = this.AddDynamicOutput(typeof(UiStateNode.ButtonNode), fieldName: newPortName);
						changedAtLeastOnePort = true;
					}
				}
			}


			List<NodePort> removedConnections = new List<NodePort>();
			var bufferedOutputs = this.DynamicOutputs.ToArray();
			int portsRemoved = 0;
			foreach (NodePort port in bufferedOutputs) { 
				bool stillExists = false;
				foreach (var button in allContainedButtons)
				{
					string newPortName = button.name;
					if (port.fieldName == newPortName) {
						stillExists = true;
						break;
					}
				}
				foreach (var button in allContainedToggleableButtons)
				{
					if (port.fieldName == button.name + "ChangedOn") {
						stillExists = true;
						break;
					}
					if (port.fieldName == button.name + "ChangedOff") {
						stillExists = true;
						break;
					}
				}
				if (!stillExists)
				{
					portsRemoved++;
					// buffer old connections
					removedConnections = port.GetConnections();
					// remove the old port
					RemoveDynamicPort(port);
					changedAtLeastOnePort = true;
				}
			};
			
			// if we removed one port and added one new, move the connections of the old to the new one. (probably a rename)
			if (lastNewlyAddedPort != null && portsRemoved == 1)
			{
				removedConnections.ForEach(connectionPort => lastNewlyAddedPort.Connect(connectionPort));
			}

			if (changedAtLeastOnePort)
			{
				(graph as StateGraph).QueueRepaint();
			}
		}

		public void removeButtonReferenceOnly(UnityEngine.UI.Button btn) {
			//TagJK_implement should remove port that references this button 
			//WIHTOUT removing the buttons listener (is done in OnDropObjects())
		}
	#endif // End #if (UNITY_EDITOR)

		public void AddCanvasReference(GuidReference canvasGuidRef) {
			assignedCanvasRef = canvasGuidRef;
		}
	}
}