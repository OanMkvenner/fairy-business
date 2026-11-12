#if UNITY_EDITOR

using UnityEngine;
using UnityEditor;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
public static class EditorWindowExtensions {
    public enum ShowMode {
        NormalWindow = 0,
        PopupMenu = 1,
        Utility = 2,
        NoShadow = 3,
        MainWindow = 4,
        AuxWindow = 5,
        Tooltip = 6,
        ModalUtility = 7
    }

    public static void ShowWithMode( this EditorWindow window, ShowMode mode, bool focus ) {
        var _EditorWindow = typeof(EditorWindow);
        var flags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;
#if UNITY_2018_3_OR_NEWER
        var showWithMode = _EditorWindow.GetMethod( "ShowPopupWithMode", flags );
        showWithMode.Invoke( window, new object[] { (int)mode, focus } );
#else
        // todo: missing gui state stuff b/c I am lazy
        // SavedGUIState oldState = SavedGUIState.Create();

        var _ContainerWindow = _EditorWindow.Assembly.GetType( "UnityEditor.ContainerWindow" );
        var _HostView = _EditorWindow.Assembly.GetType("UnityEditor.HostView");

        var cw = ScriptableObject.CreateInstance( _ContainerWindow );
        var host = ScriptableObject.CreateInstance( _HostView );


        var _actualView = _HostView.GetProperty( "actualView", flags );
        _actualView.SetValue( host, window );

        var _borderSize = host.GetProperty( "borderSize", flags );
        var offset = (RectOffset)_borderSize.GetValue( host );

        var position = window.position;
        var r = offset.Add( new Rect(position.x, position.y, position.width, position.height) );

        var _position = _ContainerWindow.GetProperty( "position", flags );
        _position.SetValue( cw, r );
        var _rootView = _ContainerWindow.GetProperty( "rootView", flags );
        _rootView.SetValue( cw, host );

        var _MakeParentsSettingsMatchMe = _EditorWindow.GetMethod( "MakeParentsSettingsMatchMe", flags );
        _MakeParentsSettingsMatchMe.Invoke( window, null );

        var _Show = _ContainerWindow.GetMethod( "Show", flags );
        _Show.Invoke( cw, new object[] { (int)mode, true, true, focus } );

        var _SetMinMaxSize = _ContainerWindow.GetMethod( "SetMinMaxSizes", flags );
        _SetMinMaxSize.Invoke( cw, new object[] { window.minSize, window.maxSize } );

        // todo
        // oldState.ApplyAndForget();
#endif
    }
}
namespace RotaryHeart.Lib.AutoComplete
{
    
    public class EditorAddItemWindow : EditorWindow
    {
        class SectionData
        {
            public string data = string.Empty;
            public bool isSection = false;
            
            public bool Equals(SectionData other)
            {
                if (ReferenceEquals(null, other)) return false;
                if (ReferenceEquals(this, other)) return true;
                return other.data == data;
            }
 
            public override bool Equals(object obj)
            {
                if (ReferenceEquals(null, obj)) return false;
                if (ReferenceEquals(this, obj)) return true;
                if (obj.GetType() != typeof(SectionData)) return false;
                return Equals((SectionData)obj);
            }
 
            public override int GetHashCode()
            {
                return data.GetHashCode();
            }
        }
        
        static class Styles
        {
            public static readonly GUIStyle Header = new GUIStyle("In BigTitle");
            public static readonly GUIStyle HeaderText = new GUIStyle();
            public static readonly GUIStyle ComponentButton = new GUIStyle("PR Label");
            public static readonly GUIStyle Background = "grey_border";
            public static readonly GUIStyle RightArrow = "AC RightArrow";
            public static readonly GUIStyle LeftArrow = "AC LeftArrow";
            public static readonly Color CurrentElementColor;
            public static readonly Color SelectionColor;
            public static readonly Color OddElementColor;
            public static readonly Color EvenElementColor;

            static Styles()
            {
                HeaderText.font = EditorStyles.boldFont;
                HeaderText.alignment = TextAnchor.MiddleCenter;
                
                ComponentButton.alignment = TextAnchor.MiddleLeft;
                ComponentButton.padding.left -= 15;
                ComponentButton.fixedHeight = 20f;
                ComponentButton.hover.textColor = Color.white;
                ComponentButton.active.background = null;

                if (EditorGUIUtility.isProSkin)
                {
                    HeaderText.normal.textColor = Color.white;
                    
                    OddElementColor = new Color(0.36f, 0.36f, 0.36f);
                    EvenElementColor = new Color(0.3f, 0.3f, 0.3f);
                }
                else
                {
                    OddElementColor = new Color(0.76f, 0.76f, 0.76f);
                    EvenElementColor = new Color(0.72f, 0.72f, 0.72f);
                }
                
                SelectionColor = new Color(0.24f, 0.49f, 0.91f);
                CurrentElementColor = new Color(0.31f, 0.54f, 0.8f);
            }
        }
        
