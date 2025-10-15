using UnityEngine;
using UnityEngine.EventSystems;

#if BARNACLE_SOUNDS
using FMODUnity;
#endif
[CreateAssetMenu(fileName = "UiElementSoundSetSO", menuName = "UiElementSoundSetSO", order = 0)]
public class UiElementSoundSetSO : ScriptableObject {
#if BARNACLE_SOUNDS
    public EventReference mouseDownDefaultEvent;
    public EventReference mouseUpDefaultEvent;
    public EventReference mouseDownAlternateEvent;
    public EventReference mouseUpAlternateEvent;
#endif
}

public enum UiSoundType
{
    MouseDownDefault,
    MouseUpDefault,
    MouseDownAlternate,
    MouseUpAlternate,
}

