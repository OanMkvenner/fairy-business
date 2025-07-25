using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Reflection;

using System.Linq;
using System;

namespace XNode.UiStateGraph {
	[NodeTint(163, 105, 36)]
	public class MethodCallerNode : UiStateNode {
		
		
    	[HideInInspector] public GuidReference assignedMethodTarget = new GuidReference(); // reference to the object, wich the specified method is called on
		// serialized
		[HideInInspector] public string methodToCall = "";
		[HideInInspector] public string methodToCallFullPath = "";
		[HideInInspector] public string targetComponent = "";
		// not serialized
		string[] targetMethods = new string[0];
		MethodInfo[] targetMethodInfos = new MethodInfo[0];
		string[] ignoreMethods = new string[] { "Start", "Update" }; // dont list these Methods

		override public void OnEnter(UiStateNode originNode, NodePort incomingPort) {
			// call the assigned method immideately, before continuing through PassThrough ports
			CallAssignedMethod(originNode.ConsumePayload());

			base.OnEnter(originNode, incomingPort);
			
			// no need to stay active after call, even if this is a dead-end
			if (active)
				OnDeactivate();
		}

		public void AddMethodCallerTarget(GuidReference canvasGuidRef){
			assignedMethodTarget = canvasGuidRef;
			updateAvailableMethods();
		}

		public void updateAvailableMethods(){
			GameObject targetObj = assignedMethodTarget.gameObject;
			if (targetObj == null) return;
			
			//targetMethods =
			Component[] componentList = targetObj.GetComponents(typeof(Component));

			IEnumerable<MethodInfo> methodList = Enumerable.Empty<MethodInfo>();
			foreach (var component in componentList)
			{
				if (!component) continue;
				var newMethods = component.GetType()
				.GetMethods(BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public) // Instance methods, both public and private/protected
				//.Where(x => componentName == "" || x.DeclaringType.ToString() == componentName) // Only list methods defined in a specific class
				.Where(x => !ignoreMethods.Any(n => n == x.Name)); // Don't list methods in the ignoreMethods array (so we can exclude Unity specific methods, etc.)
				methodList = methodList.Concat(newMethods);
			}
			targetMethodInfos = methodList.ToArray();
			targetMethods = methodList.Select(x => x.DeclaringType + "/" + x.Name).ToArray();
		}
		public string[] GetMethodsOfTarget(){
			if (targetMethods.Length == 0 || targetMethods.Length != targetMethodInfos.Length) {
				updateAvailableMethods();
			}
			return targetMethods;
		}
		public void setMethodToCall(int index){
			string fullMethodName = targetMethods[index];
			if(methodToCallFullPath == fullMethodName) { 
				return; // no change needed
			}
			methodToCallFullPath = fullMethodName;
			// shorten fullMethodName to the last entry behind all slashes
			int pos = fullMethodName.LastIndexOf("/") + 1;
			string methodNameOnly = fullMethodName.Substring(pos, fullMethodName.Length - pos);
			methodToCall = methodNameOnly;
			// get Typename
			targetComponent = targetMethodInfos[index].DeclaringType.ToString();
		}
		
		//void test(){
		//	//UnityEventTools.AddStringPersistentListener(btn.onClick, new UnityAction<string>(view_node.MoveAlongPort), newPortName);
		//	
		//}
//
		void CallAssignedMethod(UnityEngine.Object payload = null){
			if (methodToCall == "") return;
			object invokingObject = assignedMethodTarget.gameObject.GetComponent(targetComponent);

			if (invokingObject == null) return;
			var method = invokingObject.GetType().GetMethod(methodToCall, BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
			if (method == null) {
				Debug.LogError($"Method of name {methodToCall} was not found. Maybe it was renamed?");
				return;
			}
			// handle optional parameters
			var parameterCount = method.GetParameters().Length;
			List<object> parameters = new List<object>();
			if (parameterCount > 0) {
				parameters.Add(payload);
			}
			// additional parameters
			//if (parameterCount > 1)
			//	parameters.Add(payload2);
			
			// fill unused spaces with null objects to accomodate expected amount of object parameters
			while (parameters.Count < parameterCount)
			{
				parameters.Add(null);
			}

			method.Invoke(invokingObject, parameters.ToArray());	
		}
		
        
	}
}