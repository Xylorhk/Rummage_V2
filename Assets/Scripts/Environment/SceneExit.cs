using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SceneExit : MonoBehaviour, ISaveable
{
    public string nextSceneName = string.Empty;
    public string nextSceneSpawnLocationName = string.Empty;
    public bool canUseExit = false;

    private bool hasActivated = false;

    public object CaptureState()
    {
        return new SaveData
        {
            savedCanUseValue = canUseExit
        };
    }

    public void RestoreState(object state)
    {
        var saveData = (SaveData)state;

        canUseExit = saveData.savedCanUseValue;
    }

    [System.Serializable]
    private struct SaveData
    {
        public bool savedCanUseValue;
    }

    private void OnTriggerStay(Collider other)
    {
        if (other.gameObject.GetComponentInChildren<Player>() != null)
        {
            if (!hasActivated && canUseExit)
            {
                hasActivated = true;
                Player.Instance.vThirdPersonInput.ShouldMove(false);
                Player.Instance.panelFade.Fade((int)PanelComponentFade.FadeType.FadeIn);
                Player.Instance.panelFade.OnFadeFinished.AddListener(delegate { MoveToScene(); });
            }
        }
    }

    public void UpdateCanUseExit(bool nExit)
    {
        canUseExit = nExit;
    }

    public void MoveToScene()
    {
        LevelManager.Instance.LoadNextActiveScene(nextSceneName, nextSceneSpawnLocationName);
    }
}
