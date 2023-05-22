using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;

#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
#endif

[System.Serializable]
public class Quest
{
    public string questName = "";
    public string questStartSceneName = string.Empty;
    public Vector3 questStartLocation = Vector3.zero;
    public bool isActive = false;
    public bool isCompleted = false;

    [HideInInspector] public UnityEvent OnQuestComplete;

    [SerializeField] public int currentObjective = 0;
    [SerializeField] private List<Objective> objectives = new List<Objective>();

    public void Activate()
    {
        if (!isCompleted)
        {
            isActive = true;
        }
    }

    public Objective GetCurrentObjective()
    {
        if (!isActive)
        {
            return null;
        }

        if (currentObjective >= objectives.Count)
        {
            return null;
        }
        else
        {
            return objectives[currentObjective];
        }
    }

    public List<Objective> GetGizmosInformation()
    {
        return objectives;
    }

    public void ResetQuest()
    {
        isActive = false;
        isCompleted = false;
        currentObjective = 0;

        for (int i = 0; i < objectives.Count; i++)
        {
            objectives[i].ResetObjective();
        }
    }

    public void UpdateCurrentObjective()
    {
        if (!isActive)
        {
            return;
        }

        currentObjective++;
        if (currentObjective > objectives.Count - 1 && !isCompleted)
        {
            Complete();
        }
    }

    private void Complete()
    {
        if (isActive && !isCompleted && currentObjective > objectives.Count - 1)
        {
            isCompleted = true;
            isActive = false;

            OnQuestComplete?.Invoke();
        }
    }
}

[System.Serializable]
public class Objective
{
    public string levelName = string.Empty;

    public int numbOfStartEvents = 0;
    public UnityEvent OnObjectiveStart;

    public enum GoalType
    {
        Gather,
        Craft,
        Location,
        Talk,
        Activate,
        Restore,
        External
    };
    public GoalType goalType = GoalType.Location;
    public string objectiveDescription = "";
    public bool isCompleted = false;

    public int numbOfCompleteEvents = 0;
    public UnityEvent OnObjectiveComplete;

    public float activationRadius = 3f;
    public Vector3 targetWorldPosition = Vector3.zero;

    public string itemName = "";
    public Item.TypeTag itemType;

    public string npcName = "";
    public TextAsset npcDialogue = null;
    public Sprite npcSprite = null;

    public int numberToCollect = 0;
    public int collectedAmount = 0;

    public string externalObjectiveName = string.Empty;

    public void ResetObjective()
    {
        isCompleted = false;
        collectedAmount = 0;
    }

    public void AddGatheringItem(string nItemName)
    {
        if (levelName != SceneManager.GetActiveScene().name)
        {
            return;
        }

        if (goalType == GoalType.Gather)
        {
            if (nItemName == itemName)
            {
                collectedAmount++;
            }

            if (collectedAmount >= numberToCollect)
            {
                Complete();
            }
        }
    }

    public void AddGatheringScrap(string nItemName, int scrapAmount)
    {
        if (levelName != SceneManager.GetActiveScene().name)
        {
            return;
        }

        if (goalType == GoalType.Gather)
        {
            if (nItemName == itemName)
            {
                collectedAmount += scrapAmount;
            }

            if (collectedAmount >= numberToCollect)
            {
                Complete();
            }
        }
    }

    public void CraftItem(string nItemName)
    {
        if (levelName != SceneManager.GetActiveScene().name)
        {
            return;
        }

        if (goalType == GoalType.Craft)
        {
            if (nItemName == itemName)
            {
                Complete();
            }
        }
    }

    public void CheckLocation(Vector3 currentPosition)
    {
        if (levelName != SceneManager.GetActiveScene().name)
        {
            return;
        }

        if (goalType == GoalType.Location)
        {
            if (Vector3.Distance(currentPosition, targetWorldPosition) < activationRadius)
            {
                Complete();
            }
        }
    }

