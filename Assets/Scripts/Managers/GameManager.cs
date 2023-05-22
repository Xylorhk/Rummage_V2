using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;

#if UNITY_EDITOR
using UnityEditor;
#endif

public class GameManager : SingletonMonoBehaviour<GameManager>, ISaveable
{
    [Header("Save Files")]
    public string gameManagerSaveFile;
    public string optionsSaveFile;
    public string levelManagerSaveFile;
    public string questManagerSaveFile;
    public string sceneSaveFile;

    [Header("Scenes")]
    public string mainMenuName = string.Empty;
    public List<string> sceneNames = new List<string>();

    [SerializeField]
    public int currentSavedScene = 0;
    private string nextSceneSpawnLocationName = string.Empty;
    private bool hasLoadedInitially = false;

    [Header("Player Input Manager")]
    public PlayerInputManager playerInputManager;

    public object CaptureState()
    {
        return new SaveData
        {
            savedSceneIndex = currentSavedScene,
            spawnLocationName = nextSceneSpawnLocationName,
        };
    }

    public void RestoreState(object state)
    {
        var saveData = (SaveData)state;

        currentSavedScene = saveData.savedSceneIndex;
        nextSceneSpawnLocationName = saveData.spawnLocationName;
    }

    [Serializable]
    private struct SaveData
    {
        public int savedSceneIndex;
        public string spawnLocationName;
    }

    new void Awake()
    {
        base.Awake();

        mainMenuName = mainMenuName.ToLower();
        for (int i = 0; i < sceneNames.Count; i++)
        {
            sceneNames[i] = sceneNames[i].ToLower();
        }

        DontDestroyOnLoad(this.gameObject);
    }

    [ContextMenu("ResetSaveFile")]
    public void ResetSaveFiles()
    {
        SaveSystem.ResetSaveFile(gameManagerSaveFile);
        SaveSystem.ResetSaveFile(levelManagerSaveFile);
        SaveSystem.ResetSaveFile(questManagerSaveFile);
        SaveSystem.ResetSaveFile(sceneSaveFile);
    }

    [ContextMenu("ResetOptionsFile")]
    public void ResetOptionsFile()
    {
        SaveSystem.ResetSaveFile(optionsSaveFile);
    }

    void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode sceneMode)
    {
        SaveSystem.Load(optionsSaveFile);

        if (scene.name == mainMenuName && !hasLoadedInitially)
        {
            SaveSystem.Load(gameManagerSaveFile);
            SaveSystem.Load(questManagerSaveFile);
            hasLoadedInitially = true;
        }
        else
        {
            if (scene.name != mainMenuName)
            {
                SaveSystem.Load(levelManagerSaveFile);
                SaveSystem.Load(sceneSaveFile);
            }
        }
    }

    public void SetNextSpawnLocationName(string spawnLocationName)
    {
        nextSceneSpawnLocationName = spawnLocationName;
    }

    public string GetSpawnLocationName()
    {
        return nextSceneSpawnLocationName;
    }

    public void SaveGameManager()
    {
        SaveSystem.Save(gameManagerSaveFile);
    }

    public void SaveOptionsManager()
    {
        SaveSystem.Save(optionsSaveFile);
    }

    public void SaveScene()
    {
        for (int i = 0; i < sceneNames.Count; i++)
        {
            if (sceneNames[i].CompareTo(SceneManager.GetActiveScene().name.ToLower()) == 0)
            {
                currentSavedScene = i;
                break;
            }
        }

        SaveSystem.Save(sceneSaveFile);
        SaveSystem.Save(levelManagerSaveFile);
        SaveSystem.Save(questManagerSaveFile);
    }

    public bool HasSaveData()
    {
        bool returnValue = SaveSystem.DoesFileExist(gameManagerSaveFile) || SaveSystem.DoesFileExist(levelManagerSaveFile) || SaveSystem.DoesFileExist(sceneSaveFile) || SaveSystem.DoesFileExist(questManagerSaveFile);
        return returnValue;
    }

    public string GetLastSavedScene()
    {
        return sceneNames[currentSavedScene];
    }

    public void QuitGame()
    {
        if (SceneManager.GetActiveScene().name.ToLower() != mainMenuName)
        {
            SaveScene();
            SaveGameManager();
        }
        else
        {
            SaveGameManager();
        }

        if (Application.isEditor)
        {
            #if UNITY_EDITOR
                if (EditorApplication.isPlaying)
                {
                    EditorApplication.isPlaying = false;
                }
            #endif
        }
        else
        {
            Application.Quit();
        }
    }

    private void Update()
    {
        //Debug.Log(nextSceneSpawnLocationName);
    }

    //private void OnApplicationQuit()
    //{
    //    if (Application.isEditor)
    //    {
    //        QuitGame();
    //    }
    //}

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }
}
