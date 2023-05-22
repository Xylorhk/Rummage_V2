using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

#region Component Transform
[System.Serializable]
public class ChassisComponentTransform
{
    public Transform componentTransform;
    private bool isOccupied = false;
    private Item currentComponent = null;

    public void CopyChassisComponentTransform(ChassisComponentTransform transformToCopy)
    {
        isOccupied = transformToCopy.IsComponentTransformOccupied();
        currentComponent = transformToCopy.GetComponentTransformItem();
    }

    public void AddNewComponentTransform(Item newComponent)
    {
        if (newComponent.itemType == Item.TypeTag.chassis || newComponent.itemType == Item.TypeTag.grip)
        {
            Debug.LogError("Illegal item type being added as Component Transform!");
            return;
        }

        isOccupied = true;
        currentComponent = newComponent;
        currentComponent.isEquipped = true;
        currentComponent.OnEquip();
    }

    public Item GetComponentTransformItem()
    {
        return currentComponent;
    }

    public bool IsComponentTransformOccupied()
    {
        return isOccupied;
    }

    public void ResetComponentTransform()
    {
        if (currentComponent == null)
        {
            return;
        }

        currentComponent.isEquipped = false;
        currentComponent.OnUnequip();

        isOccupied = false;
        currentComponent = null;
    }
}
#endregion

#region Grip Transform
[System.Serializable]
public class ChassisGripTransform
{
    public Transform gripTransform;
    private bool isOccupied = false;
    private Item currentGrip = null;

    public void CopyChassisGripTransform(ChassisGripTransform transformToCopy)
    {
        isOccupied = transformToCopy.IsGripTransformOccupied();
        currentGrip = transformToCopy.GetGripTransformItem();
    }

    public void AddNewGripTransform(Item newGrip)
    {
        isOccupied = true;
        currentGrip = newGrip;
        currentGrip.isEquipped = true;
        currentGrip.OnEquip();
    }

    public Item GetGripTransformItem()
    {
        return currentGrip;
    }

    public bool IsGripTransformOccupied()
    {
        return isOccupied;
    }

    public void ResetGripTransform()
    {
        if (currentGrip == null)
        {
            return;
        }

        currentGrip.isEquipped = false;
        currentGrip.OnUnequip();

        isOccupied = false;
        currentGrip = null;
    }
}
#endregion

#region Data Models
[Serializable]
public struct ChassisDataModel
{
    public string itemName;
    public List<float> itemPosition;
    public List<float> itemRotation;
    public bool canPickUp;
    public bool isObtained;
    public bool isRestored;
    public bool isEquipped;
    public List<ItemDataModel?> componentItemModels;
    public ItemDataModel? gripItemModel;
}

[Serializable]
public struct ItemDataModel
{
    public string itemName;
    public List<float> itemPosition;
    public List<float> itemRotation;
    public bool canPickUp;
    public bool isObtained;
    public bool isRestored;
    public bool isEquipped;
}
#endregion

[Serializable]
[RequireComponent(typeof(SaveableEntity))]
public class Item : MonoBehaviour, ISaveable, IGrabbable
{
    #region Variables
    public enum TypeTag
    {
        chassis,
        effector,
        grip,
        ammo,
        modifier,
        scrap,
        external
    };
    public TypeTag itemType;

    public string itemName;
    [TextArea]
    public string description;
    public bool canPickUp = true;
    public bool isObtained = false;
    public bool isRestored = false;
    public int restorationScrapAmount = 0;
    public bool isEquipped = false;
    public Vector3 localHandPos = Vector3.zero;
    public Vector3 localHandRot = Vector3.zero;
    public Sprite inventorySprite;
    public List<ChassisComponentTransform> chassisComponentTransforms = new List<ChassisComponentTransform>();
    public ChassisGripTransform chassisGripTransform;

    public bool interactableOnStart = true;

    #endregion

    #region Saveables
    public object CaptureState()
    {
        List<float> currentPosition = new List<float>();
        currentPosition.Add(transform.position.x);
        currentPosition.Add(transform.position.y);
        currentPosition.Add(transform.position.z);

        List<float> currentRotation = new List<float>();
        currentRotation.Add(transform.rotation.x);
        currentRotation.Add(transform.rotation.y);
        currentRotation.Add(transform.rotation.z);
        currentRotation.Add(transform.rotation.w);