    public KeyValuePair<TextAsset, Sprite> NPCTalkedTo(string nNPCName)
    {
        if (levelName != SceneManager.GetActiveScene().name)
        {
            return new KeyValuePair<TextAsset, Sprite>(null, null);
        }

        if (goalType == GoalType.Talk)
        {
            if (nNPCName == npcName)
            {
                Complete();
                return new KeyValuePair<TextAsset, Sprite>(npcDialogue, npcSprite);
            }
            else
            {
                return new KeyValuePair<TextAsset, Sprite>(null, null);
            }
        }

        return new KeyValuePair<TextAsset, Sprite>(null, null);
    }

    public void ActivateItem(string nItemName)
    {
        if (levelName != SceneManager.GetActiveScene().name)
        {
            return;
        }

        if (goalType == GoalType.Activate)
        {
            if (nItemName == itemName)
            {
                Complete();
            }
        }
    }

    public void RestoreItem(string nItemName)
    {
        if (levelName != SceneManager.GetActiveScene().name)
        {
            return;
        }

        if (goalType == GoalType.Restore)
        {
            if (nItemName == itemName)
            {
                Complete();
            }
        }
    }

    public void ExternalObjective(string nExternalName)
    {
        if(levelName != SceneManager.GetActiveScene().name)
        {
            return;
        }

        if (externalObjectiveName == nExternalName)
        {
            Complete();
        }
    }

    public void Complete()
    {
        if (!isCompleted)
        {
            OnObjectiveComplete?.Invoke();
            isCompleted = true;
        }
    }
}

[System.Serializable]
public struct QuestDataModel
{
    public bool isActive;
    public bool isQuestComplete;
    public List<bool> isObjectiveComplete;
    public List<int> objectiveItemsCollected;
}

public class QuestManager : SingletonMonoBehaviour<QuestManager>, ISaveable
{
    [Header("UI Variables")]
    private TMPro.TextMeshProUGUI questTextMesh;
    private GameObject questTextBackground;
    private TMPro.TextMeshProUGUI objectiveTextMesh;
    private GameObject objectiveTextBackground;
    private TMPro.TextMeshProUGUI generalInformationTextMesh;
    private GameObject questFlavorBackground;
    private TMPro.TextMeshProUGUI questFlavorTextMesh;
    private GameObject questBackgroundCollapsed;
    public float questFlavorTimer = 2f;

    private bool isQuestCollapsed = false;

    [Header("Compass UI Variables")]
    private Compass compassRef;
    public Sprite questMarker;

    [Header("Audio Variables")]
    public string questStartSFX = string.Empty;
    public string objectiveFinishSFX = string.Empty;
    public string questFinishSFX = string.Empty;

    [Header("Questing Variables")]
    public bool activateFirstOnStart = false;
    public float questActivationRadius = 3f;
    public GameObject markerGO;
    [SerializeField] private List<Quest> levelQuests = new List<Quest>();
    public int currentQuestIndex = 0;

    private string currentSceneName = string.Empty;

    public object CaptureState()
    {
        List<QuestDataModel> tempQuestDataModels = new List<QuestDataModel>();
        for (int i = 0; i < levelQuests.Count; i++)
        {
            List<int> currentObjectiveItemsCollected = new List<int>();
            List<bool> currentObjectiveCompleteStatus = new List<bool>();

            for (int j = 0; j < levelQuests[i].GetGizmosInformation().Count; j++)
            {
                currentObjectiveItemsCollected.Add(levelQuests[i].GetGizmosInformation()[j].collectedAmount);
                currentObjectiveCompleteStatus.Add(levelQuests[i].GetGizmosInformation()[j].isCompleted);
            }

            QuestDataModel currentQuestDataModel = new QuestDataModel
            {
                isActive = levelQuests[i].isActive,
                isQuestComplete = levelQuests[i].isCompleted,
                isObjectiveComplete = new List<bool>(currentObjectiveCompleteStatus),
                objectiveItemsCollected = new List<int>(currentObjectiveItemsCollected),
            };

            tempQuestDataModels.Add(currentQuestDataModel);
        }

        if (currentQuestIndex > levelQuests.Count - 1 || currentQuestIndex == -1)
        {
            return new SaveData
            {
                currentQuestIndex = -1,
                currentObjectiveIndex = -1,
                questDataModels = new List<QuestDataModel>(tempQuestDataModels),
            };
        }
        else
        {
            return new SaveData
            {
                currentQuestIndex = currentQuestIndex,
                currentObjectiveIndex = levelQuests[currentQuestIndex].currentObjective,
                questDataModels = new List<QuestDataModel>(tempQuestDataModels),
            };
        }
    }

