using UnityEngine;
using UnityEngine.EventSystems;

#if BARNACLE_SOUNDS
using FMODUnity;
[CreateAssetMenu(fileName = "UiElementSoundSetSO", menuName = "UiElementSoundSetSO", order = 0)]
public class UiElementSoundSetSO : ScriptableObject {
    public EventReference mouseDownDefaultEvent;
    public EventReference mouseUpDefaultEvent;
    public EventReference mouseDownAlternateEvent;
    public EventReference mouseUpAlternateEvent;

}
#endif

public enum UiSoundType
{
    MouseDownDefault,
    MouseUpDefault,
    MouseDownAlternate,
    MouseUpAlternate,
}

