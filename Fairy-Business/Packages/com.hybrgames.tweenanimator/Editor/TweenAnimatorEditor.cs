using UnityEditor;
using UnityEngine;
using System.Linq;
using System;

namespace XNodeEditor.UiStateGraph {
	// overload Inspector
	[CustomEditor(typeof(TweenAnimator), true)]	
    [CanEditMultipleObjects]
    public class TweenAnimatorInspector : Editor {
		private TweenAnimator animator;
        public override void OnInspectorGUI() {
			// Initialization
			if (animator == null) {
				animator = target as TweenAnimator;
			}
            serializedObject.Update();

            //GUILayout.Space(EditorGUIUtility.singleLineHeight);
            //GUILayout.Label("Animator data", "BoldLabel");

			string potentialError = animator.GetPotentialError();
			if (potentialError == "") {
				SerializedProperty reactToAnyChildButton = serializedObject.FindProperty("reactToAnyChildButton");
				EditorGUILayout.PropertyField(reactToAnyChildButton, new GUIContent("React to child buttons"), true);
				SerializedProperty reactToParentButton = serializedObject.FindProperty("reactToParentButton");
				EditorGUILayout.PropertyField(reactToParentButton, new GUIContent("React to direct parent button"), true);
				for (int i = 0; i < 2; i++)
				{
					bool buttonClickedMode = i == 0;
					bool highlightingMode = i == 1;
					TweenAnimator.ValueSet valueSet = null;
					TweenAnimator.TweenMode tweenMode = TweenAnimator.TweenMode.None;
					if (buttonClickedMode)
					{
						SerializedProperty btnClickTweenMode = serializedObject.FindProperty("btnClickTweenMode");
						EditorGUILayout.PropertyField(btnClickTweenMode, new GUIContent("OnClick Tween Mode"), true);
						valueSet = animator.onClickValues;
						tweenMode = animator.btnClickTweenMode;
					}
					if (highlightingMode)
					{
						SerializedProperty autoHighlight = serializedObject.FindProperty("autoHighlight");
						EditorGUILayout.PropertyField(autoHighlight, new GUIContent("Auto Highlight"), true);
						SerializedProperty highlightTweenMode = serializedObject.FindProperty("highlightTweenMode");
						EditorGUILayout.PropertyField(highlightTweenMode, new GUIContent("Highlighting Tween Mode"), true);
						valueSet = animator.highlightValues;
						tweenMode = animator.highlightTweenMode;
					}

					if (animator.GetAnyFieldsRequired(tweenMode)){
						string[] requiredDataFields = animator.GetRequiredDataFields(tweenMode);
						if(requiredDataFields.Length > 0)
							valueSet.val_1 = EditorGUILayout.FloatField(new GUIContent(requiredDataFields[0]), valueSet.val_1);
						if(requiredDataFields.Length > 1)
							valueSet.val_2 = EditorGUILayout.FloatField(new GUIContent(requiredDataFields[1]), valueSet.val_2);
						if(requiredDataFields.Length > 2)
							valueSet.val_3 = EditorGUILayout.FloatField(new GUIContent(requiredDataFields[2]), valueSet.val_3);
						if(requiredDataFields.Length > 3)
							valueSet.val_4 = EditorGUILayout.FloatField(new GUIContent(requiredDataFields[3]), valueSet.val_4);

						valueSet.easeMode = (DG.Tweening.Ease) EditorGUILayout.EnumPopup(new GUIContent("Easing Mode"), valueSet.easeMode);
						valueSet.duration = EditorGUILayout.FloatField(new GUIContent("Over duration (s)"), valueSet.duration);
					}
				}


			} else {
				GUIStyle style = new GUIStyle(EditorStyles.textArea);
        		style.wordWrap = true;
				EditorGUILayout.LabelField(potentialError, style);
			}
            serializedObject.ApplyModifiedProperties();
        }
    }
}