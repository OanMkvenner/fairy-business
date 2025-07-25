using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Reflection;

using UnityEditor;
using System.Linq;
using System;

namespace XNode.UiStateGraph {
	[NodeWidth(155), NodeTint(70, 140, 40)]
	public class TimerNode : UiStateNode, UiTimerTick {

		[Output] public ButtonNode timedContinue;

		[HideInInspector]
		private float timer;

		public float seconds = 1.0f;

		override public void OnEnter(UiStateNode originNode, NodePort incomingPort) {
			// actual timer stuff is done passively whenever the node is active. 
			// No need to initialize or start anything but the base.OnEnter()
			base.OnEnter(originNode, incomingPort);
		}

		void OnTimerOver(){
			MoveAlongPort("timedContinue");
		}

		// when the timer reaches "seconds" it starts OnTimerOver() but keeps 
		// leftover time fractions standing. If it gets activated again before 
		// the next Tick it will continue with that leftover, if not it will reset.
		// This allows accurate pulsing when feeding into itself
		public void Tick(float deltaTime) {
			if (!active) {
				timer = 0;
				return;
			}

			timer += deltaTime;
			if (timer > seconds) {
				timer -= seconds;
				OnTimerOver();
			}
		}
    	
	}
}