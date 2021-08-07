using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// To be continued, sound design
/// </summary>
public class WorldAudio : Singleton<WorldAudio>
{
    public GameObject WorldAudioPrefab;

    public AudioSource GetWorldAudio()
    {
        var a = Poolable.TryGetPoolable<Poolable>(WorldAudioPrefab);
        return a.GetComponent<AudioSource>();
    }
}
