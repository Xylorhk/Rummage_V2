using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Playables;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class StartMenu : MonoBehaviour
{
    [Header("Main Menu Functionality")]
    public GameObject MainMenuPanel;
    public Button continueButton;
    public List<Button> mainMenuButtons = new List<Button>();
    public GameObject warningPanel;

    private bool continueButtonOriginalState = false;
    private bool warningCalled = false;

    [Header("Opening Cutscene Properties")]
    public PlayableDirector mainTimelineDirector;
    public int skipToTime = 0;
    public GameObject UnwrappedJDRef;
    public Vector3 JDEndPosition = Vector3.zero;

    private bool hasSkipped = false;
    private PlayerInput playerInput;

    private void Start()
    {
        if (GameManager.Instance.HasSaveData())
        {
            continueButton.interactable = true;
            continueButtonOriginalState = true;
        }
        else
        {
            continueButton.interactable = false;
            continueButtonOriginalState = false;
        }

        AudioManager.instance.CrossFadeTo("DistantMountains");
        playerInput = GetComponent<PlayerInput>();
    }

    private void SkipOPCutscene()
    {
        MainMenuPanel.GetComponent<CanvasGroup>().alpha = 1;
        MainMenuPanel.GetComponent<CanvasGroup>().blocksRaycasts = true;

        UnwrappedJDRef.transform.position = JDEndPosition;

        mainTimelineDirector.time = skipToTime;
        
        for (int i = 0; i < mainMenuButtons.Count; i++)
        {
            if (mainMenuButtons[i].interactable)
            {
                mainMenuButtons[i].Select();
                break;
            }
        }
    }

    public void LoadPlay()
    {
        if (!GameManager.Instance.HasSaveData())
        {
            SceneManager.LoadScene(GameManager.Instance.GetLastSavedScene());
        }
        else
        {
            StartNewGame();
        }
    }

    public void LoadContinue()
    {
        SceneManager.LoadScene(GameManager.Instance.GetLastSavedScene());
    }

    private void ToggleMainMenuButtonInteraction(bool shouldToggle)
    {
        for (int i = 0; i < mainMenuButtons.Count; i++)
        {
            if (mainMenuButtons[i] == continueButton && shouldToggle)
            {
                mainMenuButtons[i].interactable = continueButtonOriginalState;
                continue;
            }

            mainMenuButtons[i].interactable = shouldToggle;
        }
    }

    void WarningResult()
    {
        warningCalled = true;
    }

    private void StartNewGame()
    {
        string warning = "This will erase your progress and reset the game!";
        if (!warningCalled)
        {
            warningPanel.SetActive(true);
            ToggleMainMenuButtonInteraction(false);
            warningPanel.GetComponent<WarningMessageUI>().SetWarning(warning, delegate { WarningResult(); }, delegate { WarningResult(); });
            warningPanel.GetComponent<WarningMessageUI>().AddWarningDelegate(delegate { StartNewGame(); warningCalled = false; ToggleMainMenuButtonInteraction(true); }, delegate { warningCalled = false; ToggleMainMenuButtonInteraction(true); });
        }
        else
        {
            GameManager.Instance.ResetSaveFiles();
            GameManager.Instance.currentSavedScene = 0;

            QuestManager.Instance.ResetAllQuests();
            QuestManager.Instance.currentQuestIndex = 0;
            SceneManager.LoadScene(GameManager.Instance.GetLastSavedScene());
        }
    }

    public void QuitGame()
    {
        GameManager.Instance.QuitGame();
    }

    private void Update()
    {
        if ((playerInput.actions["Unpause"].WasPressedThisFrame() || playerInput.actions["Jump"].WasPressedThisFrame()) && !hasSkipped)
        {
            SkipOPCutscene();
            hasSkipped = true;
        }
    }
}
