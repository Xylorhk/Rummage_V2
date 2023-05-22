using System;
using UnityEngine;

[Serializable]
public class ModifierItem : Item
{
    public enum ModifierType
    {
        Amplifier,
        Reflector,
        Exploding
    }

    public ModifierType modifierType = ModifierType.Amplifier;

    private Item currentChassis = null;

    public override void Activate()
    {
        if (currentChassis == null)
        {
            FindCurrentChassis();

            if (currentChassis == null)
            {
                Debug.LogError("Could not find current Chassis.");
                return;
            }
        }

        for (int i = 0; i < currentChassis.chassisComponentTransforms.Count; i++)
        {
            if (!currentChassis.chassisComponentTransforms[i].IsComponentTransformOccupied())
            {
                continue;
            }

            if (currentChassis.chassisComponentTransforms[i].GetComponentTransformItem().gameObject == this.gameObject)
            {
                continue;
            }

            currentChassis.chassisComponentTransforms[i].GetComponentTransformItem().ModifyComponent(modifierType);
        }
    }

    public void Deactivate()
    {
        if(currentChassis == null)
        {
            FindCurrentChassis();

            if (currentChassis == null)
            {
                Debug.LogError("Could not find current Chassis.");
                return;
            }
        }

        for(int i = 0; i < currentChassis.chassisComponentTransforms.Count; i++)
        {
            if (!currentChassis.chassisComponentTransforms[i].IsComponentTransformOccupied())
            {
                continue;
            }

            currentChassis.chassisComponentTransforms[i].GetComponentTransformItem().UnmodifyComponent(modifierType);
        }
    }

    void FindCurrentChassis()
    {
        currentChassis = Inventory.Instance.currentEquippedGO.GetComponent<Item>();
    }

    public override void OnUnequip()
    {
        Deactivate();
        currentChassis = null;
    }

    void Update()
    {
        if (itemType != TypeTag.modifier)
        {
            Debug.LogError($"{itemName} is currently of {itemType} type and not Modifier!");
            return;
        }
    }
}