    public void RestoreState(object state)
    {
        var saveData = (SaveData)state;

        if (!HasNewQuest())
        {
            return;
        }

        levelQuests[currentQuestIndex].isActive = false;

        markerGO.SetActive(false);

        for (int i = 0; i < saveData.questDataModels.Count; i++)
        {
            levelQuests[i].isActive = saveData.questDataModels[i].isActive;
            levelQuests[i].isCompleted = saveData.questDataModels[i].isQuestComplete;

            for (int j = 0; j < saveData.questDataModels[i].isObjectiveComplete.Count; j++)
            {
                levelQuests[i].GetGizmosInformation()[j].isCompleted = saveData.questDataModels[i].isObjectiveComplete[j];
                levelQuests[i].GetGizmosInformation()[j].collectedAmount = saveData.questDataModels[i].objectiveItemsCollected[j];
            }
        }

        currentQuestIndex = saveData.currentQuestIndex;
        if (currentQuestIndex > levelQuests.Count - 1 || currentQuestIndex == -1)
        {
            return;
        }

        levelQuests[currentQuestIndex].currentObjective = saveData.currentObjectiveIndex;
    }

    [System.Serializable]
    private struct SaveData
    {
        public int currentQuestIndex;
        public int currentObjectiveIndex;
        public List<QuestDataModel> questDataModels;
    };

    private void OnDrawGizmosSelected()
    {
        #if UNITY_EDITOR
            for (int i = 0; i < levelQuests.Count; i++)
            {
                if (levelQuests[i].questStartSceneName == EditorSceneManager.GetActiveScene().name)
                {
                    Gizmos.color = Color.green;
                    Vector3 currentStartPosition = levelQuests[i].questStartLocation;
                    Gizmos.DrawWireSphere(currentStartPosition, 0.5f);
                    Gizmos.DrawLine(currentStartPosition, currentStartPosition + Vector3.up);

                    Gizmos.DrawWireSphere(currentStartPosition, questActivationRadius);
                    #if UNITY_EDITOR
                    GUIStyle startStyle = new GUIStyle();
                    startStyle.normal.textColor = Color.green;
                    Handles.Label(currentStartPosition + Vector3.up, $"Quest: {levelQuests[i].questName}, start position.", startStyle);
                    #endif
                }


                for (int j = 0; j < levelQuests[i].GetGizmosInformation().Count; j++)
                {
                    if (levelQuests[i].GetGizmosInformation()[j].levelName != EditorSceneManager.GetActiveScene().name)
                    {
                        continue;
                    }

                    if (levelQuests[i].GetGizmosInformation()[j].goalType == Objective.GoalType.Location)
                    {
                        Gizmos.color = Color.red;
                        Vector3 currentLocationPosition = levelQuests[i].GetGizmosInformation()[j].targetWorldPosition;
                        Gizmos.DrawWireSphere(currentLocationPosition, 0.5f);
                        Gizmos.DrawLine(currentLocationPosition, currentLocationPosition + Vector3.up);

                        Gizmos.DrawWireSphere(currentLocationPosition, levelQuests[i].GetGizmosInformation()[j].activationRadius);

                    
                            GUIStyle locationStyle = new GUIStyle();
                            locationStyle.normal.textColor = Color.red;
                            Handles.Label(currentLocationPosition + Vector3.up, $"Quest: {levelQuests[i].questName}, Objective {j + 1}, target.", locationStyle);
                    }
                }
            }
        #endif
    }

    new void Awake()
    {
        if (Instance == this)
        {
            Debug.Log("Awake called on main quest manager");
        }

        if (Instance != null)
        {
            if (Instance != this)
            {
                for (int k = 0; k < levelQuests.Count; k++)
                {
                    for (int m = 0; m < levelQuests[k].GetGizmosInformation().Count; m++)
                    {
                        Instance.levelQuests[k].GetGizmosInformation()[m].OnObjectiveComplete.RemoveAllListeners();


                        Instance.levelQuests[k].GetGizmosInformation()[m].OnObjectiveStart = levelQuests[k].GetGizmosInformation()[m].OnObjectiveStart;
                        Instance.levelQuests[k].GetGizmosInformation()[m].OnObjectiveComplete = levelQuests[k].GetGizmosInformation()[m].OnObjectiveComplete;
                    }
                }
            }
        }

        base.Awake();

        markerGO.SetActive(false);
    }

