using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using XNode;
using System.Linq;
using UnityEngine.Events;
using UnityEngine.EventSystems;

public class ViewNodePortConnector : MonoBehaviour
{
	private ViewNodeReferencer parentViewNodeReferencer = null;
    [Tooltip("Can be used to change the created portname to use a static string insteadof using the name of the gameobject.")]
	public string overridePortName = "";
    [Tooltip("Added to the end of the Port-Name. Usually added automatically by special Interface types like CustomButton. If more than one of these is added, multiple ports are generated in the ViewNodes.")]
	[SerializeField]
	public List<string> portNameEndings = new();
    [Tooltip("Dont create Port on resync. Can be used to resync a ViewNode but ignore this ViewNodeConnector during this resync.")]
	public bool dontCreatePortOnResync = false;
	public void UpdateParentViewNode(){
		parentViewNodeReferencer = null;
		CheckAssignViewNodeReferencer(transform);
	}
	public void CheckAssignViewNodeReferencer(Transform checkTrans){
		if (checkTrans == null) return;
		var referencer = checkTrans.GetComponent<ViewNodeReferencer>();
		if (referencer != null) {
			parentViewNodeReferencer = referencer;
			return;
		}
		CheckAssignViewNodeReferencer(checkTrans.parent);
	}
	public void OnTransformParentChanged() {
		UpdateParentViewNode();
	}
	public void CallAssignedViewNodePorts(){
		CallAssignedViewNodePorts(-1);
	}
	public void CallAssignedViewNodePorts(int portNameIdx = -1){
		if (!isActiveAndEnabled) return;
		if (parentViewNodeReferencer == null) UpdateParentViewNode();
		if (parentViewNodeReferencer == null) {
			Debug.LogError("ViewNodePortConnector cant find parent ViewNodeReferencer!", this);
			return;
		}
		var portNames = GetPortNames();
		string portName;
		if (portNameIdx > portNames.Count) {
			Debug.LogError("CallAssignedViewNodePorts called with too high portNameIdx", this);
			return;
		} else if (portNameIdx == -1) {
			portName = GetPortBaseName();
		} else {
			portName = portNames[portNameIdx];
		}
		var tgtViewNodes = parentViewNodeReferencer.GetTargetViewNodes();
		foreach (var viewNode in tgtViewNodes)
		{
			viewNode.MoveAlongPortSupressingErrors(portName);
			//viewNode.MoveAlongPort(portName);
		}
	}
	public string GetPortBaseName(){
		var portName = overridePortName;
		if (portName == "") portName = transform.name;
		return portName;
	}
	public List<string> GetPortNames() {
		List<string> portNameList = new();
		foreach (var item in portNameEndings)
		{
			portNameList.Add(GetPortBaseName() + item);
		}
		return portNameList;
	}
	public void AssignPortNameEndings(List<string> newPortNameEndings){
		portNameEndings = newPortNameEndings;
	}
	private void Start() {
		FindAndAssignDefaultInteractable();
	}
	// called when first added to inspector
	void Reset(){
		// tell any existing special component to initialize me using AssignViewNodePortConnecter interface
		ExecuteEvents.Execute<IAssignViewNodePortConnecter>(gameObject, null, (x,y)=>x.AssignViewNodePortConnecter());
		//more basic variant, but it yields an annoying Assert when called at editor-time. No real problem, but annoying
		// gameObject.SendMessage("AssignViewNodePortConnecter", null, SendMessageOptions.DontRequireReceiver);
	}
	public void FindAndAssignDefaultInteractable(){
		var defaultButton = GetComponent<Button>();
		if (defaultButton != null){
			defaultButton.onClick.AddListener(new UnityAction(CallAssignedViewNodePorts));
		}
		// other components like CustomButton's should themselves search for a ViewNodePortConnector and "apply" themselves
	}

}
public interface IAssignViewNodePortConnecter : UnityEngine.EventSystems.IEventSystemHandler
{
	void AssignViewNodePortConnecter();
}