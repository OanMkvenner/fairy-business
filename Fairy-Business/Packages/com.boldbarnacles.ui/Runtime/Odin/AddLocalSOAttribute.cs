using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using MemoryPack;
using Sirenix.OdinInspector;
using Sirenix.OdinInspector.Editor;
using Sirenix.Utilities;
using UnityEngine;

[IncludeMyAttributes()]
[Conditional("UNITY_EDITOR")]
[AttributeUsage(AttributeTargets.Field)] // only allowed to be added to field members right now
public class AddLocalSOAttribute: Attribute {
    public string creationName ="";
    public Type creationType = null;
    
    public AddLocalSOAttribute(string soName = "", Type soType = null) {
        this.creationName = soName;
        this.creationType = soType;
    }
}

public class AddLocalSOProcessor<T> : OdinAttributeProcessor<T>
{
    public override void ProcessChildMemberAttributes(InspectorProperty parentProperty, MemberInfo member, List<Attribute> attributes)
    {
        if (attributes.HasAttribute<AddLocalSOAttribute>()) {
            // only allowed to be added to field members right now
            if (member.MemberType == MemberTypes.Field) {
                AddLocalSOAttribute attr = attributes.GetAttribute<AddLocalSOAttribute>();
                var creationTypeName = ((FieldInfo)member).FieldType.Name;
                if (attr.creationType != null) creationTypeName = attr.creationType.Name;
                var creationObjectName = attr.creationName != "" ? attr.creationName : member.Name + "_" + creationTypeName;
                
                attributes.Add(new OnValueChangedAttribute("@UtilitiesSO.OnValueChangeDestroyUnreferencedLocalAssets($property)"));
                var inlineButtonAttribute = new InlineButtonAttribute(
                    $"@UtilitiesSO.AddCreateLocalScriptableObject<{creationTypeName}>($property, \"{creationObjectName}\") as {creationTypeName}",
                    "+");
                inlineButtonAttribute.ShowIf = $"@{member.Name} == null";
                attributes.Add(inlineButtonAttribute);
            }
        }
    }
}