    void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void Start()
    {
        if (SceneManager.GetActiveScene().name.ToLower() == GameManager.Instance.mainMenuName)
        {
            return;
        }

        compassRef.ResetQuestMarker();

        if (activateFirstOnStart)
        {
            if (currentQuestIndex == 0 && levelQuests[0].questStartSceneName == currentSceneName)
            {
                levelQuests[0].Activate();
                InitQuestInfo();
            }   
        }
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (SceneManager.GetActiveScene().name.ToLower() == GameManager.Instance.mainMenuName)
        {
            return;
        }

        currentSceneName = scene.name;

        QuestUIManager.Instance.InitUI(ref questTextMesh, ref questTextBackground, ref objectiveTextMesh, ref objectiveTextBackground, ref generalInformationTextMesh, ref questFlavorBackground, ref questFlavorTextMesh, ref questBackgroundCollapsed);
        QuestUIManager.Instance.InitCompass(ref compassRef);

        if (currentQuestIndex < levelQuests.Count)
        {
            if (levelQuests[currentQuestIndex].isActive)
            {
                levelQuests[currentQuestIndex].Activate();
                InitQuestInfo();
            }
            else
            {
                ResetQuestInfo();
            }
        }
        
        for (int i = 0; i < levelQuests.Count; i++)
        {
            if (levelQuests[i].isCompleted)
            {
                continue;
            }

            levelQuests[i].OnQuestComplete.AddListener(CurrentQuestComplete);

            for (int j = 0; j < levelQuests[i].GetGizmosInformation().Count; j++)
            {
                if (levelQuests[i].GetGizmosInformation()[j].isCompleted)
                {
                    continue;
                }

                levelQuests[i].GetGizmosInformation()[j].OnObjectiveComplete.AddListener(levelQuests[i].UpdateCurrentObjective);
                levelQuests[i].GetGizmosInformation()[j].OnObjectiveComplete.AddListener(CurrentObjectiveComplete);
            }
        }
    }

    public void ResetAllQuests()
    {
        for (int i = 0; i < levelQuests.Count; i++)
        {
            levelQuests[i].ResetQuest();
        }
    }


    private void CurrentObjectiveComplete()
    {
        markerGO.SetActive(false);

        compassRef.ResetQuestMarker();
        isQuestCollapsed = false;
        UpdateQuestText(isQuestCollapsed);
        SetObjectiveInfo();

        if (!HasNewQuest())
        {
            return;
        }

        if (!levelQuests[currentQuestIndex].isCompleted || levelQuests[currentQuestIndex].isActive)
        {
            if (objectiveFinishSFX != string.Empty)
            {
               AudioManager.instance.Play(objectiveFinishSFX);
            }
        }

        if (IsCurrentQuestActive())
        {
            CheckForPrecompletedItems();
        }
    }

    private void CheckForPrecompletedItems()
    {
        Objective currentObjective = levelQuests[currentQuestIndex].GetCurrentObjective();

        if (currentObjective != null)
        {
            if (currentObjective.goalType == Objective.GoalType.Gather)
            {
                if (currentObjective.itemType == Item.TypeTag.scrap)
                {
                    if (Inventory.Instance.amountOfScrap >= currentObjective.numberToCollect)
                    {
                        currentObjective.Complete();
                        return;
                    }
                    else
                    {
                        currentObjective.collectedAmount += Inventory.Instance.amountOfScrap;
                    }
                }
                else
                {
                    if (Inventory.Instance.Contains(currentObjective.itemName))
                    {
                        currentObjective.Complete();
                        return;
                    }
                }
            }
            else if (currentObjective.goalType == Objective.GoalType.Restore)
            {
                if (Inventory.Instance.Contains(currentObjective.itemName))
                {
                    if (Inventory.Instance.IsItemRestored(currentObjective.itemName))
                    {
                        currentObjective.Complete();
                        return;
                    }
                }
            }
        }
    }