        static public EditorAddItemWindow M_instance;

        readonly List<SectionData> m_sectionData = new List<SectionData>();
        
        Action<string> m_onItemAdded;
        string[] m_availableItems;
        string[] m_ignoreItems;
        string[] m_usedItems; 

        Vector2 m_scrollPosition;
        string m_separation = string.Empty;
        public string m_searchString = string.Empty;
        public int m_cursorIndex = 0;
        public Event m_parentEvent = null;
        string m_insideSection = string.Empty;
        string m_backText = string.Empty;
        bool m_useFullPath = true;
        bool m_allowCustom = false;
        bool m_allowEmpty = false;
        bool m_firstOpen = true;
        int m_selectionIndex = -1;
        Vector2? m_prevMousePos;

        static public bool CheckInstanceActive(){
            return M_instance != null;
        }
        static EditorAddItemWindow Instance
        {
            get
            {
                if (M_instance == null)
                {
                    M_instance = ScriptableObject.CreateInstance<EditorAddItemWindow>();
                }

                return M_instance;
            }
        }

        /// <summary>
        /// Shows the AddItemWindow dropdown
        /// </summary>
        /// <param name="position">Window position, ignoring height</param>
        /// <param name="items">Items to show</param>
        /// <param name="usedItems">Used items will be shown as disabled</param>
        /// <param name="onItemAdded">Action to call when an item is added</param>
        /// <param name="separator">Entry separator logic</param>
        /// <param name="ignore">Items to ignore (not show) from the Items array</param>
        /// <param name="backText">Text to show on the top part (back area)</param>
        /// <param name="returnFullPath">If ture, returns the full path including the separator; otherwise just the selected item</param>
        /// <param name="allowCustom">Allow custom items to be added from the search bar</param>
        /// <param name="allowEmpty">Allows to select an empty element</param>
        public static void Show(Rect position, string[] items, string[] usedItems, Action<string> onItemAdded, 
                                string separator = null, string[] ignore = null, string backText = "Select Item", bool returnFullPath = true, bool allowCustom = false, bool allowEmpty = false)
        {
            Instance.Init(position, separator, backText, items, ignore, usedItems, onItemAdded, returnFullPath, allowCustom, allowEmpty);
            Instance.Repaint();
        }
        
        void Init(Rect rect, string separator, string backText, string[] items, string[] ignore, string[] usedItems, Action<string> onItemAdded, bool returnFullPath, bool allowCustom, bool allowEmpty)
        {
            Vector2 v2 = GUIUtility.GUIToScreenPoint(new Vector2(rect.x, rect.y));
            rect.x = v2.x;
            rect.y = v2.y;
            
            M_instance.position = new Rect(rect.x, rect.yMax, rect.width, 320f);;
            //M_instance.maxSize
            var showWithMode = typeof(EditorWindow).GetMethod("ShowPopupWithMode", BindingFlags.Instance | BindingFlags.NonPublic);
            var popupMenu = 1;
            showWithMode.Invoke(M_instance, new object[] { popupMenu, false });
            EditorWindowExtensions.ShowWithMode(M_instance, EditorWindowExtensions.ShowMode.Tooltip, false);
            //ShowAsDropDown(rect, new Vector2(rect.width, 320f));
            //Focus();
            wantsMouseMove = true;

            m_separation = separator;
            m_onItemAdded = onItemAdded;
            m_availableItems = items;
            m_ignoreItems = ignore;
            m_usedItems = usedItems;
            m_backText = backText;
            m_useFullPath = returnFullPath;
            m_allowCustom = allowCustom;
            m_allowEmpty = allowEmpty;
            m_searchString = backText;
            
            CalculateVisibleElements();
        }

        void OnGUI()
        {
            GUI.Label(new Rect(0.0f, 0.0f, position.width, position.height), GUIContent.none,
                      Styles.Background);

            Rect searchArea = new Rect(2, 8, position.width - 4, EditorGUIUtility.singleLineHeight);

            if (searchArea.Contains(Event.current.mousePosition))
            {
                EditorGUIUtility.AddCursorRect(searchArea, MouseCursor.Text);
            }

            GUILayout.Space(7);
            /*
            using (new GUILayout.HorizontalScope())
            {
                GUILayout.Space(16);

                GUI.SetNextControlName("SearchTextField");
                m_searchString = GUILayout.TextField(m_searchString, GUI.skin.FindStyle("SearchTextField"));
                if (m_firstOpen)
                {
                    //GUI.FocusControl("SearchTextField");
                    m_firstOpen = false;
                }
                if (GUILayout.Button("", string.IsNullOrEmpty(m_searchString) ? GUI.skin.FindStyle("SearchCancelButtonEmpty") : GUI.skin.FindStyle("SearchCancelButton")))
                {
                    m_searchString = "";
                    //GUI.FocusControl(null);
                }
                
                GUILayout.Space(16);
            }
            */
            
            Rect rect = position;
            rect.x = +1f;
            rect.y = 30f;
            rect.height -= 30f;
            rect.width -= 2f;

            using (new GUILayout.AreaScope(rect))
            {
                DrawBackButton();
                DrawElements();
            }
        }

