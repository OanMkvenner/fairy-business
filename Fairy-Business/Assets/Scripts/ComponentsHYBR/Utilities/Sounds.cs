using System.Collections;
using UnityEngine;


public class Sounds : MonoBehaviour
{
    public static Sounds instance { get; private set; }

    private void Awake() {
        instance = this;
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

    public void Play(string name)
    {
        var soundItem =  gameObject.transform.Find(name);
        if (soundItem != null)
        {
            soundItem.gameObject.GetComponent<AudioSource>().Play();
        } else {
            Debug.LogError($"Soundfile {name} not found, cant start sound.");
        }
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

}