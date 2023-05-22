using BasicTools.ButtonInspector;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Inventory : SingletonMonoBehaviour<Inventory>, ISaveable
{
    [Header("Containers and External Variables")]
    public List<ChassisDataModel> chassisDataModels = new List<ChassisDataModel>();
    public List<ItemDataModel> itemDataModels = new List<ItemDataModel>();
    public GameObject currentEquippedGO;
    public PlayerItemHandler playerItemHandler;
    public Transform dropTransform;

    [Header("Inventory Variables")]
    public int amountOfScrap = 0;
    public GameObject inventoryItemPanel;
    public GameObject inventoryItemBox;
    [Range(3,6)]
    public int maxNumbofColumns = 0;
    public FlexibleGridLayout inventoryPanelGridLayout;

    [Header("UI Variables")]
    public GameObject inventoryPanel;
    public TMPro.TextMeshProUGUI scrapText;
    public Image selectedInspectorImage;
    public TMPro.TextMeshProUGUI selectedItemTitle;
    public TMPro.TextMeshProUGUI selectedItemDescription;
    public GameObject dropItemButton;
    public GameObject equipItemButton;
    public GameObject unequipItemButton;
    public GameObject restoreItemButton;
    private bool isActive = false;

    private ObjectPooler.Key inventoryItemUIKey = ObjectPooler.Key.InventoryItemUIButtons;

    public object CaptureState()
    {
        return new SaveData
        {
            amountOfScrap = amountOfScrap,
            chassisDataModels = chassisDataModels,
            itemDataModels = itemDataModels,
        };
    }

    public void RestoreState(object state)
    {
        var saveData = (SaveData)state;

        amountOfScrap = saveData.amountOfScrap;
        chassisDataModels = new List<ChassisDataModel>(saveData.chassisDataModels);
        
        for (int i = 0; i < chassisDataModels.Count; i++)
        {
            if (chassisDataModels[i].isEquipped)
            {
                EquipItem(i);
                break;
            }
        }

        itemDataModels = new List<ItemDataModel>(saveData.itemDataModels);
    }

    [Serializable]
    private struct SaveData
    {
        public int amountOfScrap;
        public List<ChassisDataModel> chassisDataModels;
        public List<ItemDataModel> itemDataModels;
    }

    private new void Awake()
    {
        base.Awake();
        inventoryPanelGridLayout.columns = maxNumbofColumns;
    }

    private void Update()
    {
        if (inventoryPanel.activeSelf && !isActive)
        {
            isActive = true;
            ResetSelectedInfo();
            InitInventory();
        }
        else if (!inventoryPanel.activeSelf && isActive)
        {
            isActive = false;
            ResetSelectedInfo();
            DeactivateCurrentInventoryView();
        }
    }

    void InitInventory()
    {
        DisplayScrapAmount();
        UpdateInventoryView();
    }

    void DisplayScrapAmount()
    {
        scrapText.text = $"Scrap: {amountOfScrap.ToString("D4")}";
    }

    void ResetSelectedInfo()
    {
        selectedInspectorImage.sprite = null;
        selectedInspectorImage.color = new Color(0, 0, 0, 0);
        selectedItemTitle.text = "";
        selectedItemDescription.text = "";

        dropItemButton.SetActive(false);
        dropItemButton.GetComponentInChildren<Button>().onClick.RemoveAllListeners();
        equipItemButton.SetActive(false);
        equipItemButton.GetComponentInChildren<Button>().onClick.RemoveAllListeners();
        unequipItemButton.SetActive(false);
        unequipItemButton.GetComponentInChildren<Button>().onClick.RemoveAllListeners();
        restoreItemButton.SetActive(false);
        restoreItemButton.GetComponentInChildren<Button>().onClick.RemoveAllListeners();
    }

    public void AddToInventory(Item newItem, bool shouldIncreaseBackpack)
    {
        if (newItem.itemType == Item.TypeTag.chassis)
        {
            List<ItemDataModel?> tempComponentModels = new List<ItemDataModel?>();
            for (int i = 0; i < newItem.chassisComponentTransforms.Count; i++)
            {
                ItemDataModel? currentItemDataModel;
                if (newItem.chassisComponentTransforms[i].IsComponentTransformOccupied())
                {
                    Item currentComponent = newItem.chassisComponentTransforms[i].GetComponentTransformItem();

                    List<float> currentPosition = new List<float>();
                    currentPosition.Add(currentComponent.gameObject.transform.position.x);
                    currentPosition.Add(currentComponent.gameObject.transform.position.y);
                    currentPosition.Add(currentComponent.gameObject.transform.position.z);

                    List<float> currentRotation = new List<float>();
                    currentRotation.Add(currentComponent.gameObject.transform.rotation.x);
                    currentRotation.Add(currentComponent.gameObject.transform.rotation.y);
                    currentRotation.Add(currentComponent.gameObject.transform.rotation.z);
                    currentRotation.Add(currentComponent.gameObject.transform.rotation.w);

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
            if (newItem.chassisGripTransform.IsGripTransformOccupied())
            {
                Item currentGripItem = newItem.chassisGripTransform.GetGripTransformItem();

                List<float> currentPosition = new List<float>();
                currentPosition.Add(currentGripItem.gameObject.transform.position.x);
                currentPosition.Add(currentGripItem.gameObject.transform.position.y);
                currentPosition.Add(currentGripItem.gameObject.transform.position.z);

                List<float> currentRotation = new List<float>();
                currentRotation.Add(currentGripItem.gameObject.transform.rotation.x);
                currentRotation.Add(currentGripItem.gameObject.transform.rotation.y);
                currentRotation.Add(currentGripItem.gameObject.transform.rotation.z);
                currentRotation.Add(currentGripItem.gameObject.transform.rotation.w);

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

            List<float> currentChassisPosition = new List<float>();
            currentChassisPosition.Add(newItem.gameObject.transform.position.x);
            currentChassisPosition.Add(newItem.gameObject.transform.position.y);
            currentChassisPosition.Add(newItem.gameObject.transform.position.z);

            List<float> currentChassisRotation = new List<float>();
            currentChassisRotation.Add(newItem.gameObject.transform.rotation.x);
            currentChassisRotation.Add(newItem.gameObject.transform.rotation.y);
            currentChassisRotation.Add(newItem.gameObject.transform.rotation.z);
            currentChassisRotation.Add(newItem.gameObject.transform.rotation.w);

            ChassisDataModel currentChassisDataModel =  new ChassisDataModel
            {
                itemName = newItem.itemName,
                itemPosition = currentChassisPosition,
                itemRotation = currentChassisRotation,
                canPickUp = newItem.canPickUp,
                isObtained = newItem.isObtained,
                isRestored = newItem.isRestored,
                isEquipped = newItem.isEquipped,
                componentItemModels = new List<ItemDataModel?>(tempComponentModels),
                gripItemModel = tempGripModel,
            };

            currentChassisDataModel.isObtained = true;
            currentChassisDataModel.canPickUp = false;
            chassisDataModels.Add(currentChassisDataModel);
        }
        else
        {
            List<float> currentItemPosition = new List<float>();
            currentItemPosition.Add(newItem.gameObject.transform.position.x);
            currentItemPosition.Add(newItem.gameObject.transform.position.y);
            currentItemPosition.Add(newItem.gameObject.transform.position.z);

            List<float> currentItemRotation = new List<float>();
            currentItemRotation.Add(newItem.gameObject.transform.rotation.x);
            currentItemRotation.Add(newItem.gameObject.transform.rotation.y);
            currentItemRotation.Add(newItem.gameObject.transform.rotation.z);
            currentItemRotation.Add(newItem.gameObject.transform.rotation.w);

            ItemDataModel currentItemDataModel = new ItemDataModel
            {
                itemName = newItem.itemName,
                itemPosition = currentItemPosition,
                itemRotation = currentItemRotation,
                canPickUp = newItem.canPickUp,
                isObtained = newItem.isObtained,
                isRestored = newItem.isRestored,
                isEquipped = newItem.isEquipped,
            };

            currentItemDataModel.isObtained = true;
            currentItemDataModel.isEquipped = false;
            currentItemDataModel.canPickUp = false;
            itemDataModels.Add(currentItemDataModel);
        }

        newItem.isObtained = true;

        if (LevelManager.Instance.HasItemName(newItem.itemName))
        {
            LevelManager.Instance.RemoveItemName(newItem.itemName);
        }

        GameManager.Instance.SaveScene();

        if (newItem.itemType == Item.TypeTag.chassis)
        {
            if (newItem.chassisGripTransform.IsGripTransformOccupied())
            {
                Destroy(newItem.chassisGripTransform.GetGripTransformItem().gameObject);
            }
            else
            {
                Destroy(newItem.gameObject);
            }
        }
        else
        {
            Destroy(newItem.gameObject);
        }
        

        if (shouldIncreaseBackpack)
        {
            Player.Instance.backpackFill.IncreaseBackpack(20);
        }
    }

    public void AddScrap(Item newItem)
    {
        ScrapItem currentScrap = newItem.gameObject.GetComponent<ScrapItem>();
        int scrapAmount = currentScrap.scrapAmount;

        if (amountOfScrap + scrapAmount > 9999)
        {
            amountOfScrap = 9999;
        }
        else
        {
            amountOfScrap += scrapAmount;
        }

        DisplayScrapAmount();

        newItem.isObtained = true;

        GameManager.Instance.SaveScene();
        
        Destroy(newItem.gameObject);
    }

    public void RemoveScrapChassis(int chassisIndex, int amountToRemove)
    {
        if (amountOfScrap - amountToRemove < 0)
        {
            return;
        }
        else
        {
            amountOfScrap -= amountToRemove;
            DisplayScrapAmount();

            if (QuestManager.Instance.IsCurrentQuestActive())
            {
                Objective currentObjective = QuestManager.Instance.GetCurrentQuest().GetCurrentObjective();
                if (currentObjective != null)
                {
                    currentObjective.RestoreItem(chassisDataModels[chassisIndex].itemName);
                }
            }

            ChassisDataModel tempChassisDataModel = new ChassisDataModel
            {
                itemName = chassisDataModels[chassisIndex].itemName,
                itemPosition = chassisDataModels[chassisIndex].itemPosition,
                itemRotation = chassisDataModels[chassisIndex].itemRotation,
                canPickUp = chassisDataModels[chassisIndex].canPickUp,
                isObtained = chassisDataModels[chassisIndex].isObtained,
                isRestored = true,
                isEquipped = chassisDataModels[chassisIndex].isEquipped,
                componentItemModels = chassisDataModels[chassisIndex].componentItemModels,
                gripItemModel = chassisDataModels[chassisIndex].gripItemModel
            };
            chassisDataModels[chassisIndex] = tempChassisDataModel;

            ChangeInventoryInformationChassis(chassisDataModels[chassisIndex].itemName);
        }
    }

    public void RemoveScrapItem(int itemIndex, int amountToRemove)
    {
        if (amountOfScrap - amountToRemove < 0)
        {
            return;
        }
        else
        {
            amountOfScrap -= amountToRemove;
            DisplayScrapAmount();

            if (QuestManager.Instance.IsCurrentQuestActive())
            {
                Objective currentObjective = QuestManager.Instance.GetCurrentQuest().GetCurrentObjective();
                if (currentObjective != null)
                {
                    currentObjective.RestoreItem(itemDataModels[itemIndex].itemName);
                }
            }

            ItemDataModel tempItemDataModel = new ItemDataModel
            {
                itemName = itemDataModels[itemIndex].itemName,
                itemPosition = itemDataModels[itemIndex].itemPosition,
                itemRotation = itemDataModels[itemIndex].itemRotation,
                canPickUp = itemDataModels[itemIndex].canPickUp,
                isObtained = itemDataModels[itemIndex].isObtained,
                isRestored = true,
                isEquipped = itemDataModels[itemIndex].isEquipped
            };
            itemDataModels[itemIndex] = tempItemDataModel;

            ChangeInventoryInformationItem(itemDataModels[itemIndex].itemName);
        }
    }

    public void RemoveScrap(int amountToRemove)
    {
        if (amountOfScrap - amountToRemove < 0)
        {
            return;
        }
        else
        {
            amountOfScrap -= amountToRemove;
            DisplayScrapAmount();
        }
    }


    /// <summary>
    /// Update for chassis and item
    /// </summary>
    
    public void DropChassisFromInventory(int dropIndex)
    {
        if (dropIndex == -1)
        {
            return;
        }

        if (playerItemHandler.attachedItem != null && chassisDataModels[dropIndex].isEquipped)
        {
            UnequipItem(dropIndex);
        }

        GameObject currentDropGO = ItemPooler.Instance.InstantiateItemByName(chassisDataModels[dropIndex].itemName);
        Item currentChassisItem = currentDropGO.GetComponent<Item>();
        currentChassisItem.LoadChassisModelInfo(chassisDataModels[dropIndex]);

        if (currentChassisItem.chassisGripTransform.IsGripTransformOccupied())
        {
            currentDropGO.transform.parent = currentChassisItem.chassisGripTransform.GetGripTransformItem().gameObject.transform;
            currentDropGO = currentChassisItem.chassisGripTransform.GetGripTransformItem().gameObject;
        }
        currentDropGO.transform.position = dropTransform.position;
        currentDropGO.transform.rotation = UnityEngine.Random.rotation;

        foreach (Item currentItem in currentDropGO.GetComponentsInChildren<Item>())
        {
            if (currentItem.gameObject == currentDropGO)
            {
                currentItem.gameObject.GetComponent<Rigidbody>().isKinematic = false;
                currentItem.gameObject.GetComponent<Collider>().enabled = true;
                currentItem.gameObject.SetActive(true);
            }
            else
            {
                currentItem.gameObject.GetComponent<Rigidbody>().isKinematic = true;
                currentItem.gameObject.GetComponent<Collider>().enabled = false;
                currentItem.gameObject.SetActive(true);
            }
            
        }

        currentChassisItem.isObtained = false;
        currentChassisItem.canPickUp = true;

        if (!LevelManager.Instance.HasItemName(currentChassisItem.itemName))
        {
            LevelManager.Instance.AddItemName(currentChassisItem.itemName);
        }

        chassisDataModels.RemoveAt(dropIndex);

        ResetSelectedInfo();

        Player.Instance.backpackFill.DecreaseBackpack(20);
        if (chassisDataModels.Count + itemDataModels.Count == 0)
        {
            Player.Instance.backpackFill.DecreaseBackpack(300);
        }

        UpdateInventoryView();
    }

    public void DropItemFromInventory(int dropIndex)
    {
        if (dropIndex == -1)
        {
            return;
        }

        GameObject currentItemGO = ItemPooler.Instance.InstantiateItemByName(itemDataModels[dropIndex].itemName);
        Item currentItem = currentItemGO.GetComponent<Item>();
        currentItem.LoadItemModelInfo(itemDataModels[dropIndex]);


        currentItemGO.transform.position = dropTransform.position;
        currentItemGO.transform.rotation = UnityEngine.Random.rotation;
        currentItemGO.GetComponent<Rigidbody>().isKinematic = false;
        currentItemGO.GetComponent<Collider>().enabled = true;
        currentItemGO.SetActive(true);

        currentItem.isObtained = false;
        currentItem.canPickUp = true;

        if (!LevelManager.Instance.HasItemName(currentItem.itemName))
        {
            LevelManager.Instance.AddItemName(currentItem.itemName);
        }

        itemDataModels.RemoveAt(dropIndex);

        ResetSelectedInfo();

        Player.Instance.backpackFill.DecreaseBackpack(20);
        if (chassisDataModels.Count + itemDataModels.Count == 0)
        {
            Player.Instance.backpackFill.DecreaseBackpack(300);
        }

        UpdateInventoryView();
    }

    public void ChangeInventoryInformationChassis(string currentItemName)
    {
        ResetSelectedInfo();


        int chassisIndex = -1;
        for (int i = 0; i < chassisDataModels.Count; i++)
        {
            if (chassisDataModels[i].itemName == currentItemName)
            {
                chassisIndex = i;
                break;
            }
        }

        if (chassisIndex == -1)
        {
            Debug.Log("Chassis Component Empty!");
            return;
        }

        int restorationAmount;
        Item.TypeTag itemType;
        string description;
        Sprite itemIcon;
        ItemPooler.Instance.GetItemInformation(chassisDataModels[chassisIndex].itemName, out restorationAmount, out itemType, out description, out itemIcon);


        selectedInspectorImage.sprite = itemIcon;
        selectedInspectorImage.color = new Color(1, 1, 1, 1);

        string itemTypeStr = (itemType).ToString();
        itemTypeStr = char.ToUpper(itemTypeStr[0]) + itemTypeStr.Substring(1);
        selectedItemTitle.text = $"{chassisDataModels[chassisIndex].itemName} ({itemType})";

        if (chassisDataModels[chassisIndex].isRestored)
        {
            if (chassisDataModels[chassisIndex].isEquipped)
            {
                equipItemButton.SetActive(false);
                unequipItemButton.SetActive(true);
            }
            else
            {
                unequipItemButton.SetActive(false);
                equipItemButton.SetActive(true);
            }
            equipItemButton.GetComponent<Button>().onClick.AddListener(delegate { EquipItem(chassisIndex); });
            unequipItemButton.GetComponent<Button>().onClick.AddListener(delegate { UnequipItem(chassisIndex); });
        }
        else
        {
            restoreItemButton.SetActive(true);
            restoreItemButton.GetComponentInChildren<Button>().onClick.AddListener(delegate { RemoveScrapChassis(chassisIndex, restorationAmount); });
            restoreItemButton.GetComponentInChildren<Button>().onClick.AddListener(delegate { UpdateInventoryView(); });
            restoreItemButton.GetComponentInChildren<TMPro.TextMeshProUGUI>().text = $"Restore: {restorationAmount}";
        }
            
        selectedItemDescription.text = description;
        dropItemButton.SetActive(true);
        dropItemButton.GetComponentInChildren<Button>().onClick.AddListener(delegate { DropChassisFromInventory(chassisIndex); });
    }

    public void ChangeInventoryInformationItem(string currentItemName)
    {
        ResetSelectedInfo();


        int itemIndex = -1;
        for (int i = 0; i < itemDataModels.Count; i++)
        {
            if (itemDataModels[i].itemName == currentItemName)
            {
                itemIndex = i;
                break;
            }
        }

        if (itemIndex == -1)
        {
            Debug.Log("Item Component Empty!");
            return;
        }

        int restorationAmount;
        Item.TypeTag itemType;
        string description;
        Sprite itemIcon;
        ItemPooler.Instance.GetItemInformation(itemDataModels[itemIndex].itemName, out restorationAmount, out itemType, out description, out itemIcon);


        selectedInspectorImage.sprite = itemIcon;
        selectedInspectorImage.color = new Color(1, 1, 1, 1);

        string itemTypeStr = (itemType).ToString();
        itemTypeStr = char.ToUpper(itemTypeStr[0]) + itemTypeStr.Substring(1);
        selectedItemTitle.text = $"{itemDataModels[itemIndex].itemName} ({itemType})";

        if (!itemDataModels[itemIndex].isRestored)
        {
            restoreItemButton.SetActive(true);
            restoreItemButton.GetComponentInChildren<Button>().onClick.AddListener(delegate { RemoveScrapItem(itemIndex, restorationAmount); });
            restoreItemButton.GetComponentInChildren<Button>().onClick.AddListener(delegate { UpdateInventoryView(); });
            restoreItemButton.GetComponentInChildren<TMPro.TextMeshProUGUI>().text = $"Restore: {restorationAmount}";
        }

        selectedItemDescription.text = description;
        dropItemButton.SetActive(true);
        dropItemButton.GetComponentInChildren<Button>().onClick.AddListener(delegate { DropItemFromInventory(itemIndex); });
    }
    
    public bool Contains(string itemName)
    {
        for (int i = 0; i < chassisDataModels.Count; i++)
        {
            if (chassisDataModels[i].itemName == itemName)
            {
                return true;
            }

            for (int j = 0; j < chassisDataModels[i].componentItemModels.Count; j++)
            {
                if (chassisDataModels[i].componentItemModels[j].HasValue)
                {
                    if (chassisDataModels[i].componentItemModels[j].Value.itemName == itemName)
                    {
                        return true;
                    }
                }
            }

            if (chassisDataModels[i].gripItemModel.HasValue)
            {
                if(chassisDataModels[i].gripItemModel.Value.itemName == itemName)
                {
                    return true;
                }
            }
        }

        for (int i = 0; i < itemDataModels.Count; i++)
        {
            if (itemDataModels[i].itemName == itemName)
            {
                return true;
            }
        }

        return false;
    }

    public bool IsItemRestored(string itemName)
    {
        for (int i = 0; i < chassisDataModels.Count; i++)
        {
            if (chassisDataModels[i].itemName == itemName)
            {
                return chassisDataModels[i].isRestored;
            }

            for (int j = 0; j < chassisDataModels[i].componentItemModels.Count; j++)
            {
                if (chassisDataModels[i].componentItemModels[j].HasValue)
                {
                    if (chassisDataModels[i].componentItemModels[j].Value.itemName == itemName)
                    {
                        return chassisDataModels[i].componentItemModels[j].Value.isRestored;
                    }
                }
            }

            if (chassisDataModels[i].gripItemModel.HasValue)
            {
                if (chassisDataModels[i].gripItemModel.Value.itemName == itemName)
                {
                    return chassisDataModels[i].gripItemModel.Value.isRestored;
                }
            }
        }

        for (int i = 0; i < itemDataModels.Count; i++)
        {
            if (itemDataModels[i].itemName == itemName)
            {
                return itemDataModels[i].isRestored;
            }
        }

        return false;
    }

    public void EquipItem(int equipIndex)
    {
        if (equipIndex == -1)
        {
            return;
        }

        ChassisDataModel tempChassisDataModel = new ChassisDataModel
        {
            itemName = chassisDataModels[equipIndex].itemName,
            itemPosition = chassisDataModels[equipIndex].itemPosition,
            itemRotation = chassisDataModels[equipIndex].itemRotation,
            canPickUp = chassisDataModels[equipIndex].canPickUp,
            isObtained = chassisDataModels[equipIndex].isObtained,
            isRestored = chassisDataModels[equipIndex].isRestored,
            isEquipped = true,
            componentItemModels = chassisDataModels[equipIndex].componentItemModels,
            gripItemModel = chassisDataModels[equipIndex].gripItemModel
        };
        chassisDataModels[equipIndex] = tempChassisDataModel;

        GameObject chassisToEquip = ItemPooler.Instance.InstantiateItemByName(chassisDataModels[equipIndex].itemName);
        Item currentChassisItem = chassisToEquip.GetComponent<Item>();
        currentChassisItem.LoadChassisModelInfo(chassisDataModels[equipIndex]);

        if (currentChassisItem.chassisGripTransform.IsGripTransformOccupied())
        {
            playerItemHandler.EquipItem(currentChassisItem.chassisGripTransform.GetGripTransformItem(), true);
        }
        else
        {
            playerItemHandler.EquipItem(currentChassisItem, true);
        }

        currentChassisItem.OnEquip();
        currentEquippedGO = chassisToEquip;

        equipItemButton.SetActive(false);
        unequipItemButton.SetActive(true);
    }

    public void UnequipItem(int unequipIndex)
    {
        if (unequipIndex == -1)
        {
            return;
        }

        Item currentChassisItem = currentEquippedGO.GetComponent<Item>();
        GameObject gameObjectToDestroy = currentEquippedGO;

        if (currentChassisItem.chassisGripTransform.IsGripTransformOccupied())
        {
            playerItemHandler.UnequipItem(currentChassisItem.chassisGripTransform.GetGripTransformItem());
            gameObjectToDestroy = currentChassisItem.chassisGripTransform.GetGripTransformItem().gameObject;
            currentChassisItem.isEquipped = false;
        }
        else
        {
            playerItemHandler.UnequipItem(currentChassisItem);
        }

        for (int i = 0; i < currentChassisItem.chassisComponentTransforms.Count; i++)
        {
            if (currentChassisItem.chassisComponentTransforms[i].IsComponentTransformOccupied())
            {
                GrabberEffector grabberEffector = currentChassisItem.chassisComponentTransforms[i].GetComponentTransformItem().gameObject.GetComponent<GrabberEffector>();

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

        ChassisDataModel tempChassisDataModel = new ChassisDataModel
        {
            itemName = chassisDataModels[unequipIndex].itemName,
            itemPosition = chassisDataModels[unequipIndex].itemPosition,
            itemRotation = chassisDataModels[unequipIndex].itemRotation,
            canPickUp = chassisDataModels[unequipIndex].canPickUp,
            isObtained = chassisDataModels[unequipIndex].isObtained,
            isRestored = chassisDataModels[unequipIndex].isRestored,
            isEquipped = false,
            componentItemModels = chassisDataModels[unequipIndex].componentItemModels,
            gripItemModel = chassisDataModels[unequipIndex].gripItemModel
        };
        chassisDataModels[unequipIndex] = tempChassisDataModel;
        currentChassisItem.OnUnequip();

        Destroy(gameObjectToDestroy);
        currentEquippedGO = null;

        unequipItemButton.SetActive(false);
        equipItemButton.SetActive(true);
    }

    void UpdateInventoryView()
    {
        DeactivateCurrentInventoryView();

        for (int i = 0; i < chassisDataModels.Count; i++)
        {
            GameObject currentItemBox = ObjectPooler.GetPooler(inventoryItemUIKey).GetPooledObject();
            currentItemBox.transform.SetParent(inventoryItemPanel.transform, false);
            string currentItemName = chassisDataModels[i].itemName;
            
            Sprite currentItemSprite;
            ItemPooler.Instance.GetItemSprite(currentItemName, out currentItemSprite);


            currentItemBox.GetComponentInChildren<InventoryItemBox>().SetInventoryIcon(currentItemSprite, chassisDataModels[i].isRestored);
            currentItemBox.GetComponentInChildren<Button>().onClick.AddListener(delegate { ChangeInventoryInformationChassis(currentItemName); });
            currentItemBox.SetActive(true);
        }

        for (int i = 0; i < itemDataModels.Count; i++)
        {
            GameObject currentItemBox = ObjectPooler.GetPooler(inventoryItemUIKey).GetPooledObject();
            currentItemBox.transform.SetParent(inventoryItemPanel.transform, false);
            string currentItemName = itemDataModels[i].itemName;

            Sprite currentItemSprite;
            ItemPooler.Instance.GetItemSprite(currentItemName, out currentItemSprite);


            currentItemBox.GetComponentInChildren<InventoryItemBox>().SetInventoryIcon(currentItemSprite, itemDataModels[i].isRestored);
            currentItemBox.GetComponentInChildren<Button>().onClick.AddListener(delegate { ChangeInventoryInformationItem(currentItemName); });
            currentItemBox.SetActive(true);
        }

        inventoryPanelGridLayout.cellSize.y = inventoryPanelGridLayout.cellSize.x;
    }

    void DeactivateCurrentInventoryView()
    {
        foreach(Transform currentTrans in inventoryItemPanel.GetComponentsInChildren<Transform>())
        {
            if(currentTrans == inventoryItemPanel.GetComponent<Transform>())
            {
                continue;
            }

            if (currentTrans.gameObject.GetComponent<Button>() != null)
            {
                currentTrans.gameObject.GetComponent<Button>().onClick.RemoveAllListeners();
                currentTrans.gameObject.GetComponentInChildren<Image>().sprite = null;
                currentTrans.gameObject.SetActive(false);
                currentTrans.SetParent(ObjectPooler.GetPooler(inventoryItemUIKey).gameObject.transform, false);
            }
        }
    }
}