        void GoBack()
        {
            if (m_insideSection.EndsWith(m_separation))
                m_insideSection = m_insideSection.Substring(0, m_insideSection.LastIndexOf(m_separation));

            if (m_insideSection.Contains(m_separation))
                m_insideSection =
                    m_insideSection.Substring(0, m_insideSection.LastIndexOf(m_separation) + 1);
            else
                m_insideSection = string.Empty;

            GUI.FocusControl(null);
            CalculateVisibleElements(m_insideSection);
            m_selectionIndex = -1;
        }
        
        void DrawBackButton()
        {
            Rect rect = GUILayoutUtility.GetRect(10f, 25f);

            GUI.Label(rect, string.Empty, Styles.Header);

            if (string.IsNullOrEmpty(m_insideSection))
            {
                GUI.Label(rect, string.IsNullOrEmpty(m_searchString) ? m_backText : "Search", Styles.HeaderText);
            }
            else
            {
                if (GUI.Button(rect, m_insideSection, Styles.HeaderText))
                {
                    GoBack();
                    return;
                }

                GUI.Label(new Rect(5, 5, 15, 15), string.Empty, Styles.LeftArrow);
            }
        }
        
        public void DrawElements()
        {
            // search for currently selected word inside the string (defined by the cursor being adjacent or inside the word, and the word being surrounded by spaces or end/start of string)
            var cursorIndex = m_cursorIndex;
            var finalString = "";
            var suffix = "";
            var prefix = "";
            if (m_searchString.Length > 0){
                if (cursorIndex >= m_searchString.Length) cursorIndex = m_searchString.Length - 1;
                var idxEnd = m_searchString.IndexOf(' ', cursorIndex);
                idxEnd = idxEnd < 0 ? m_searchString.Length - 1 : idxEnd - 1;
                var reverseSearchIdx = cursorIndex <= 0? 0 : cursorIndex - 1 ;
                var idxStart = m_searchString.LastIndexOf(' ', reverseSearchIdx);
                idxStart = idxStart < 0 ? 0 : idxStart + 1;
                var finalSubstringLength = Math.Clamp(idxEnd - idxStart + 1, 0, 99999);
                finalString = m_searchString.Substring(idxStart, finalSubstringLength);
                prefix = m_searchString.Substring(0, idxStart);
                var idxEndPlus1 = idxEnd + 1;
                if (idxEndPlus1 < m_searchString.Length) suffix = m_searchString.Substring(idxEndPlus1, m_searchString.Length - (idxEndPlus1));
            }

            // then show all relevant results for this substring
            string searchString = finalString.ToLower();
            Event currentEvent = Event.current;
            if (m_parentEvent != null) {
                currentEvent = m_parentEvent;
                m_parentEvent = null;
            }
            bool moveInside = false;
            bool enter = false;

            if (m_prevMousePos == null)
                m_prevMousePos = currentEvent.mousePosition;

            if (currentEvent.isKey && currentEvent.type == EventType.KeyUp)
            {
                switch (currentEvent.keyCode)
                {
                    case KeyCode.UpArrow:
                        m_selectionIndex--;
                        GUI.FocusControl(null);
                        break;
                    
                    case KeyCode.DownArrow:
                        m_selectionIndex++;
                        GUI.FocusControl(null);
                        break;
                    
                    //case KeyCode.LeftArrow:
                    //    if (string.IsNullOrEmpty(GUI.GetNameOfFocusedControl()))
                    //        GoBack();
                    //    return;
                    //
                    //case KeyCode.RightArrow:
                    case KeyCode.Tab:
                        if (string.IsNullOrEmpty(GUI.GetNameOfFocusedControl()))
                            moveInside = true;
                        break;
                    
                    case KeyCode.KeypadEnter:
                    case KeyCode.Return:
                        enter = true;
                        if (string.IsNullOrEmpty(searchString) && m_allowEmpty)
                        {
                            m_onItemAdded(string.Empty);
                            Close();
                            return;
                        }

                        if (m_allowCustom)
                        {
                            m_onItemAdded((m_useFullPath ? m_insideSection : "") + m_searchString);
                            Close();
                            return;
                        }
                        break;
                    
                    case KeyCode.Backspace:
                        //GUI.FocusControl("SearchTextField");
                        break;
                    
                    case KeyCode.Escape:
                        Close();
                        break;
                }
            }

            m_selectionIndex = Mathf.Clamp(m_selectionIndex, -1, m_sectionData.Count - 1);

            if (m_selectionIndex < 0)
            {
                m_selectionIndex = 0;
                //GUI.FocusControl("SearchTextField");
            }
            
            using (GUILayout.ScrollViewScope scrollViewScope = new GUILayout.ScrollViewScope(m_scrollPosition))
            {
                m_scrollPosition = scrollViewScope.scrollPosition;
                bool odd = false;

                int visibleElementsIdx = -1;
                for (int i = 0; i < m_sectionData.Count; i++)
                {
                    SectionData section = m_sectionData[i];
                    
                    if (m_ignoreItems != null && m_ignoreItems.Contains(m_insideSection + section.data))
                        continue;

                    if (!string.IsNullOrEmpty(searchString) && !section.data.ToLower().Contains(searchString))
                        continue;

                    visibleElementsIdx++;

                    Rect buttonRect = GUILayoutUtility.GetRect(16f, 20f, GUILayout.ExpandWidth(true));

                    if ((m_prevMousePos.Value != currentEvent.mousePosition) && (buttonRect.Contains(currentEvent.mousePosition)))
                    {
                        m_selectionIndex = visibleElementsIdx;
                    }

                    if (m_selectionIndex == visibleElementsIdx)
                    {
                        EditorGUI.DrawRect(buttonRect, Styles.SelectionColor);
                        Repaint();
                    }
                    else if (m_usedItems != null && m_usedItems.Contains(m_insideSection + section.data))
                    {
                        EditorGUI.DrawRect(buttonRect, Styles.CurrentElementColor);
                        Repaint();
                    }
                    else
                    {
                        if (odd)
                            EditorGUI.DrawRect(buttonRect, Styles.OddElementColor);
                        else
                            EditorGUI.DrawRect(buttonRect, Styles.EvenElementColor);
                    }

                    if (section.isSection)
                    {
                        float lineHeight = EditorGUIUtility.singleLineHeight;
                        GUI.Label(new Rect(buttonRect.xMax - lineHeight, buttonRect.y, lineHeight, lineHeight),
                                  string.Empty, Styles.RightArrow);
                    }

                    if (GUI.Button(buttonRect, section.data, Styles.ComponentButton))
                    {
                        if (section.isSection)
                        {
                            moveInside = true;
                        }
                        else
                        {
                            enter = true;
                        }
                    }

                    if (enter && m_selectionIndex == visibleElementsIdx)
                    {
                        if (section.isSection)
                        {
                            moveInside = true;
                        }
                        else
                        {
                            if (section.data.Equals("(Nothing)"))
                            {
                                m_onItemAdded((m_useFullPath ? m_insideSection : string.Empty) + prefix + suffix);
                            }
                            else
                            {
                                m_onItemAdded((m_useFullPath ? m_insideSection : string.Empty) + prefix + section.data + suffix);
                            }
                            Close();
                        }
                    }
                    
                    if (moveInside && section.isSection && m_selectionIndex == visibleElementsIdx)
                    {
                        m_insideSection += section.data + m_separation;
                        CalculateVisibleElements(m_insideSection);
                        m_selectionIndex = 0;
                        break;
                    }

                    odd = !odd;
                }
            
                if (m_selectionIndex > visibleElementsIdx)
                {
                    m_selectionIndex = visibleElementsIdx;
                    //GUI.FocusControl("SearchTextField");
                }
            }
            
            m_prevMousePos = currentEvent.mousePosition;
        }

        /// <summary>
        /// Used to re calculate what items are available depending on the current section (prefix)
        /// </summary>
        /// <param name="prefix">Section where the search is</param>
        void CalculateVisibleElements(string prefix = null)
        {
            m_sectionData.Clear();
            m_selectionIndex = 0;

            if (m_allowEmpty)
            {
                m_sectionData.Add(new SectionData()
                {
                    data = "(Nothing)",
                    isSection = false
                });
            }

            foreach (string item in m_availableItems)
            {
                SectionData data = new SectionData();

                string itemToAdd = item;

                if (!string.IsNullOrEmpty(prefix))
                {
                    if (!item.StartsWith(prefix))
                        continue;
                    
                    itemToAdd = itemToAdd.Remove(0, prefix.Length);
                }

                if (!string.IsNullOrEmpty(m_separation) && itemToAdd.Contains(m_separation))
                {
                    itemToAdd = itemToAdd.Split(new[] {m_separation}, StringSplitOptions.RemoveEmptyEntries)[0];
                    
                    data.isSection = true;
                }

                data.data = itemToAdd;
                
                if (!m_sectionData.Contains(data))
                    m_sectionData.Add(data);
            }
        }
    }
}
#endif