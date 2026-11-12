using System;
using System.Diagnostics;
using Sirenix.OdinInspector;

[IncludeMyAttributes()]
[Conditional("UNITY_EDITOR")]
[PreviewField(Alignment = ObjectFieldAlignment.Center, Height = 100)]
[HideLabel]
[AssetsOnly]
public class TextureViewAttribute : Attribute
{
}