        if (itemType == TypeTag.chassis)
        {
            List<ItemDataModel?> tempComponentModels = new List<ItemDataModel?>();
            for (int i = 0; i < chassisComponentTransforms.Count; i++)
            {
                ItemDataModel? currentItemDataModel;
                if (chassisComponentTransforms[i].IsComponentTransformOccupied())
                {
                    Item currentComponent = chassisComponentTransforms[i].GetComponentTransformItem();

                    currentItemDataModel = new ItemDataModel
                    {
                        itemName = currentComponent.itemName,
                        itemPosition = currentPosition,
                        itemRotation = currentRotation,
                        canPickUp = currentComponent.canPickUp,
                        isObtained = currentComponent.isObtained,
                        isRestored = currentComponent.isRestored,
                        isEquipped = currentComponent.isEquipped
                    };

                    tempComponentModels.Add(currentItemDataModel);
                }
                else
                {
                    currentItemDataModel = null;
                    tempComponentModels.Add(currentItemDataModel);
                }
            }

            ItemDataModel? tempGripModel;
            if (chassisGripTransform.IsGripTransformOccupied())
            {
                Item currentGripItem = chassisGripTransform.GetGripTransformItem();
                tempGripModel = new ItemDataModel
                {
                    itemName = currentGripItem.itemName,
                    itemPosition = currentPosition,
                    itemRotation = currentRotation,
                    canPickUp = currentGripItem.canPickUp,
                    isObtained = currentGripItem.isObtained,
                    isRestored = currentGripItem.isRestored,
                    isEquipped = currentGripItem.isEquipped
                };
            }
            else
            {
                tempGripModel = null;
            }


            return new ChassisDataModel
            {
                itemName = itemName,
                itemPosition = currentPosition,
                itemRotation = currentRotation,
                canPickUp = canPickUp,
                isObtained = isObtained,
                isRestored = isRestored,
                isEquipped = isEquipped,
                componentItemModels = new List<ItemDataModel?>(tempComponentModels),
                gripItemModel = tempGripModel,
            };
        }
        else
        {
            return new ItemDataModel
            {
                itemName = itemName,
                itemPosition = currentPosition,
                itemRotation = currentRotation,
                canPickUp = canPickUp,
                isObtained = isObtained,
                isRestored = isRestored,
                isEquipped = isEquipped,
            };
        }
    }

    public void RestoreState(object state)
    {
        if (itemType != TypeTag.chassis)
        {
            var savedItemDataModel = (ItemDataModel)state;

            if (savedItemDataModel.isObtained || savedItemDataModel.isEquipped)
            {
                Destroy(this.gameObject);
                return;
            }
            else
            {
                transform.position = new Vector3(savedItemDataModel.itemPosition[0], savedItemDataModel.itemPosition[1], savedItemDataModel.itemPosition[2]);
                transform.rotation = new Quaternion(savedItemDataModel.itemRotation[0], savedItemDataModel.itemRotation[1], savedItemDataModel.itemRotation[2], savedItemDataModel.itemRotation[3]);
                LoadItemModelInfo(savedItemDataModel);
            }
        }
        else
        {
            var savedChassisDataModel = (ChassisDataModel)state;

            if (savedChassisDataModel.isObtained || savedChassisDataModel.isEquipped)
            {
                Destroy(this.gameObject);
                return;
            }
            else
            {
                transform.position = new Vector3(savedChassisDataModel.itemPosition[0], savedChassisDataModel.itemPosition[1], savedChassisDataModel.itemPosition[2]);
                transform.rotation = new Quaternion(savedChassisDataModel.itemRotation[0], savedChassisDataModel.itemRotation[1], savedChassisDataModel.itemRotation[2], savedChassisDataModel.itemRotation[3]);
                LoadChassisModelInfo(savedChassisDataModel);

                ///Add logic for equipped Chassis here.
            }
        }
    }

    public void SetPickUp(bool nPickUp)
    {
        canPickUp = nPickUp;
    }
    #endregion