    private void CurrentQuestComplete()
    {
        if (GetCurrentQuest() != null)
        {
            if (!GetCurrentQuest().isCompleted)
            {
                return;
            }
        }
        
        ResetQuestInfo();
        currentQuestIndex++;

        if (questFinishSFX != string.Empty)
        {
            AudioManager.instance.Play(questFinishSFX);
        }

        questFlavorTextMesh.text = $"Protocol Completed!";
        questFlavorBackground.SetActive(true);
        StartCoroutine(DeactivateQuestFlavor());
    }

    void SetObjectiveInfo()
    {
        if (!HasNewQuest())
        {
            return;
        }

        if (levelQuests[currentQuestIndex].isCompleted || !levelQuests[currentQuestIndex].isActive)
        {
            return;
        }

        Objective currentObjective = levelQuests[currentQuestIndex].GetCurrentObjective();
        if (currentObjective == null)
        {
            objectiveTextBackground.SetActive(false);
            return;
        }
        else if (currentObjective.levelName != currentSceneName)
        {
            objectiveTextBackground.SetActive(false);
            return;
        }
        else
        {
            if (!isQuestCollapsed && Time.timeScale == 1.0f)
            {
                objectiveTextBackground.SetActive(true);
            }
            else
            {
                objectiveTextBackground.SetActive(false);
            }
            
        }

        currentObjective.OnObjectiveStart?.Invoke();

        if (currentObjective.goalType == Objective.GoalType.Gather)
        {
            if (currentObjective.itemType == Item.TypeTag.scrap)
            {
                objectiveTextMesh.text = $"{currentObjective.objectiveDescription}\n • {Inventory.Instance.amountOfScrap} out of {currentObjective.numberToCollect} {currentObjective.itemName}";
            }
            else
            {
                objectiveTextMesh.text = $"{currentObjective.objectiveDescription}\n • {currentObjective.collectedAmount} out of {currentObjective.numberToCollect} {currentObjective.itemName}";
            }
            
        }
        else
        {
            if (currentObjective.goalType == Objective.GoalType.Location && !compassRef.compassGO.activeInHierarchy)
            {
                compassRef.SetQuestMarker(questMarker, currentObjective.targetWorldPosition);
            }

            objectiveTextMesh.text = currentObjective.objectiveDescription;
        }
    }

    void InitQuestInfo()
    {
        questTextMesh.text = levelQuests[currentQuestIndex].questName;
        isQuestCollapsed = false;
        UpdateQuestText(isQuestCollapsed);

        SetObjectiveInfo();
    }
    
    void SpawnMarkerGO(Vector3 position, float radius, Color color)
    {
        if (currentQuestIndex != -1)
        {
            markerGO.transform.position = position;
            markerGO.transform.localScale = new Vector3(radius * 2, radius, radius * 2);
            markerGO.GetComponent<Renderer>().material.SetColor("_MainColor", color);
            markerGO.SetActive(true);
        }
    }

    void ResetQuestInfo()
    {
        //questTextMesh.text = "";
        objectiveTextMesh.text = "";

        questTextBackground.SetActive(false);
        objectiveTextBackground.SetActive(false);
        questBackgroundCollapsed.SetActive(false);

        compassRef.ResetQuestMarker();
    }

    IEnumerator DeactivateQuestFlavor()
    {
        yield return new WaitForSeconds(questFlavorTimer);

        if (questFlavorBackground != null)
        {
            questFlavorBackground.SetActive(false);
        }
    }

    void UpdateQuestText(bool shouldShow)
    {
        questTextBackground.SetActive(!shouldShow);
        objectiveTextBackground.SetActive(!shouldShow);
        questBackgroundCollapsed.SetActive(shouldShow);
    }

