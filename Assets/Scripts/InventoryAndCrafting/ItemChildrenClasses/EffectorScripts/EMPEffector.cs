using System;
using UnityEngine;

[Serializable]
public class EMPEffector : Item
{
    public int maxRadius = 5;
    public float radiusGrowthTime = 1;
    public GameObject EMPRadiusObj;

    [Header("Modifier Variables")]
    public int amplifiedMaxRadius = 10;

    private float currentRadius = 0;
    private int originalMaxRadius = 0;

    void Awake()
    {
        originalMaxRadius = maxRadius;
    }

    public override void Activate()
    {
        if (currentRadius > 0)
        {
            UpdateEMPDetection();
        }

        if (Player.Instance.playerInput.actions["Fire"].IsPressed())
        {
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
                SetEMPRadiusSphereScale(currentRadius);
                return;
            }
            currentRadius = maxRadius;
            SetEMPRadiusSphereScale(maxRadius);
        }
        else
        {
            if (currentRadius > 0.1f)
            {
                currentRadius = Mathf.Lerp(currentRadius, 0, radiusGrowthTime * Time.deltaTime);
                SetEMPRadiusSphereScale(currentRadius);
                return;
            }
            currentRadius = 0;
            SetEMPRadiusSphereScale(0.0f);
        }
    }

    void UpdateSphereNotAlive()
    {
        if (currentRadius > 0.1f)
        {
            currentRadius = Mathf.Lerp(currentRadius, 0, radiusGrowthTime * Time.deltaTime);
            SetEMPRadiusSphereScale(currentRadius);
            return;
        }
        currentRadius = 0;
        SetEMPRadiusSphereScale(0.0f);
    }

    void SetEMPRadiusSphereScale(float radius)
    {
        float radiusX = radius * (1 / transform.localScale.x);
        float radiusY = radius * (1 / transform.localScale.y);
        float radiusZ = radius * (1 / transform.localScale.z);

        EMPRadiusObj.transform.localScale = new Vector3(radiusX * 2, radiusY * 2, radiusZ * 2); //Multiplication is temp.
    }

    void UpdateEMPDetection()
    {
        Collider[] collidersInRange = Physics.OverlapSphere(EMPRadiusObj.transform.position, currentRadius);

        for (int i = 0; i < collidersInRange.Length; i++)
        {
            EffectorActions effectorActions = collidersInRange[i].gameObject.GetComponent<EffectorActions>();
            if (effectorActions != null)
            {
                effectorActions.EMPEffectorAction();
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
    }
}
