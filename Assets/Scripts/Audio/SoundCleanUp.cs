using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoundCleanUp : MonoBehaviour
{
    private AudioSource source;
    private void Awake()
    {
        source = GetComponent<AudioSource>();
    }

    void Update()
    {
        if (gameObject.activeInHierarchy && source != null && !source.isPlaying)
        {
            gameObject.SetActive(false);
        }
    }
}
