using System;
using System.Diagnostics;
using Sirenix.OdinInspector;
using UnityEngine;

#if UNITY_EDITOR
using Sirenix.OdinInspector.Editor;
using Sirenix.Utilities.Editor;
#endif

[IncludeMyAttributes()]
[DontApplyToListElements]
[Conditional("UNITY_EDITOR")]
[AttributeUsage(AttributeTargets.All, AllowMultiple = true)]
public class SeparatorAttribute : Attribute 
{
    public enum Location
    { 
        Top,
        Bottom,
    }

    public int TopPadding;
    public int BottomPadding;
    public Location SeperatorLocation;

    public SeparatorAttribute(Location location) => SeperatorLocation = location;
}

#if UNITY_EDITOR
public class SeparatorAttributeDrawer : OdinAttributeDrawer<SeparatorAttribute>
{
    protected override void DrawPropertyLayout(GUIContent label)
    {
        if (Attribute.SeperatorLocation == SeparatorAttribute.Location.Top)
        { 
            SirenixEditorGUI.DrawThickHorizontalSeparator(Attribute.TopPadding, Attribute.BottomPadding);
        }

        CallNextDrawer(label);

        if (Attribute.SeperatorLocation == SeparatorAttribute.Location.Bottom)
        { 
            SirenixEditorGUI.DrawThickHorizontalSeparator(Attribute.TopPadding, Attribute.BottomPadding);
        }
    }
}
#endif