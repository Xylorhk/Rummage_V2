using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class GunEffector : Item
{
    public Launcher gunLauncher;
    
    private Item currentChassis = null;

    public override void Activate()
    {
        if (currentChassis == null && isEquipped)
        {
            FindCurrentChassis();
            return;
        }

        if (Player.Instance.playerInput.actions["Fire"].WasPressedThisFrame())
        {
            List<Item> currentAttachedAmmo = new List<Item>();
            for (int i = 0; i < currentChassis.chassisComponentTransforms.Count; i++)
            {
                if (currentChassis.chassisComponentTransforms[i].IsComponentTransformOccupied())
                {
                    if (currentChassis.chassisComponentTransforms[i].GetComponentTransformItem().gameObject == this.gameObject)
                    {
                        continue;
                    }
                    else if (currentChassis.chassisComponentTransforms[i].GetComponentTransformItem().itemType == Item.TypeTag.ammo)
                    {
                        currentAttachedAmmo.Add(currentChassis.chassisComponentTransforms[i].GetComponentTransformItem());
                    }
                }
            }

            for (int i = 0; i < currentAttachedAmmo.Count; i++)
            {
                gunLauncher.Shoot(currentAttachedAmmo[i].gameObject.GetComponent<AmmoItem>().ammoPrefabKey);
                currentAttachedAmmo[i].gameObject.GetComponent<AmmoItem>().DisableUnwrappedJuice();
            }

            if (QuestManager.Instance.IsCurrentQuestActive())
            {
                Objective currentObjective = QuestManager.Instance.GetCurrentQuest().GetCurrentObjective();
                if (currentObjective != null)
                {
                    currentObjective.ActivateItem(itemName);
                }
            }
        }
    }

    void FindCurrentChassis()
    {
        currentChassis = Inventory.Instance.currentEquippedGO.GetComponent<Item>();
    }

    void Update()
    {
        if (itemType != TypeTag.effector)
        {
            Debug.LogError($"{itemName} is currently of {itemType} type and not Effector!");
            return;
        }

        if (currentChassis != null)
        {
            if (!currentChassis.isEquipped)
            {
                currentChassis = null;
            }
        }
    }
}