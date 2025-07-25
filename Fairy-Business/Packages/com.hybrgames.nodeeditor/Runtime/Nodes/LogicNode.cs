using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Reflection;

using System.Linq;
using System;

namespace XNode.UiStateGraph {
	public class LogicNode : UiStateNode {
		//public var input;

		[Output] public LogicTrueNode truePort;
		[Output] public LogicFalseNode falsePort;


		[Serializable]
		public class LogicTrueNode { }
		[Serializable]
		public class LogicFalseNode { }
	}

}