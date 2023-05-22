using UnityEngine;
using UnityEngine.Events;

public class Electrical : MonoBehaviour, ISaveable
{
    [Header("Electrical Parent Class Variables")]
    public string electricalLayerName = "Electrical";
    public bool poweredOnAwake = false;

    public UnityEvent OnActivated;
    public UnityEvent OnDeactived;
    public UnityEvent OnPowerStart;
    public UnityEvent OnPowerStop;

    protected bool isPowered = false;

    public object CaptureState()
    {
        return new SaveData
        {
            poweredState = IsPowered()
        };
    }

    public void RestoreState(object state)
    {
        var saveData = (SaveData)state;

        SetIsPowered(saveData.poweredState, true);
    }

    [System.Serializable]
    private struct SaveData
    {
        public bool poweredState;
    }

    void Awake()
    {
        gameObject.layer = LayerMask.NameToLayer(electricalLayerName);

        if (poweredOnAwake)
        {
            SetIsPowered(true, true);
        }
    }

    public virtual void SetIsPowered(bool shouldPower, bool onSceneLoad = false)
    {
        isPowered = shouldPower;

        if (shouldPower)
        {
            Debug.Log("Yes");
            OnActivated?.Invoke();
        }
        else
        {
            OnDeactived?.Invoke();
        }

        if (!onSceneLoad)
        {
            GameManager.Instance.SaveScene();
        }
    }

    public bool IsPowered()
    {
        return isPowered;
    }
}
