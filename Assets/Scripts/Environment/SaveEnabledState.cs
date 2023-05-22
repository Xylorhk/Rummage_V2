using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SaveEnabledState : MonoBehaviour, ISaveable
{
    public bool shouldBeEnabled = false;

    public object CaptureState()
    {
        return new SaveData
        {
            savedEnabledState = shouldBeEnabled
        };
    }

    public void RestoreState(object state)
    {
        var saveData = (SaveData)state;

        shouldBeEnabled = saveData.savedEnabledState;

        gameObject.SetActive(shouldBeEnabled);
    }

    [System.Serializable]
    private struct SaveData
    {
        public bool savedEnabledState;
    }

    public void ShouldEnableObject(bool shouldEnable)
    {
        if (shouldEnable == shouldBeEnabled)
        {
            return;
        }

        shouldBeEnabled = shouldEnable;
        gameObject.SetActive(shouldEnable);
    }

    void Update()
    {
        if (!shouldBeEnabled && gameObject.activeSelf)
        {
            shouldBeEnabled = true;
        }
    }
}
