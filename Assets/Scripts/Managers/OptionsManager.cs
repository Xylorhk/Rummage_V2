using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class OptionsManager : SingletonMonoBehaviour<OptionsManager>, ISaveable
{
    public object CaptureState()
    {   
        return new SaveData
        {
            masterVolume = masterVolSlider.value,
            musicVolume = musicVolSlider.value,
            soundVolume = soundVolSlider.value,
        };
    }

    public void RestoreState(object state)
    {
        var saveData = (SaveData)state;

        masterVolSlider.value = saveData.masterVolume;
        musicVolSlider.value = saveData.musicVolume;
        soundVolSlider.value = saveData.soundVolume;

        AudioManager.Get().mixerControl.SetMasterVolume(saveData.masterVolume);
        AudioManager.Get().mixerControl.SetMusicVolume(saveData.musicVolume);
        AudioManager.Get().mixerControl.SetGameSoundsVolume(saveData.soundVolume);
    }

    [System.Serializable]
    private struct SaveData
    {
        public float masterVolume;
        public float musicVolume;
        public float soundVolume;
    }

    public GameObject optionsBackgroundPanel;
    public GameObject optionsPanel;
    public GameObject controlsPanel;

    [Header("Volume Options")]
    public Slider masterVolSlider;
    public Slider musicVolSlider;
    public Slider soundVolSlider;

    private void Start()
    {
        masterVolSlider.onValueChanged.AddListener(delegate { AudioManager.Get().mixerControl.SetMasterVolume(masterVolSlider.value); });
        musicVolSlider.onValueChanged.AddListener(delegate { AudioManager.Get().mixerControl.SetMusicVolume(musicVolSlider.value); });
        soundVolSlider.onValueChanged.AddListener(delegate { AudioManager.Get().mixerControl.SetGameSoundsVolume(soundVolSlider.value); });

        soundVolSlider.onValueChanged.AddListener(delegate { AudioManager.Get().Play("TestSound"); });

        masterVolSlider.onValueChanged.AddListener(delegate { GameManager.Instance.SaveOptionsManager(); });
        musicVolSlider.onValueChanged.AddListener(delegate { GameManager.Instance.SaveOptionsManager(); });
        soundVolSlider.onValueChanged.AddListener(delegate { GameManager.Instance.SaveOptionsManager(); });
    }

    public void ToggleOptionsPanel(bool toggle)
    {
        optionsBackgroundPanel.SetActive(toggle);

        if (controlsPanel.activeSelf)
        {
            optionsPanel.SetActive(true);
            controlsPanel.SetActive(false);
        }
    }

    private void Update()
    {
        if (AudioManager.Get().mixerControl.GetMasterVolume() != Mathf.Log(masterVolSlider.value) * 20)
        {
            AudioManager.Get().mixerControl.SetMasterVolume(masterVolSlider.value);
        }

        if (AudioManager.Get().mixerControl.GetMusicVolume() != Mathf.Log(musicVolSlider.value) * 20)
        {
            AudioManager.Get().mixerControl.SetMusicVolume(musicVolSlider.value);
        }

        if (AudioManager.Get().mixerControl.GetSoundVolume() != Mathf.Log(soundVolSlider.value) * 20)
        {
            AudioManager.Get().mixerControl.SetGameSoundsVolume(soundVolSlider.value);
        }
    }
}
