using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

public class MusicManager : MonoBehaviour
{
    // Start is called before the first frame update
    private static MusicManager instance;
    public AudioSource intro;
    public AudioSource mainLoop;
    public AudioSource musicSource;
    public AudioMixerGroup musicGroup;
    public Song[] songList;

    private void Awake()
    {
        if (instance != null)
        {
            Destroy(gameObject);
            return;
        }

        instance = this;

        if (this.gameObject == gameObject.transform.root.gameObject)
        {
            DontDestroyOnLoad(gameObject);
        }
    }

    void Start()
    {
        if (intro.clip != null && mainLoop.clip != null && musicSource.clip == null)
        {
            Intro();
        }

        else
        {
            intro.Stop();
            mainLoop.Stop();
            musicSource.Play();
        }
    }

    public static MusicManager Get()
    {
        return instance;
    }

    public void CrossFade(AudioClip newTrack, float fadeTime = 1.0f)
    {
        AudioSource newAudioSource = musicSource.gameObject.AddComponent<AudioSource>();
        newAudioSource.volume = 0.0f;
        newAudioSource.outputAudioMixerGroup = musicGroup;
        newAudioSource.loop = true;
        newAudioSource.clip = newTrack;
        newAudioSource.Play();
        StartCoroutine(CrossFadeCoroutine(newAudioSource, fadeTime));
    }

    public void CrossFadeIntro(AudioClip newIntro, AudioClip newMainLoop, float fadeTime = 1.0f)
    {
        AudioSource newIntroSource = intro.gameObject.AddComponent<AudioSource>();
        AudioSource newMainLoopSource = mainLoop.gameObject.AddComponent<AudioSource>();
        newIntroSource.volume = 0.0f;
        newIntroSource.outputAudioMixerGroup = musicGroup;
        newIntroSource.loop = false;
        newIntroSource.clip = newIntro;
        newMainLoopSource.volume = 0.0f;
        newMainLoopSource.outputAudioMixerGroup = musicGroup;
        newMainLoopSource.loop = true;
        newMainLoopSource.clip = newMainLoop;
        StartCoroutine(CrossFadeIntroCoroutine(newIntroSource, newMainLoopSource, fadeTime));
    }

    public void Intro()
    {
        musicSource.Stop();
        double introDuration = (double)intro.clip.samples / intro.clip.frequency;
        double startTime = AudioSettings.dspTime;
        intro.PlayScheduled(startTime);
        mainLoop.PlayScheduled(startTime + introDuration);
    }

    public void CrossFadeTo(string name)
    {
        Song s = Array.Find(songList, song => song.name == name);
        CrossFade(s.clip);
    }

    public void CrossFadeTo(string name, float fadeTime)
    {
        Song s = Array.Find(songList, song => song.name == name);
        CrossFade(s.clip, fadeTime);
    }

    public void CrossFadeTo(AudioClip audioClip)
    {
        CrossFade(audioClip);
    }

    public void CrossFadeIntroTo(string name, string name2)
    {
        Song s = Array.Find(songList, song => song.name == name);
        Song s2 = Array.Find(songList, song => song.name == name2);
        CrossFadeIntro(s.clip, s2.clip);

    }

    public void SetMusic(string name)
    {
        Song s = Array.Find(songList, song => song.name == name);
        if(s == null || s.clip == null)
        {
            musicSource.clip = Resources.Load<AudioClip>(name);
        }
        else
        musicSource.clip = s.clip;
    }


    public void SetMusic(AudioClip audioClip)
    {
        musicSource.clip = audioClip;
    }

    public void SetIntroMain(string name1, string name2)
    {
        Song s1 = Array.Find(songList, song => song.name == name1);
        Song s2 = Array.Find(songList, song => song.name == name2);
        intro.clip = s1.clip;
        mainLoop.clip = s2.clip;
    }

    IEnumerator CrossFadeCoroutine(AudioSource newSource, float fadeTime)
    {
        float t = 0.0f;
        float initialVolume = musicSource.volume;
        float intialIntroVolume = intro.volume;
        float initialLoopVolue = mainLoop.volume;

        while (t < fadeTime)
        {
            musicSource.volume = Mathf.Lerp(initialVolume, 0.0f, t / fadeTime);
            intro.volume = Mathf.Lerp(intialIntroVolume, 0.0f, t / fadeTime);
            mainLoop.volume = Mathf.Lerp(initialLoopVolue, 0.0f, t / fadeTime);
            newSource.volume = Mathf.Lerp(0.0f, 1.0f, t / fadeTime);

            t += Time.deltaTime;
            yield return null;
        }

        newSource.volume = 1.0f;
        Destroy(musicSource);
        musicSource = newSource;
        intro.Stop();
        intro.volume = 1.0f;
        mainLoop.Stop();
        mainLoop.volume = 1.0f;
    }

    IEnumerator CrossFadeIntroCoroutine(AudioSource newSource, AudioSource newMain, float fadeTime)
    {
        float t = 0.0f;
        float initialVolume = musicSource.volume;
        float intialIntroVolume = intro.volume;
        float initialLoopVolue = mainLoop.volume;

        while (t < fadeTime)
        {
            musicSource.volume = Mathf.Lerp(initialVolume, 0.0f, t / fadeTime);
            intro.volume = Mathf.Lerp(intialIntroVolume, 0.0f, t / fadeTime);
            mainLoop.volume = Mathf.Lerp(initialLoopVolue, 0.0f, t / fadeTime);
            newSource.volume = Mathf.Lerp(0.0f, 1.0f, t / fadeTime);

            t += Time.deltaTime;
            yield return null;
        }

        newSource.volume = 1.0f;
        newMain.volume = 1.0f;
        Destroy(intro);
        intro = newSource;
        Destroy(mainLoop);
        mainLoop = newMain;
        musicSource.Stop();
        musicSource.volume = 1.0f;

        double introDuration = (double)intro.clip.samples / intro.clip.frequency;
        double startTime = AudioSettings.dspTime;
        intro.PlayScheduled(startTime);
        mainLoop.PlayScheduled(startTime + introDuration);
    }
}
