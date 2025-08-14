using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

#if BARNACLE_SOUNDS
using FMODUnity;

public class UiElementSounds : MonoBehaviour {
    
    public UiElementSoundSetSO soundSet;

    Dictionary<UiSoundType, StudioEventEmitter> emitters = new();
    
    private void Start() {
        if (soundSet == null) {
            Debug.LogWarning("soundSet is null, please assign UiElementSoundSetSO", this);
            return;
        }
        emitters[UiSoundType.MouseDownDefault] = gameObject.AddComponent<StudioEventEmitter>();
        emitters[UiSoundType.MouseUpDefault] = gameObject.AddComponent<StudioEventEmitter>();
        emitters[UiSoundType.MouseDownAlternate] = gameObject.AddComponent<StudioEventEmitter>();
        emitters[UiSoundType.MouseUpAlternate] = gameObject.AddComponent<StudioEventEmitter>();
    }

    public void UpdateEmitters(){
        if (emitters[UiSoundType.MouseDownDefault].EventReference.Guid != soundSet.mouseDownDefaultEvent.Guid) emitters[UiSoundType.MouseDownDefault].EventReference = soundSet.mouseDownDefaultEvent;
        if (emitters[UiSoundType.MouseUpDefault].EventReference.Guid != soundSet.mouseUpDefaultEvent.Guid) emitters[UiSoundType.MouseUpDefault].EventReference = soundSet.mouseUpDefaultEvent;
        if (emitters[UiSoundType.MouseDownAlternate].EventReference.Guid != soundSet.mouseDownAlternateEvent.Guid) emitters[UiSoundType.MouseDownAlternate].EventReference = soundSet.mouseDownAlternateEvent;
        if (emitters[UiSoundType.MouseUpAlternate].EventReference.Guid != soundSet.mouseUpAlternateEvent.Guid) emitters[UiSoundType.MouseUpAlternate].EventReference = soundSet.mouseUpAlternateEvent;
    }
    public void PlaySound(UiSoundType uiSoundType){
        if (soundSet == null) return;
        if(!emitters.ContainsKey(uiSoundType)){
            Debug.LogError($"UiElementSoundsSO doesnt handle UiSoundType of type {uiSoundType} yet!");
            return;
        }
        UpdateEmitters();
        emitters[uiSoundType].Play();
    }
}
#else
public class UiElementSounds : MonoBehaviour {

    public void UpdateEmitters(){}
    public void PlaySound(UiSoundType uiSoundType){}
}
#endif


 