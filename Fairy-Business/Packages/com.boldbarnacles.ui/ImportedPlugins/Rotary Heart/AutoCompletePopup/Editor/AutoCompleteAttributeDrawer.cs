using System;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace RotaryHeart.Lib.AutoComplete
{
    [CustomPropertyDrawer(typeof(AutoCompleteAttribute))]
    [CustomPropertyDrawer(typeof(AutoCompleteTextFieldAttribute))]
    public class AutoCompleteAttributeDrawer : PropertyDrawer
    {
        
        string[] m_entries;

        // Draw the property inside the given rect
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            // i have no idea why every second OnGui call is from this 0 position, but i dont care. Ignore it
            if (position.x == 0 && position.y == 0) return;
            if (Event.current.isKey && Event.current.type == EventType.KeyDown && (
                    Event.current.keyCode == KeyCode.UpArrow || 
                    Event.current.keyCode == KeyCode.DownArrow || 
                    Event.current.keyCode == KeyCode.Return || 
                    Event.current.keyCode == KeyCode.KeypadEnter)){
                Event.current.Use();
                return;
            }
            // if the dropdown is currently open, catch Escape calls to not escape the editor field yet
            if (EditorAddItemWindow.CheckInstanceActive()){
                if (Event.current.isKey && Event.current.type == EventType.KeyDown && (
                        Event.current.keyCode == KeyCode.Escape)){
                    Event.current.Use();
                    return;
                }
            }

            if (m_entries == null)
            {
                AutoCompleteAttribute attribute = System.Attribute.GetCustomAttribute(fieldInfo, typeof(AutoCompleteAttribute)) as AutoCompleteAttribute;
                m_entries = attribute.GetEntries();
            }

            property.stringValue = AutoCompleteTextField.EditorGUI.AutoCompleteTextField(property, position, label, property.stringValue, GUI.skin.textField, m_entries, "Type something here");
        }
    }
}