using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class DetectorEffector : Item
{
    [Header("Detector Variables")]
    public int maxRadius = 5;
    public float radiusGrowthTime = 1;
    public float detectorMoveThreshold = 0.1f;
    public GameObject detectorRadiusObj;
    public Renderer detectorRadiusRenderer;
    public float outsideFresnelPower = 5f;
    public float insideFresnelPower = 1.5f;
    public float fresnelLerpingSpeed = 3f;
    public List<Material> detectableMats = new List<Material>();

    [Header("Modifier Variables")]
    public int amplifiedMaxRadius = 10;

    private float currentRadius = 0;
    private int originalMaxRadius = 0;

    void Awake()
    {
        for (int i = 0; i < detectableMats.Count; i++)
        {
            detectableMats[i].SetFloat("_Radius", currentRadius);
            detectableMats[i].SetVector("_Center", detectorRadiusObj.transform.position);
        }

        originalMaxRadius = maxRadius;
    }

    public override void Activate()
    {
        if (currentRadius > 0)
        {
            UpdateVisibility();
        }

        Battery batteryCheck = gameObject.GetComponent<Battery>();

        if (batteryCheck != null)
        {
            if (!BatteryChargeUI.Instance.batteryUIObj.activeSelf)
            {
                BatteryChargeUI.Instance.ShowBatteryCharge(true);
            }

            if ((Player.Instance.playerInput.actions["Fire"].WasPressedThisFrame() && Player.Instance.playerInput.actions["Aim"].IsPressed()) || (Player.Instance.playerInput.actions["Fire"].IsPressed() && Player.Instance.playerInput.actions["Aim"].IsPressed()))
            {
                batteryCheck.ShouldDrainBattery(true);
            }
            else if (Player.Instance.playerInput.actions["Fire"].WasReleasedThisFrame() || Player.Instance.playerInput.actions["Aim"].WasReleasedThisFrame())
            {
                batteryCheck.ShouldDrainBattery(false);
            }
        }

        if (Player.Instance.playerInput.actions["Fire"].IsPressed() && Player.Instance.playerInput.actions["Aim"].IsPressed())
        {
            if (batteryCheck != null)
            {
                if (batteryCheck.GetCurrentFill() == 0)
                {
                    return;
                }
            }

            if (QuestManager.Instance.IsCurrentQuestActive())
            {
                Objective currentObjective = QuestManager.Instance.GetCurrentQuest().GetCurrentObjective();
                if (currentObjective != null)
                {
                    currentObjective.ActivateItem(itemName);
                }
            }

            if (currentRadius < maxRadius - 0.1f)
            {
                currentRadius = Mathf.Lerp(currentRadius, maxRadius, radiusGrowthTime * Time.deltaTime);
                SetDetectorRadiusSphereScale(currentRadius);
                return;
            }
            currentRadius = maxRadius;
            SetDetectorRadiusSphereScale(maxRadius);
        }
        else
        {
            if(currentRadius > 0.1f)
            {
                currentRadius = Mathf.Lerp(currentRadius, 0, radiusGrowthTime * Time.deltaTime);
                SetDetectorRadiusSphereScale(currentRadius);
                return;
            }
            currentRadius = 0;
            SetDetectorRadiusSphereScale(0.0f);

            for (int i = 0; i < detectableMats.Count; i++)
            {
                detectableMats[i].SetFloat("_Radius", 0);
                detectableMats[i].SetVector("_Center", Vector3.zero);
            }
        }
    }

    void UpdateSphereNotAlive()
    {
        if (currentRadius > 0.1f)
        {
            currentRadius = Mathf.Lerp(currentRadius, 0, radiusGrowthTime * Time.deltaTime);
            SetDetectorRadiusSphereScale(currentRadius);
            return;
        }
        currentRadius = 0;
        SetDetectorRadiusSphereScale(0.0f);

        for (int i = 0; i < detectableMats.Count; i++)
        {
            detectableMats[i].SetFloat("_Radius", 0);
            detectableMats[i].SetVector("_Center", Vector3.zero);
        }
    }

    void SetDetectorRadiusSphereScale(float radius)
    {
        float radiusX = radius * (1 / transform.localScale.x);
        float radiusY = radius * (1 / transform.localScale.y);
        float radiusZ = radius * (1 / transform.localScale.z);

        detectorRadiusObj.transform.localScale = new Vector3(radiusX, radiusY, radiusZ);
    }

    void UpdateVisibility()
    {
        for (int i = 0; i < detectableMats.Count; i++)
        {
            detectableMats[i].SetFloat("_Radius", currentRadius);
            detectableMats[i].SetVector("_Center", detectorRadiusObj.transform.position);
        }
    }

    void UpdateFresnelPower()
    {
        float currentFresnelPower = detectorRadiusRenderer.material.GetFloat("_FresnelPower");

        if (Vector3.Distance(detectorRadiusObj.transform.position, Player.Instance.vThirdPersonCamera.transform.position) < currentRadius)
        {
            if (currentFresnelPower > insideFresnelPower)
            {
                currentFresnelPower = Mathf.Lerp(currentFresnelPower, insideFresnelPower, fresnelLerpingSpeed * Time.deltaTime);

                detectorRadiusRenderer.material.SetFloat("_FresnelPower", currentFresnelPower);
            }
        }
        else
        {
            if (currentFresnelPower < outsideFresnelPower)
            {
                currentFresnelPower = Mathf.Lerp(currentFresnelPower, outsideFresnelPower, fresnelLerpingSpeed * Time.deltaTime);

                detectorRadiusRenderer.material.SetFloat("_FresnelPower", currentFresnelPower);
            }
        }
    }

    public override void ModifyComponent(ModifierItem.ModifierType modifierType)
    {
        switch (modifierType)
        {
            case ModifierItem.ModifierType.Amplifier:
                maxRadius = amplifiedMaxRadius;
                break;
            case ModifierItem.ModifierType.Exploding:
                break;
            case ModifierItem.ModifierType.Reflector:
                break;
        }
    }

    public override void UnmodifyComponent(ModifierItem.ModifierType modifierType)
    {
        switch (modifierType)
        {
            case ModifierItem.ModifierType.Amplifier:
                maxRadius = originalMaxRadius;
                break;
            case ModifierItem.ModifierType.Exploding:
                break;
            case ModifierItem.ModifierType.Reflector:
                break;
        }
    }

    public override void OnUnequip()
    {
        foreach (ModifierItem.ModifierType modifierType in (ModifierItem.ModifierType[])Enum.GetValues(typeof(ModifierItem.ModifierType)))
        {
            UnmodifyComponent(modifierType);
        }

        currentRadius = 0;

        for (int i = 0; i < detectableMats.Count; i++)
        {
            detectableMats[i].SetFloat("_Radius", 0);
            detectableMats[i].SetVector("_Center", Vector3.zero);
        }

        Battery batteryCheck = gameObject.GetComponent<Battery>();

        if (batteryCheck != null)
        {
            if (BatteryChargeUI.Instance.batteryUIObj.activeSelf)
            {
                BatteryChargeUI.Instance.ShowBatteryCharge(false);
            }
        }
    }

    void Update()
    {
        if (itemType != TypeTag.effector)
        {
            Debug.LogError($"{itemName} is currently of {itemType} type and not effector!");
        }

        if (!Player.Instance.IsAlive() || !Player.Instance.vThirdPersonInput.CanMove())
        {
            UpdateSphereNotAlive();
        }

        Battery batteryCheck = gameObject.GetComponent<Battery>();
        if (batteryCheck != null)
        {
            if (batteryCheck.GetCurrentFill() == 0 && batteryCheck.GetBatteryDrainStatus())
            {
                UpdateSphereNotAlive();
            }
        }

        UpdateFresnelPower();
    }

    void OnDestroy()
    {
        for (int i = 0; i < detectableMats.Count; i++)
        {
            detectableMats[i].SetFloat("_Radius", 0);
            detectableMats[i].SetVector("_Center", Vector3.zero);
        }
    }
}