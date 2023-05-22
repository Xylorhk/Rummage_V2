using Invector.vCharacterController;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerItemHandler : MonoBehaviour
{
    public vThirdPersonController playerController;
    public Pickup itemDetection;
    private Item equippedItem;
    private GameObject equippedGO;

    public Transform leftHandAttachmentBone;

    public GameObject attachedItem = null;

    public Item GetEquippedItem()
    {
        
        if (equippedItem == null)
        {
            return null;
        }
        else
        {
            return equippedItem;
        }
        
    }

    void AttachItem(Vector3 newPos, Vector3 newRot)
    {
        attachedItem = equippedGO;
        attachedItem.transform.parent = leftHandAttachmentBone;
        attachedItem.transform.localPosition = newPos;
        attachedItem.transform.localRotation = Quaternion.Euler(newRot);
    }

    public void EquipItem(Item itemToEquip, bool calledFromInventory = false)
    {
        if (equippedGO != null)
        {
            if (itemToEquip.itemType == Item.TypeTag.grip)
            {
                ChassisItem tempChassis = itemToEquip.gameObject.GetComponentInChildren<ChassisItem>();
                if (tempChassis != null)
                {
                    if (tempChassis.itemName != Inventory.Instance.currentEquippedGO.GetComponent<Item>().itemName)
                    {
                        UnequipItem(equippedItem, true, calledFromInventory);
                    }
                }
            }
            else
            {
                UnequipItem(equippedItem, true, calledFromInventory);
            }
        }

        itemToEquip.gameObject.SetActive(true);
        itemToEquip.gameObject.GetComponent<Rigidbody>().isKinematic = true;
        itemToEquip.gameObject.GetComponent<Collider>().enabled = false;

        foreach (Item childItem in itemToEquip.gameObject.GetComponentsInChildren<Item>())
        {
            childItem.gameObject.SetActive(true);
            childItem.gameObject.GetComponent<Rigidbody>().isKinematic = true;
            childItem.gameObject.GetComponent<Collider>().enabled = false;
        }

        equippedItem = itemToEquip;
        equippedGO = equippedItem.gameObject;

        if (itemToEquip.itemType != Item.TypeTag.grip)
        {
            int chassisIndex = -1;
            for (int i = 0; i < Inventory.Instance.chassisDataModels.Count; i++)
            {
                if (Inventory.Instance.chassisDataModels[i].itemName == itemToEquip.itemName)
                {
                    chassisIndex = i;
                    break;
                }
            }

            ChassisDataModel tempChassisDataModel = new ChassisDataModel
            {
                itemName = Inventory.Instance.chassisDataModels[chassisIndex].itemName,
                itemPosition = Inventory.Instance.chassisDataModels[chassisIndex].itemPosition,
                itemRotation = Inventory.Instance.chassisDataModels[chassisIndex].itemRotation,
                isObtained = Inventory.Instance.chassisDataModels[chassisIndex].isObtained,
                isRestored = Inventory.Instance.chassisDataModels[chassisIndex].isRestored,
                isEquipped = true,
                componentItemModels = Inventory.Instance.chassisDataModels[chassisIndex].componentItemModels,
                gripItemModel = Inventory.Instance.chassisDataModels[chassisIndex].gripItemModel
            };
            Inventory.Instance.chassisDataModels[chassisIndex] = tempChassisDataModel;
            equippedItem.isEquipped = true;

            itemToEquip.OnEquip();
            Inventory.Instance.currentEquippedGO = equippedGO;
        }
        else
        {
            Inventory.Instance.currentEquippedGO = equippedItem.gameObject.GetComponentInChildren<ChassisItem>().gameObject;
        }
        
        AttachItem(itemToEquip.localHandPos, itemToEquip.localHandRot);
    }

    public void UnequipItem(Item itemToUnequip, bool calledFromHandler = false, bool calledFromInventory = false)
    {
        if (itemToUnequip.itemType != Item.TypeTag.grip)
        {
            itemToUnequip.isEquipped = false;
            Item currentItem = Inventory.Instance.currentEquippedGO.GetComponent<Item>();

            for (int i = 0; i < currentItem.chassisComponentTransforms.Count; i++)
            {
                if (currentItem.chassisComponentTransforms[i].IsComponentTransformOccupied())
                {
                    GrabberEffector grabberEffector = currentItem.chassisComponentTransforms[i].GetComponentTransformItem().gameObject.GetComponent<GrabberEffector>();

                    if (grabberEffector != null)
                    {
                        if (grabberEffector.currentAttachedObj != null)
                        {
                            grabberEffector.DropCurrentObj();
                        }
                        break;
                    }
                }
            }

            int chassisIndex = -1;
            for (int i = 0; i < Inventory.Instance.chassisDataModels.Count; i++)
            {
                if (Inventory.Instance.chassisDataModels[i].itemName == currentItem.itemName)
                {
                    chassisIndex = i;
                    break;
                }
            }

            ChassisDataModel tempChassisDataModel = new ChassisDataModel
            {
                itemName = Inventory.Instance.chassisDataModels[chassisIndex].itemName,
                itemPosition = Inventory.Instance.chassisDataModels[chassisIndex].itemPosition,
                itemRotation = Inventory.Instance.chassisDataModels[chassisIndex].itemRotation,
                isObtained = Inventory.Instance.chassisDataModels[chassisIndex].isObtained,
                isRestored = Inventory.Instance.chassisDataModels[chassisIndex].isRestored,
                isEquipped = false,
                componentItemModels = Inventory.Instance.chassisDataModels[chassisIndex].componentItemModels,
                gripItemModel = Inventory.Instance.chassisDataModels[chassisIndex].gripItemModel
            };
            Inventory.Instance.chassisDataModels[chassisIndex] = tempChassisDataModel;
            currentItem.OnUnequip();
        }
        else
        {
            int otherChassisIndex = -1;
            for (int i = 0; i < Inventory.Instance.chassisDataModels.Count; i++)
            {
                if (Inventory.Instance.chassisDataModels[i].itemName == itemToUnequip.gameObject.GetComponentInChildren<ChassisItem>().itemName)
                {
                    otherChassisIndex = i;
                    break;
                }
            }

            ChassisDataModel tempChassisDataModel = new ChassisDataModel
            {
                itemName = Inventory.Instance.chassisDataModels[otherChassisIndex].itemName,
                itemPosition = Inventory.Instance.chassisDataModels[otherChassisIndex].itemPosition,
                itemRotation = Inventory.Instance.chassisDataModels[otherChassisIndex].itemRotation,
                isObtained = Inventory.Instance.chassisDataModels[otherChassisIndex].isObtained,
                isRestored = Inventory.Instance.chassisDataModels[otherChassisIndex].isRestored,
                isEquipped = false,
                componentItemModels = Inventory.Instance.chassisDataModels[otherChassisIndex].componentItemModels,
                gripItemModel = Inventory.Instance.chassisDataModels[otherChassisIndex].gripItemModel
            };
            Inventory.Instance.chassisDataModels[otherChassisIndex] = tempChassisDataModel;
            itemToUnequip.gameObject.GetComponentInChildren<ChassisItem>().OnUnequip();
        }

        if (itemToUnequip.chassisGripTransform.IsGripTransformOccupied())
        {
            Destroy(itemToUnequip.chassisGripTransform.GetGripTransformItem().gameObject);
        }
        else
        {
            if (itemToUnequip.itemType == Item.TypeTag.grip && calledFromHandler)
            {
                GameObject chassis = itemToUnequip.GetComponentInChildren<ChassisItem>().gameObject;
                itemToUnequip.GetComponentInChildren<ChassisItem>().gameObject.transform.parent = null;

                if (calledFromInventory)
                {
                    Destroy(chassis);
                }
            }

            Destroy(itemToUnequip.gameObject);
        }

        equippedItem = null;
        equippedGO = null;
        attachedItem = null;
    }
}