    #region Gizmos
    private void OnDrawGizmos()
    {
        if (itemType == TypeTag.chassis)
        {
            if (chassisGripTransform.gripTransform != null)
            {
                Gizmos.color = new Color(0, 1, 0, 0.5f);
                Gizmos.DrawSphere(chassisGripTransform.gripTransform.position, 0.05f);
            }
            
            for (int i = 0; i < chassisComponentTransforms.Count; i++)
            {
                if (chassisComponentTransforms[i].componentTransform != null)
                {
                    Gizmos.color = new Color(0, 0, 1, 0.5f);
                    Gizmos.DrawSphere(chassisComponentTransforms[i].componentTransform.position, 0.05f);
                }
            }
        }
    }
    #endregion

    #region Public Functions
    public void LoadItemModelInfo(ItemDataModel itemDataModel)
    {
        canPickUp = itemDataModel.canPickUp;
        isObtained = itemDataModel.isObtained;
        isRestored = itemDataModel.isRestored;
        isEquipped = itemDataModel.isEquipped;
    }

    public void LoadChassisModelInfo(ChassisDataModel chassisDataModel)
    {
        canPickUp = chassisDataModel.canPickUp;
        isObtained = chassisDataModel.isObtained;
        isRestored = chassisDataModel.isRestored;
        isEquipped = chassisDataModel.isEquipped;

        for (int i = 0; i < chassisDataModel.componentItemModels.Count; i++)
        {
            chassisComponentTransforms[i].ResetComponentTransform();
            if (chassisDataModel.componentItemModels[i].HasValue)
            {
                GameObject currentComponentGameObject = ItemPooler.Instance.InstantiateItemByName(chassisDataModel.componentItemModels[i].Value.itemName);
                currentComponentGameObject.GetComponent<Item>().LoadItemModelInfo(chassisDataModel.componentItemModels[i].Value);

                currentComponentGameObject.GetComponent<Rigidbody>().isKinematic = true;
                currentComponentGameObject.GetComponent<Collider>().enabled = false;
                currentComponentGameObject.gameObject.SetActive(true);

                currentComponentGameObject.transform.parent = gameObject.transform;
                currentComponentGameObject.transform.position = chassisComponentTransforms[i].componentTransform.position;
                currentComponentGameObject.transform.rotation = chassisComponentTransforms[i].componentTransform.rotation;

                chassisComponentTransforms[i].AddNewComponentTransform(currentComponentGameObject.GetComponent<Item>());
            }
        }

        chassisGripTransform.ResetGripTransform();
        if (chassisDataModel.gripItemModel.HasValue)
        {
            GameObject currentGripGameObject = ItemPooler.Instance.InstantiateItemByName(chassisDataModel.gripItemModel.Value.itemName);
            currentGripGameObject.GetComponent<Item>().LoadItemModelInfo(chassisDataModel.gripItemModel.Value);

            gameObject.GetComponent<Rigidbody>().isKinematic = true;
            gameObject.GetComponent<Collider>().enabled = false;
            gameObject.SetActive(true);

            currentGripGameObject.transform.position = new Vector3(chassisDataModel.gripItemModel.Value.itemPosition[0], chassisDataModel.gripItemModel.Value.itemPosition[1], chassisDataModel.gripItemModel.Value.itemPosition[2]);
            currentGripGameObject.transform.rotation = new Quaternion(chassisDataModel.gripItemModel.Value.itemRotation[0], chassisDataModel.gripItemModel.Value.itemRotation[1], chassisDataModel.gripItemModel.Value.itemRotation[2], chassisDataModel.gripItemModel.Value.itemRotation[3]);

            gameObject.transform.parent = currentGripGameObject.transform;
            gameObject.transform.position = currentGripGameObject.transform.position;
            gameObject.transform.localRotation = Quaternion.Euler(0, currentGripGameObject.GetComponent<Item>().localHandRot.y, 0);

            chassisGripTransform.AddNewGripTransform(currentGripGameObject.GetComponent<Item>());
        }
    }
    #endregion

    #region Monobehaviour Functions
    protected void Start()
    {
        Interactables interactable = GetComponent<Interactables>();
        bool shouldPulse = ((!isObtained || !isEquipped) && interactableOnStart);

        interactable.ShouldPulse(shouldPulse);
    }

    #endregion

    #region Virtual Functions
    public virtual void OnEquip() { }
    public virtual void OnUnequip() { }
    public virtual void Activate() { }
    public virtual void ModifyComponent(ModifierItem.ModifierType modifierType) { }
    public virtual void UnmodifyComponent(ModifierItem.ModifierType modifierType) { }
    #endregion
}
