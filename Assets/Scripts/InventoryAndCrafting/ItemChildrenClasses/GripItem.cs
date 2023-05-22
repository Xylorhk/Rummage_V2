using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class GripItem : Item
{
    public enum GripType
    {
        None,
        Simple,
        Pole,
        Umbrella
    };

    [Header("Grip Variables")]
    public GripType gripType = GripType.None;

    public override void Activate()
    {
        if (Player.Instance.playerInput.actions["Aim"].WasPressedThisFrame())
        {
            if (Player.Instance.vThirdPersonInput.CanMove())
            {
                Player.Instance.anim.SetBool("IsActivated", true);
                
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
        else if (Player.Instance.playerInput.actions["Aim"].WasReleasedThisFrame())
        {
            Player.Instance.anim.SetBool("IsActivated", false);
        }

        if (!Player.Instance.playerInput.actions["Aim"].IsPressed() && Player.Instance.anim.GetBool("IsActivated"))
        {
            Player.Instance.anim.SetBool("IsActivated", false);
        }
    }

    public override void OnEquip()
    {

    }

    public override void OnUnequip()
    {
        Player.Instance.anim.SetInteger("GripEnum", -1);
        Player.Instance.anim.SetBool("IsActivated", false);
    }

    void Update()
    {
        if (itemType != TypeTag.grip)
        {
            Debug.LogError($"{itemName} is currently of {itemType} type and not Grip!");
            return;
        }
    }
}
