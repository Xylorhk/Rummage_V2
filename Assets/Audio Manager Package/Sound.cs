using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;
[System.Serializable]
public class Sound
{

    public string name;

    public string fileName;

    public AudioClip clip;

    [HideInInspector]
    public AudioSource source;

    public enum soundType{midPriority, highPriority, UI};
    public soundType SoundType;
    public bool shouldLoop = false;

    [Range(0f, 1.0f)]
    public float volume;


}