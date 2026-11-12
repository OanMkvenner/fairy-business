using System;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace RotaryHeart.Lib.AutoComplete
{
    public static class AutoCompleteTextField
    {
#if UNITY_EDITOR
        

        /// <summary>
        /// Uses UnityEditor.EditorGUI to draw the text field
        /// </summary>
        public static class EditorGUI
        {
            /// <summary>
            /// Make a TextField that has an <paramref name="AutoCompleteWindow"/> logic for selecting options.
            /// </summary>
            /// <param name="position">Rectangle on the screen to use for the text field</param>
            /// <param name="label">Optional label to display in front of the text field</param>
            /// <param name="text">The text to edit</param>
            /// <param name="style">Optional GUIStyle</param>
            /// <param name="entries">Entries to display</param>
            /// <param name="hint">Hint information to show, if any</param>
            /// <param name="allowCustom">Should the system allow custom entries</param>
            /// <param name="allowEmpty">Should the system add a Nothing element and allow returning an empty string</param>
            /// <param name="separator">The separator to use for grouping selections</param>
            /// <param name="returnFullPath">Should the full path be returned when an option is selected</param>
            /// <returns>Selected value from autocomplete window</returns>
            public static string AutoCompleteTextField(SerializedProperty property, Rect position, GUIContent label, string text, GUIStyle style, string[] entries, string hint, bool allowCustom = false, bool allowEmpty = true, string separator = "/", bool returnFullPath = false)
            {

                //used for checking focus on textfield element
                UnityEngine.GUI.SetNextControlName("CheckFocus");
                string value = UnityEditor.EditorGUI.TextField(position, label, text, style);
                
                // hack to get the TextField TextEditor (its kinda difficult to get because private static and all that jazz. But Odin does it this way as well as far as i could see!)
                Type editorGUIType = typeof(UnityEditor.EditorGUI);
                FieldInfo fi = editorGUIType.GetField("s_RecycledEditorInternal", BindingFlags.NonPublic | BindingFlags.Static);
                if (fi == null) {
                    // should be findable here: https://github.com/Unity-Technologies/UnityCsReference/blob/master/Editor/Mono/EditorGUI.cs
                    Debug.LogError("s_RecycledEditorInternal not found in UnityEditor.EditorGUI. They probably changed again how its called...");
                    return value;
                }
                TextEditor te = fi.GetValue(null) as TextEditor;
                
                //Hack to avoid hint being drawn on the wrong location for attribute drawers
                //AutoCompleteBase.M_MyStyle.normal.textColor = Color.clear;
                //UnityEngine.GUI.enabled = false;
                //UnityEditor.EditorGUI.TextField(position, " ", " ", AutoCompleteBase.M_MyStyle);
                //UnityEngine.GUI.enabled = true;

                //Display the hint
                //if (string.IsNullOrEmpty(text) && !string.IsNullOrEmpty(hint))
                //{
                //    //Nothing is typed, show a hint to the user
                //    UnityEngine.GUI.enabled = false;
                //
                //    UnityEditor.EditorGUI.TextField(position, label, " " + hint, UnityEngine.GUI.skin.label);
                //    UnityEngine.GUI.enabled = true;
                //}

                string result = AutoCompleteBase._AutoCompleteLogic(property, ref te, position, label, value, entries, allowCustom, allowEmpty, true, null, separator, returnFullPath);
                //UnityEngine.GUI.changed = result != text;
                return result;
            }
        }
#endif
    }
    public interface IStyle
    {
        GUIStyle Header { get; }
        GUIStyle HeaderText { get; }
        GUIStyle ComponentButton { get; }
        Color BackgroundColor { get; }
        Color SelectionColor { get; }
        Color CurrentElementColor { get; }
        Color OddElementColor { get; }
        Color EvenElementColor { get; }
        Texture2D Background { get; }
        Texture2D WhiteTexture { get; }
        GUIStyle SearchBarStyle { get; }
        GUIStyle ClearSearchButtonStyle { get; }
    }

    internal class AutoCompleteBase
    {
#if UNITY_EDITOR
        //This is a hack to prevent saving the return value to the wrong UI element
        internal static Vector2 M_returnedScreenPos;

        internal static bool savedCursor = false;
        internal static int savedCursorPos = -1;
        internal static string lastCheckedString = "";
        internal static Vector2 openedScreenPos = Vector2.zero; // used to remember which textField is currently in use (in case multiple are in the same inspector)
        //Saves the returned value (if any was selected)
        internal static readonly GUIContent M_ReturnedContent = new GUIContent();
        internal static readonly GUIStyle M_MyStyle = new GUIStyle(GUIStyle.none);
        internal static readonly GUIStyle M_dropdownStyle = new GUIStyle("DropDownButton");

        static bool M_returnedValue;

        static AddItemWindow M_addItemWindow;

        /// <summary>
        /// Logic for the auto complete draw on text field focus
        /// </summary>
        /// <param name="position">TextField position</param>
        /// <param name="label">Label from the TextField</param>
        /// <param name="text">Value from the TextField</param>
        /// <param name="entries">Entries to display</param>
        /// <param name="allowCustom">Should the system allow custom entries</param>
        /// <param name="allowEmpty">Should the system allow to select empty objects</param>
        /// <param name="fromEditor">Is this being called from editor inspector</param>
        /// <param name="windowStyle">The style to use for the window</param>
        /// <param name="separator">The separator to use for grouping sections</param>
        /// <param name="returnFullPath">Should the full path be returned when an option is selected</param>
        /// <returns>Selected value from autocomplete window</returns>
        internal static string _AutoCompleteLogic(SerializedProperty property, ref TextEditor te, Rect position, GUIContent label, string text, string[] entries, bool allowCustom, bool allowEmpty, bool fromEditor,
            IStyle windowStyle, string separator, bool returnFullPath)
        {
            //Used to draw the window
            Rect lastRect = position;
            Vector2 myScreenPos = GUIUtility.GUIToScreenPoint(new Vector2(position.x, position.y));
            if (openedScreenPos != Vector2.zero && openedScreenPos != myScreenPos){
                // this is not our business, another field is currently interacting
                if (!GUI.GetNameOfFocusedControl().Equals("CheckFocus")){
                    return text;
                } else {
                    // so we are not the right editorField, BUT we HAVE focus...
                    // reset everything?
                    EditorAddItemWindow.M_instance.Close();
                    openedScreenPos = Vector2.zero;
                    lastCheckedString = text;
                    savedCursor = false;
                }
            }

            //The system returned a value, need to check if this UI element is the one that called
            if (M_returnedValue && M_returnedScreenPos == myScreenPos)
            {
                M_returnedValue = false;
                string val = M_ReturnedContent.text;
                M_ReturnedContent.text = "";
                M_returnedScreenPos = Vector2.zero;
                return val;
            }
            
            if (!GUI.GetNameOfFocusedControl().Equals("CheckFocus") && EditorAddItemWindow.CheckInstanceActive()){
                EditorAddItemWindow.M_instance.Close();
                return text;
            }

            if (EditorAddItemWindow.CheckInstanceActive()){
                EditorAddItemWindow.M_instance.m_cursorIndex = te.cursorIndex;
                EditorAddItemWindow.M_instance.m_searchString = text;
                Event currentEvent = Event.current;
                if (currentEvent.isKey && currentEvent.type == EventType.KeyUp){
                    Event copiedEvent = new ();
                    copiedEvent.mousePosition = currentEvent.mousePosition;
                    copiedEvent.type = currentEvent.type;
                    copiedEvent.keyCode = currentEvent.keyCode;
                    EditorAddItemWindow.M_instance.m_parentEvent = copiedEvent;
                    EditorAddItemWindow.M_instance.Repaint();
                }
            }

            bool editingStringHasChanged = false;
            bool invalidCursorPos = false;
            invalidCursorPos = savedCursor && (te.SelectedText.Length > 0);
            if (GUI.GetNameOfFocusedControl().Equals("CheckFocus") && !EditorAddItemWindow.CheckInstanceActive())
            {
                if (invalidCursorPos){
                    var targetCursorPos = AutoCompleteBase.savedCursorPos;
                    if (targetCursorPos >= te.text.Length) targetCursorPos = te.text.Length;
                    targetCursorPos = te.text.IndexOf(' ', targetCursorPos);
                    if (targetCursorPos < 0) targetCursorPos = te.text.Length;
                    
                    te.cursorIndex = targetCursorPos;
                    te.selectIndex = targetCursorPos;
                    HandleUtility.Repaint(); // repaint this shit until it finally properly sets the cursor...
                } else {
                    // either we dont have a cursor saved, or it has finally been loaded correctly
                    savedCursor = false;
                    openedScreenPos = Vector2.zero;
                }
                // if we have focus, not already an open window, and the text has changed, remember that change for later
                editingStringHasChanged = text != lastCheckedString;
                if (editingStringHasChanged) lastCheckedString = text;
            }
            //Only display the system if the text field is focused
            if (GUI.GetNameOfFocusedControl().Equals("CheckFocus") && !EditorAddItemWindow.CheckInstanceActive() && editingStringHasChanged)
            {
                openedScreenPos = myScreenPos;
                //New position for this window
                Rect newRect = lastRect;

                //Remove focus
                //GUI.FocusControl(null);

                if (fromEditor)
                {
                    if (!string.IsNullOrEmpty(label.text))
                    {
                        float offset = UnityEditor.EditorGUIUtility.labelWidth - ((UnityEditor.EditorGUI.indentLevel) * 15);
                        newRect.x += offset;
                        newRect.width -= offset;
                    }

                    newRect.x += UnityEditor.EditorGUI.indentLevel * 15;
                    newRect.width -= UnityEditor.EditorGUI.indentLevel * 15;
                    int currentCursorPos = te.cursorIndex;
                    EditorAddItemWindow.Show(newRect, entries, new []{ text }, s =>
                    {
                        SetPropertyContent(property, s);
                    }, separator, returnFullPath: returnFullPath, allowCustom: allowCustom, allowEmpty: allowEmpty);
                    EditorAddItemWindow.M_instance.m_searchString = text;
                }
            }

            return text;
        }
        internal static void SetPropertyContent(SerializedProperty property, string newString){
            UnityEditor.EditorGUI.FocusTextInControl("CheckFocus");

            Type editorGUIType = typeof(UnityEditor.EditorGUI);
            FieldInfo fi = editorGUIType.GetField("s_RecycledEditorInternal", BindingFlags.NonPublic | BindingFlags.Static);
            if (fi == null) {
                // should be findable here: https://github.com/Unity-Technologies/UnityCsReference/blob/master/Editor/Mono/EditorGUI.cs
                Debug.LogError("s_RecycledEditorInternal not found in UnityEditor.EditorGUI. They probably changed again how its called...");
            }
            TextEditor te = fi.GetValue(null) as TextEditor;

            AutoCompleteBase.savedCursorPos = te.cursorIndex;
            savedCursor = true;

            property.stringValue = newString;
            EditorUtility.SetDirty( property.serializedObject.targetObject );
            property.serializedObject.ApplyModifiedProperties();
            property.serializedObject.Update();

            lastCheckedString = newString;

            //M_ReturnedContent.text = s;
            //M_returnedValue = true;
            //M_returnedScreenPos = myScreenPos;
        }
        
#endif
    }
}