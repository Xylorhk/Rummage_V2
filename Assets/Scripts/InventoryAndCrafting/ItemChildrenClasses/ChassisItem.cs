using Invector.vCharacterController;
using System.Collections.Generic;
using UnityEngine;

public class ChassisItem : Item
{
    List<bool> currentSlotsFilled = new List<bool>();

    void Awake()
    {
        if (currentSlotsFilled.Count == 0)
        {
            for (int i = 0; i < chassisComponentTransforms.Count; i++)
            {
                currentSlotsFilled.Add(false);
            }
        }
    }

    public override void OnEquip()
    {
        if (isEquipped == false)
        {
            return;
        }

        Player.Instance.anim.SetInteger("GripEnum", 0);
    }

    public override void OnUnequip()
    {
        Player.Instance.anim.SetInteger("GripEnum", -1);
        Player.Instance.anim.SetBool("IsActivated", false);
    }

    void LateUpdate()
    {
        //Check to see if chassis active should run
        if (itemType != TypeTag.chassis)
        {
            Debug.LogError($"{itemName} is currently of {itemType} type and not Chassis!");
            return;
        }

        if (chassisComponentTransforms.Count < 1 || isEquipped == false || Time.timeScale == 0.0f || !Player.Instance.IsAlive())
        {
            return;
        }

        
        if (Player.Instance.playerInput.actions["Aim"].WasPressedThisFrame())
        {
            if (Player.Instance.vThirdPersonInput.CanMove())
            {
                Player.Instance.anim.SetBool("IsActivated", true);
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

        //Handle all attached components
        for (int i = 0; i < chassisComponentTransforms.Count; i++)
        {
            if (!Player.Instance.vThirdPersonInput.CanMove())
            {
                break;
            }

            if (chassisComponentTransforms[i].IsComponentTransformOccupied())
            {
                chassisComponentTransforms[i].GetComponentTransformItem().Activate();
            }
        }

        bool hasComponent = false;
        for (int i = 0; i < chassisComponentTransforms.Count; i++)
        {
            if (chassisComponentTransforms[i].IsComponentTransformOccupied())
            {
                if (!chassisComponentTransforms[i].GetComponentTransformItem().isEquipped)
                {
                    chassisComponentTransforms[i].GetComponentTransformItem().isEquipped = true;
                }

                hasComponent = true;
            }
        }
        Player.Instance.anim.SetBool("HasComponent", hasComponent);

        //Handle the attached grip
        if (chassisGripTransform.IsGripTransformOccupied())
        {
            if (chassisGripTransform.GetGripTransformItem() == null)
            {
                if (Player.Instance.anim.GetInteger("GripEnum") != 0)
                {
                    Player.Instance.anim.SetInteger("GripEnum", 0);
                }

                return;
            }


            if (Player.Instance.anim.GetInteger("GripEnum") != (int)chassisGripTransform.GetGripTransformItem().gameObject.GetComponent<GripItem>().gripType)
            {
                Player.Instance.anim.SetInteger("GripEnum", (int)chassisGripTransform.GetGripTransformItem().gameObject.GetComponent<GripItem>().gripType);
            }

            chassisGripTransform.GetGripTransformItem().Activate();
        }
        else
        {
            if (Player.Instance.anim.GetInteger("GripEnum") != 0)
            {
                Player.Instance.anim.SetInteger("GripEnum", 0);
            }
        }
    }
}
