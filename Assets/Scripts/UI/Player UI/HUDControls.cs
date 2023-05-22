using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class HUDControls : MonoBehaviour
{
    [Header("Player UI")]
    public GameObject pauseUI;
    public GameObject optionsUI;

    [Header("Pickup and Item UI")]
    public GameObject pickUpUIText;
    public GameObject inventoryUI;
    public GameObject craftingUI;

    [Header("Misc. UI")]
    public GameObject crosshairImage;
    public GameObject winScreenUI;

    void Start()
    {
        inventoryUI.SetActive(false);
        craftingUI.SetActive(false);
        pauseUI.SetActive(false);
        //winScreenUI.SetActive(false);

        Cursor.lockState = CursorLockMode.Locked;
        Time.timeScale = 1f;
    }

    void Update()
    {
        if (Player.Instance.playerInput.actions["Inventory"].WasPressedThisFrame() && Player.Instance.IsAlive())
        {
            if (craftingUI.activeSelf)
            {
                toggleOpt(craftingUI);
            }
            if (pauseUI.activeSelf)
            {
                toggleOpt(pauseUI);
            }

            toggleOpt(inventoryUI);
        }
        else if (Player.Instance.playerInput.actions["Crafting"].WasPressedThisFrame() && Player.Instance.IsAlive())
        {
            if (inventoryUI.activeSelf)
            {
                toggleOpt(inventoryUI);
            }
            if (pauseUI.activeSelf)
            {
                toggleOpt(pauseUI);
            }

            toggleOpt(craftingUI);
        }
        else if (Player.Instance.playerInput.actions["Pause"].WasPressedThisFrame())
        {
            if (craftingUI.activeSelf)
            {
                toggleOpt(craftingUI);
            }
            if (inventoryUI.activeSelf)
            {
                toggleOpt(inventoryUI);
            }

            toggleOpt(pauseUI);
            optionsUI.SetActive(false);
        }
        else if (Player.Instance.playerInput.actions["Unpause"].WasPerformedThisFrame())
        {
            if (craftingUI.activeSelf)
            {
                toggleOpt(craftingUI);
            }
            if (inventoryUI.activeSelf)
            {
                toggleOpt(inventoryUI);
            }
            if (pauseUI.activeSelf)
            {
                toggleOpt(pauseUI);
            }

            optionsUI.SetActive(false);
        }

        if (Player.Instance.IsAlive() && Player.Instance.vThirdPersonInput.CanMove())
        {
            if (pickUpUIText.activeSelf && Time.timeScale == 0.0f)
            {
                pickUpUIText.SetActive(false);
            }
            else if (!pickUpUIText.activeSelf && Time.timeScale == 1.0f)
            {
                pickUpUIText.SetActive(true);
            }

            if (!pauseUI.activeSelf && Time.timeScale == 1.0f)
            {
                if (Player.Instance.playerInput.actions["Aim"].WasPressedThisFrame())
                {
                    if (Player.Instance.anim.GetInteger("GripEnum") > 0)
                    {
                        crosshairImage.SetActive(true);
                        Player.Instance.aimingRig.weight = 1;
                    }
                    
                }
                else if (Player.Instance.playerInput.actions["Aim"].WasReleasedThisFrame())
                {
                    if (Player.Instance.anim.GetInteger("GripEnum") > 0)
                    {
                        crosshairImage.SetActive(false);
                        Player.Instance.aimingRig.weight = 0;
                    }
                }
            }
            else
            {
                if (Player.Instance.anim.GetInteger("GripEnum") > 0)
                {
                    crosshairImage.SetActive(false);
                    Player.Instance.aimingRig.weight = 0;
                }
            }
        }
        else
        {
            crosshairImage.SetActive(false);
            Player.Instance.aimingRig.weight = 0;
        }
    }

    public void toggleOpt(GameObject uiName)
    {
        StopCoroutine(UnlockCursor());

        if(uiName.activeSelf)
        {
            uiName.SetActive(false);
            Time.timeScale = 1f;

            Cursor.lockState = CursorLockMode.Locked;
        }
        else
        {
            uiName.SetActive(true);
            Time.timeScale = 0f;

            if (Player.Instance.playerInput.currentControlScheme == "Keyboard")
            {
                StartCoroutine(UnlockCursor());
            }
        }
    }

    IEnumerator UnlockCursor()
    {
        yield return null;
        Cursor.lockState = CursorLockMode.None;
    }
}