    public void TryStartQuest(Vector3 playerLocation)
    {
        if (currentQuestIndex >= levelQuests.Count || currentQuestIndex == -1 || !Player.Instance.vThirdPersonInput.CanMove())
        {
            return;
        }

        if (Vector3.Distance(playerLocation, levelQuests[currentQuestIndex].questStartLocation) < questActivationRadius)
        {
            if (!levelQuests[currentQuestIndex].isActive)
            {
                levelQuests[currentQuestIndex].Activate();

                compassRef.ResetQuestMarker();
                InitQuestInfo();

                if (questStartSFX != string.Empty)
                {
                    AudioManager.instance.Play(questStartSFX);
                }

                questFlavorTextMesh.text = $"Protocol Initiated.\n{levelQuests[currentQuestIndex].questName}";
                questFlavorBackground.SetActive(true);
                StartCoroutine(DeactivateQuestFlavor());

                if (levelQuests[currentQuestIndex].GetCurrentObjective().goalType == Objective.GoalType.Gather || levelQuests[currentQuestIndex].GetCurrentObjective().goalType == Objective.GoalType.Restore)
                {
                    CheckForPrecompletedItems();
                }

                generalInformationTextMesh.text = "";
                markerGO.SetActive(false);
            }
        }
        else
        {
            if (questTextBackground.activeSelf)
            {
                questTextBackground.SetActive(false);
            }

            if (objectiveTextBackground.activeSelf)
            {
                objectiveTextBackground.SetActive(false);
            }
        }
    }

    public bool HasNewQuest()
    {
        return currentQuestIndex < levelQuests.Count;
    }

    public bool IsCurrentQuestActive()
    {
        if (currentQuestIndex > levelQuests.Count - 1 || currentQuestIndex == -1)
        {
            return false;
        }

        return levelQuests[currentQuestIndex].isActive;
    }

    public Quest GetCurrentQuest()
    {
        if (currentQuestIndex > levelQuests.Count - 1 || currentQuestIndex == -1)
        {
            return null;
        }

        return levelQuests[currentQuestIndex];
    }

    private void Update()
    {
        if (SceneManager.GetActiveScene().name.ToLower() == GameManager.Instance.mainMenuName)
        {
            return;
        }

        if (!HasNewQuest())
        {
            ResetQuestInfo();
            return;
        }

        if (Time.timeScale == 1.0f && !(questTextBackground.activeSelf || questBackgroundCollapsed.activeSelf) && IsCurrentQuestActive())
        {
            UpdateQuestText(isQuestCollapsed);
        }
        else if(Time.timeScale == 0.0f && (questTextBackground.activeSelf || questBackgroundCollapsed.activeSelf))
        {
            ResetQuestInfo();
        }

        if (Player.Instance.playerInput.actions["DisplayQuestUI"].WasPressedThisFrame() && Player.Instance.vThirdPersonInput.CanMove())
        {
            isQuestCollapsed = !isQuestCollapsed;
            UpdateQuestText(isQuestCollapsed);
        }

        if (!IsCurrentQuestActive())
        {
            if (GetCurrentQuest() != null && GetCurrentQuest().questStartSceneName == SceneManager.GetActiveScene().name)
            {
                if (!markerGO.activeInHierarchy)
                {
                    SpawnMarkerGO(levelQuests[currentQuestIndex].questStartLocation, questActivationRadius, Color.green);
                }

                if (!compassRef.compassGO.activeInHierarchy)
                {
                    compassRef.SetQuestMarker(questMarker, GetCurrentQuest().questStartLocation);
                }
            }
        }
        else
        {
            Objective currentObjective = GetCurrentQuest().GetCurrentObjective();
            if (currentObjective != null)
            {
                if (currentObjective.goalType == Objective.GoalType.Location && !markerGO.activeInHierarchy)
                {
                    SpawnMarkerGO(currentObjective.targetWorldPosition, currentObjective.activationRadius, Color.cyan);
                }
            }

            SetObjectiveInfo();
        }
    }

    private void OnDisable()
    {
        for (int i = 0; i < levelQuests.Count; i++)
        {
            levelQuests[i].OnQuestComplete.RemoveAllListeners();

            for (int j = 0; j < levelQuests[i].GetGizmosInformation().Count; j++)
            {
                levelQuests[i].GetGizmosInformation()[j].OnObjectiveComplete.RemoveAllListeners();
            }
        }
    }
}
