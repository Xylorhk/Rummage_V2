using UnityEngine;

public class ElectricalBox : Electrical, ISaveable
{ 
    [Header("Electrical Box Variables")]
    public MeshRenderer meshRenderer;
    public GameObject battery;
    public Transform collectionTransform;

    public bool isBatteryAttached = false;

    public float batteryCollectionRadius = 0;

    [Header("Juice Variables")]
    public float emissionLerpSpeed = 3f;

    [Range(1,5)]
    public float intensity = 2;
    public Color deactivatedEmissionColor;
    public Color activatedEmissionColor;

    new public object CaptureState()
    {
        return new SaveData
        {
            savedBatteryStatus = isBatteryAttached,
            poweredState = IsPowered()
        };
    }

    new public void RestoreState(object state)
    {
        var saveData = (SaveData)state;

        isBatteryAttached = saveData.savedBatteryStatus;
        SetIsPowered(saveData.poweredState, true);
    }

    [System.Serializable]
    private struct SaveData
    {
        public bool savedBatteryStatus;
        public bool poweredState;
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(collectionTransform.position, batteryCollectionRadius);
    }

    public bool GetBatteryStatus()
    {
        return isBatteryAttached;
    }

    public void SetBatteryStatus(bool nBatteryStatus)
    {
        isBatteryAttached = nBatteryStatus;
    }

    public void SetPowerEvent(bool shouldPower)
    {
        SetIsPowered(shouldPower);
    }

    public override void SetIsPowered(bool shouldPower, bool onSceneLoad = false)
    {
        if (!isBatteryAttached && shouldPower)
        {
            return;
        }

        isPowered = shouldPower;

        if (shouldPower)
        {
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

    void TryCollectBattery()
    {
        Collider[] collidersInRange = Physics.OverlapSphere(collectionTransform.position, batteryCollectionRadius);

        if (collidersInRange.Length > 0)
        {
            for (int i = 0; i < collidersInRange.Length; i++)
            {
                PowerSource powerSourceCheck = null;
                powerSourceCheck = collidersInRange[i].gameObject.GetComponent<PowerSource>();

                if (powerSourceCheck != null)
                {
                    FixedJoint fixedJointCheck = null;
                    fixedJointCheck = collidersInRange[i].gameObject.GetComponent<FixedJoint>();

                    if (fixedJointCheck != null)
                    {
                        fixedJointCheck.connectedBody.gameObject.GetComponent<GrabberEffector>().DropCurrentObj();
                    }

                    collidersInRange[i].gameObject.GetComponent<SaveEnabledState>().ShouldEnableObject(false);
                    isBatteryAttached = true;
                    return;
                }
            }
        }
    }

    private void Update()
    {
        if (!isBatteryAttached)
        {
            TryCollectBattery();
        }

        if (isBatteryAttached && !battery.activeSelf)
        {
            battery.SetActive(true);
        }
        else if (!isBatteryAttached && battery.activeSelf)
        {
            battery.SetActive(false);
        }

        if (IsPowered() && isBatteryAttached)
        {
            Color currentEmissionColor = meshRenderer.material.GetColor("_EmissionColor");

            if (currentEmissionColor != activatedEmissionColor * intensity)
            {
                currentEmissionColor = Color.Lerp(currentEmissionColor, activatedEmissionColor * intensity, emissionLerpSpeed * Time.deltaTime);

                meshRenderer.material.SetColor("_EmissionColor", currentEmissionColor);
            }
        }
        else
        {
            Color currentEmissionColor = meshRenderer.material.GetColor("_EmissionColor");

            if (currentEmissionColor != deactivatedEmissionColor * intensity)
            {
                currentEmissionColor = Color.Lerp(currentEmissionColor, deactivatedEmissionColor * intensity, emissionLerpSpeed * Time.deltaTime);

                meshRenderer.material.SetColor("_EmissionColor", currentEmissionColor);
            }
        }
    }
}