using Settings;
using UnityEngine;

namespace ComponentsHYBR.Utilities
{
    public class Sounds : MonobehaviourSingletonCustom<Sounds>
    {
        [SerializeField] private AudioSource[] sources;
        
        private float volume;

        private void Awake()
        {
            sources = GetComponentsInChildren<AudioSource>();
            GameSettings.OnSoundSettingChanged += SetVolumeToggle;
        }

        private void OnDestroy()
        {
            GameSettings.OnSoundSettingChanged -= SetVolumeToggle;
        }

        public bool IsPlaying(string name)
        {
            var soundItem =  gameObject.transform.Find(name);       
            if (soundItem != null)
            {
                return soundItem.gameObject.GetComponent<AudioSource>().isPlaying;
            } else {
                Debug.LogError($"Soundfile {name} not found, cant start sound.");
                return false;
            }
        }

        public void Play(string soundName)
        {
            Debug.Log(Time.timeScale + " timescale");
            foreach (AudioSource audioSource in sources)
            {
                if (audioSource.name == soundName)
                {
                    audioSource.PlayOneShot(audioSource.clip, volume);
                    return;
                }
            }

            Debug.LogError($"Sound {soundName} not found.");
        }

        public void PlayOrContinue(string name)
        {
            var soundItem =  gameObject.transform.Find(name);
            if (soundItem != null)
            {
                AudioSource src = soundItem.gameObject.GetComponent<AudioSource>();
                if (!src.isPlaying)
                {
                    src.Play();
                }
            } else {
                Debug.LogError($"Soundfile {name} not found, cant start sound.");
            }
        }

        public void Stop(string name)
        {
            var soundItem =  gameObject.transform.Find(name);
            if (soundItem != null)
            {
                soundItem.gameObject.GetComponent<AudioSource>().Stop();
            } else {
                Debug.LogError($"Soundfile {name} not found, cant stop sound.");
            }
        }

        private void SetVolumeToggle(bool isOn)
        {
            foreach (AudioSource source in sources)
            {
                source.volume = isOn ? volume : 0;
            }
        }
    }
}