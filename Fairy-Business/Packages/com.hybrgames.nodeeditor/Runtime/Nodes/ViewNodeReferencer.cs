using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using XNode;
using System.Linq;

[DisallowMultipleComponent]
public class ViewNodeReferencer : MonoBehaviour
{
	public List<XNode.UiStateGraph.ViewNode> targetViewNodes = new();
	public void AddTargetViewNode(XNode.UiStateGraph.ViewNode viewNode){
		if (!targetViewNodes.Contains(viewNode)) targetViewNodes.Add(viewNode);
	}
	public List<XNode.UiStateGraph.ViewNode> GetTargetViewNodes(){
		return targetViewNodes;
	}
}