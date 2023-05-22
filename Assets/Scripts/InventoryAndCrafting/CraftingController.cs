using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class CraftingController : MonoBehaviour
{
    [Header("Crafting UI Variables")]
    public GameObject craftingPanel;
    public GameObject warningPanel;
    public GameObject tooltip;
    public GameObject primaryCraftingList;
    public GameObject modItemPrefab;
    public GameObject craftingModButtonPrefab;
    public GameObject resetCraftingButtonPrefab;

    private GameObject resetButton;
    private bool isActive = false;
    private int currentChassisIndex = -1;
    private bool warningCalled = false;

    private List<ItemDataModel> effectorList = new List<ItemDataModel>();
    private List<ItemDataModel> modifierList = new List<ItemDataModel>();
    private List<ItemDataModel> ammoList = new List<ItemDataModel>();
    private List<ItemDataModel> gripList = new List<ItemDataModel>();


    [Header("Item Viewer Variables")]
    public Camera itemViewerCamera;
    public ItemViewer itemViewer;
    public GameObject itemViewerPlayerModel;
    [Range(10,360)]
    public int playerModelAngularSpeed = 10;
    private Quaternion originalPlayerModelRotation;

    private ObjectPooler.Key primaryButtonUIKey = ObjectPooler.Key.PrimaryCraftingUIButtons;
    private ObjectPooler.Key secondaryButtonUIKey = ObjectPooler.Key.SecondaryCraftingUIButtons;

    private void Start()
    {
        originalPlayerModelRotation = itemViewerPlayerModel.transform.rotation;

        resetButton = Instantiate(resetCraftingButtonPrefab);
        resetButton.transform.SetParent(gameObject.transform, false);
        resetButton.SetActive(false);

        RectTransform modItemRect = modItemPrefab.GetComponent<RectTransform>();
        craftingModButtonPrefab.GetComponent<PrimaryCraftingUIDescriptor>().SetUpList(modItemRect.sizeDelta.y);

        OnDisableCraftingPanel();
    }

    void OnEnableCraftingPanel()
    {
        for (int i = 0; i < Inventory.Instance.chassisDataModels.Count; i++)
        {
            if (!Inventory.Instance.chassisDataModels[i].isRestored)
            {
                continue;
            }

            for (int j = 0; j < Inventory.Instance.chassisDataModels[i].componentItemModels.Count; j++)
            {
                if (Inventory.Instance.chassisDataModels[i].componentItemModels[j].HasValue)
                {
                    Item.TypeTag currentComponentType;
                    ItemPooler.Instance.GetItemType(Inventory.Instance.chassisDataModels[i].componentItemModels[j].Value.itemName, out currentComponentType);
                    switch (currentComponentType)
                    {
                        case Item.TypeTag.effector:
                            effectorList.Add(Inventory.Instance.chassisDataModels[i].componentItemModels[j].Value);
                            break;
                        case Item.TypeTag.modifier:
                            modifierList.Add(Inventory.Instance.chassisDataModels[i].componentItemModels[j].Value);
                            break;
                        case Item.TypeTag.ammo:
                            ammoList.Add(Inventory.Instance.chassisDataModels[i].componentItemModels[j].Value);
                            break;
                    }
                }
            }

            if (Inventory.Instance.chassisDataModels[i].gripItemModel.HasValue)
            {
                gripList.Add(Inventory.Instance.chassisDataModels[i].gripItemModel.Value);
            }
        }

        for (int i = 0; i < Inventory.Instance.itemDataModels.Count; i++)
        {
            if (!Inventory.Instance.itemDataModels[i].isRestored)
            {
                continue;
            }

            Item.TypeTag currentItemType;
            ItemPooler.Instance.GetItemType(Inventory.Instance.itemDataModels[i].itemName, out currentItemType);

            switch (currentItemType)
            {
                case Item.TypeTag.effector:
                    effectorList.Add(Inventory.Instance.itemDataModels[i]);
                    break;
                case Item.TypeTag.modifier:
                    modifierList.Add(Inventory.Instance.itemDataModels[i]);
                    break;
                case Item.TypeTag.ammo:
                    ammoList.Add(Inventory.Instance.itemDataModels[i]);
                    break;
                case Item.TypeTag.grip:
                    gripList.Add(Inventory.Instance.itemDataModels[i]);
                    break;
            }
        }

        int startingChassis = -1;
        for (int i = 0; i < Inventory.Instance.chassisDataModels.Count; i++)
        {
            if (Inventory.Instance.chassisDataModels[i].isEquipped)
            {
                startingChassis = i;
            }
        }

        ChooseNewChassis(startingChassis);
    }

    void OnDisableCraftingPanel()
    {
        DisableWholeVisualChassis();

        for (int i = 0; i < Inventory.Instance.chassisDataModels.Count; i++)
        {
            DisableVisualItem(ItemPooler.Instance.visualItemDictionary[Inventory.Instance.chassisDataModels[i].itemName]);
        }

        effectorList.Clear();
        modifierList.Clear();
        ammoList.Clear();
        gripList.Clear();

        currentChassisIndex = -1;

        itemViewerPlayerModel.transform.rotation = originalPlayerModelRotation;
        itemViewer.SwitchPlayerAnimLayer(0);

        tooltip.GetComponent<Tooltip>().DisableTooltip();

        ToggleAllPrimaryCraftingLists(true);
        
        warningCalled = false;
        warningPanel.SetActive(false);
    }

    void DestroyPrimaryCraftingList()
    {
        foreach(Transform currentCraftingModButton in primaryCraftingList.GetComponentsInChildren<Transform>())
        {
            if (currentCraftingModButton.gameObject.GetComponent<PrimaryCraftingUIDescriptor>() == null)
            {
                continue;
            }

            foreach(Transform secondaryCraftingModButton in currentCraftingModButton.gameObject.GetComponent<PrimaryCraftingUIDescriptor>().internalCraftingList.GetComponentsInChildren<Transform>())
            {
                if (secondaryCraftingModButton.GetComponent<ItemUIDescriptor>() == null)
                {
                    continue;
                }

                secondaryCraftingModButton.gameObject.GetComponent<ItemUIDescriptor>().ApplyDescriptors(null, "None");
                secondaryCraftingModButton.gameObject.GetComponentInChildren<Button>().onClick.RemoveAllListeners();
                secondaryCraftingModButton.gameObject.GetComponentInChildren<EventTrigger>().triggers.Clear();
                secondaryCraftingModButton.SetParent(ObjectPooler.GetPooler(secondaryButtonUIKey).gameObject.transform, false);
                secondaryCraftingModButton.gameObject.SetActive(false);
            }

            currentCraftingModButton.gameObject.GetComponent<PrimaryCraftingUIDescriptor>().SetButtonInformation("ERROR", "None", null);
            currentCraftingModButton.gameObject.GetComponent<PrimaryCraftingUIDescriptor>().ResetSecondaryCraftingRect();
            currentCraftingModButton.gameObject.GetComponentInChildren<Button>().onClick.RemoveAllListeners();
            currentCraftingModButton.SetParent(ObjectPooler.GetPooler(primaryButtonUIKey).gameObject.transform, false);
            currentCraftingModButton.gameObject.SetActive(false);
        }

        resetButton.GetComponentInChildren<Button>().onClick.RemoveAllListeners();
        resetButton.transform.SetParent(gameObject.transform, false);
        resetButton.SetActive(false);
    }

    void ToggleAllPrimaryCraftingLists(bool setActive)
    {
        foreach (Button currentCraftingModButton in primaryCraftingList.GetComponentsInChildren<Button>(true))
        {
            currentCraftingModButton.interactable = setActive;
        }

        resetButton.GetComponentInChildren<Button>(true).interactable = setActive;
    }

    void WarningResult()
    {
        warningCalled = true;
    }

    void DisableVisualItem(GameObject visualItem)
    {
        visualItem.SetActive(false);
        visualItem.transform.SetParent(ItemPooler.Instance.visualItemParent.transform);
    }

    void DisableWholeVisualChassis()
    {
        if (currentChassisIndex != -1)
        {
            for (int i = 0; i < Inventory.Instance.chassisDataModels[currentChassisIndex].componentItemModels.Count; i++)
            {
                if (Inventory.Instance.chassisDataModels[currentChassisIndex].componentItemModels[i].HasValue)
                {
                    DisableVisualItem(ItemPooler.Instance.visualItemDictionary[Inventory.Instance.chassisDataModels[currentChassisIndex].componentItemModels[i].Value.itemName]);
                }
            }

            if (Inventory.Instance.chassisDataModels[currentChassisIndex].gripItemModel.HasValue)
            {
                DisableVisualItem(ItemPooler.Instance.visualItemDictionary[Inventory.Instance.chassisDataModels[currentChassisIndex].gripItemModel.Value.itemName]);
            }

            ItemPooler.Instance.visualItemDictionary[Inventory.Instance.chassisDataModels[currentChassisIndex].itemName].SetActive(false);
        }
    }

    void EnableVisualItem(GameObject visualItem, Transform visualParent, Vector3 localPosition, Quaternion localRotation, bool useLocalRot = false)
    {
        visualItem.transform.parent = visualParent;
        visualItem.transform.localPosition = localPosition;
        if (useLocalRot)
        {
            visualItem.transform.localRotation = localRotation;
        }
        else
        {
            visualItem.transform.rotation = localRotation;
        }
        visualItem.SetActive(true);
    }

    void EnableVisualEquippedItem(GameObject visualItem, Transform visualParent, Vector3 localPosition, Vector3 localRotation)
    {
        visualItem.transform.parent = visualParent;
        visualItem.transform.localPosition = localPosition;
        visualItem.transform.localRotation = Quaternion.Euler(localRotation);
        visualItem.SetActive(true);
    }

    //int FindInventoryIndex(Item itemToFind)
    //{
    //    for (int i = 0; i < Inventory.Instance.inventory.Count; i++)
    //    {
    //        if (Inventory.Instance.inventory[i].gameObject == itemToFind.gameObject)
    //        {
    //            return i;
    //        }
    //    }

    //    return -1;
    //}

    /// <summary>
    /// Sets a new chassis as the target of the crafting system.
    /// </summary>
    /// <param name="index"></param>
    void ChooseNewChassis(int index)
    {
        DestroyPrimaryCraftingList();

        float heightSpacing = 0;
        if (index == -1)
        {
            GameObject chassisNoneSecondaryCraftingList = SpawnPrimaryButton("Chassis", "None", null,ref heightSpacing);
            SpawnSecondaryButtons(Item.TypeTag.chassis, chassisNoneSecondaryCraftingList.transform);

            DisableWholeVisualChassis();
            itemViewer.SwitchPlayerAnimLayer(0);

            currentChassisIndex = -1;
            return;
        }
        else
        {
            DisableWholeVisualChassis();
            ///Get current chassis
            currentChassisIndex = index;

            ///Visualize current chassis
            GameObject visualChassis = ItemPooler.Instance.visualItemDictionary[Inventory.Instance.chassisDataModels[currentChassisIndex].itemName];
            if (Inventory.Instance.chassisDataModels[currentChassisIndex].gripItemModel.HasValue)
            {
                GameObject visualGrip = ItemPooler.Instance.visualItemDictionary[Inventory.Instance.chassisDataModels[currentChassisIndex].gripItemModel.Value.itemName];

                Vector3 localHandPos, localHandRot;
                ItemPooler.Instance.GetItemHandLocals(Inventory.Instance.chassisDataModels[currentChassisIndex].gripItemModel.Value.itemName, out localHandPos, out localHandRot);

                EnableVisualEquippedItem(visualGrip, itemViewer.handAttachment, localHandPos, localHandRot);
                EnableVisualItem(visualChassis, visualGrip.transform, visualChassis.GetComponent<ChassisVisualItem>().GetVisualGripTransform().localPosition, Quaternion.Euler(0, localHandRot.y, 0), true);

                GripItem.GripType gripType;
                ItemPooler.Instance.GetGripAnimEnum(Inventory.Instance.chassisDataModels[currentChassisIndex].gripItemModel.Value.itemName, out gripType);
                itemViewer.SwitchPlayerAnimLayer((int)gripType);
            }
            else
            {
                Vector3 localHandPos, localHandRot;
                ItemPooler.Instance.GetItemHandLocals(Inventory.Instance.chassisDataModels[currentChassisIndex].itemName, out localHandPos, out localHandRot);
                EnableVisualEquippedItem(visualChassis, itemViewer.handAttachment, localHandPos, localHandRot);

                itemViewer.SwitchPlayerAnimLayer(0);
            }

            ///Spawn component buttons
            List<PrimaryCraftingUIDescriptor> resetComponentPrimaryButtons = new List<PrimaryCraftingUIDescriptor>();
            List<ItemDataModel> currentComponentList = new List<ItemDataModel>();
            for (int i = 0; i < effectorList.Count; i++)
            {
                currentComponentList.Add(effectorList[i]);
            }

            for (int i = 0; i < modifierList.Count; i++)
            {
                currentComponentList.Add(modifierList[i]);
            }

            for (int i = 0; i < ammoList.Count; i++)
            {
                currentComponentList.Add(ammoList[i]);
            }

            for (int i = 0; i < Inventory.Instance.chassisDataModels[currentChassisIndex].componentItemModels.Count; i++)
            {
                int currentcomponentTransformIndex = i;
                string componentItemName = "None";
                Sprite componentItemIcon = null;
                bool currentPointIsOccupied = false;

                if (Inventory.Instance.chassisDataModels[currentChassisIndex].componentItemModels[currentcomponentTransformIndex].HasValue)
                {
                    componentItemName = Inventory.Instance.chassisDataModels[currentChassisIndex].componentItemModels[currentcomponentTransformIndex].Value.itemName;
                    ItemPooler.Instance.GetItemSprite(componentItemName, out componentItemIcon);
                    currentPointIsOccupied = true;
                }

                GameObject componentSecondaryCraftingList = 
                    SpawnPrimaryButton($"Component { i + 1 }", componentItemName, componentItemIcon, ref heightSpacing);
                    SpawnMultiComponentSecondaryButtons(currentComponentList, componentSecondaryCraftingList.transform, currentcomponentTransformIndex);

                if (currentPointIsOccupied)
                {
                    GameObject visualEffector = ItemPooler.Instance.visualItemDictionary[Inventory.Instance.chassisDataModels[currentChassisIndex].componentItemModels[currentcomponentTransformIndex].Value.itemName];
                    EnableVisualItem(visualEffector, visualChassis.transform, visualChassis.GetComponent<ChassisVisualItem>().GetVisualComponentTransforms()[currentcomponentTransformIndex].localPosition, visualChassis.GetComponent<ChassisVisualItem>().GetVisualComponentTransforms()[currentcomponentTransformIndex].rotation);
                }

                resetComponentPrimaryButtons.Add(componentSecondaryCraftingList.gameObject.GetComponentInParent<PrimaryCraftingUIDescriptor>());
            }

            ///Spawn chassis button
            Sprite chassisSprite;
            ItemPooler.Instance.GetItemSprite(Inventory.Instance.chassisDataModels[currentChassisIndex].itemName, out chassisSprite);
            GameObject chassisSecondaryCraftingList =
                SpawnPrimaryButton("Chassis", Inventory.Instance.chassisDataModels[currentChassisIndex].itemName, chassisSprite, ref heightSpacing);
                SpawnSecondaryButtons(Item.TypeTag.chassis, chassisSecondaryCraftingList.transform);
            PrimaryCraftingUIDescriptor resetChassisPrimaryButton = chassisSecondaryCraftingList.GetComponentInParent<PrimaryCraftingUIDescriptor>();

            ///Spawn grip button
            string gripItemName = "None";
            Sprite gripItemIcon = null;
            if (Inventory.Instance.chassisDataModels[currentChassisIndex].gripItemModel.HasValue)
            {
                gripItemName = Inventory.Instance.chassisDataModels[currentChassisIndex].gripItemModel.Value.itemName;
                ItemPooler.Instance.GetItemSprite(Inventory.Instance.chassisDataModels[currentChassisIndex].gripItemModel.Value.itemName, out gripItemIcon);
            }
            GameObject gripSecondaryCraftingList = 
                SpawnPrimaryButton("Grip", gripItemName, gripItemIcon, ref heightSpacing);
                SpawnSecondaryButtons(Item.TypeTag.grip, gripSecondaryCraftingList.transform);
            
            PrimaryCraftingUIDescriptor resetGripPrimaryButton = gripSecondaryCraftingList.gameObject.GetComponentInParent<PrimaryCraftingUIDescriptor>();

            resetButton.GetComponentInChildren<Button>().onClick.AddListener(delegate { ResetCurrentChassis(resetChassisPrimaryButton, resetComponentPrimaryButtons, resetGripPrimaryButton); });
            resetButton.transform.SetParent(primaryCraftingList.transform, false);
            resetButton.SetActive(true);
        }
    }

    void ResetCurrentChassis(PrimaryCraftingUIDescriptor chassisPrimaryButton, List<PrimaryCraftingUIDescriptor> componentPrimaryButtons, PrimaryCraftingUIDescriptor gripPrimaryButton)
    {
        string warning = "You are about to remove everything from this chassis!";
        if (!warningCalled)
        {
            warningPanel.SetActive(true);
            ToggleAllPrimaryCraftingLists(false);
            warningPanel.GetComponent<WarningMessageUI>().SetWarning(warning, delegate { WarningResult(); }, delegate { WarningResult(); });
            warningPanel.GetComponent<WarningMessageUI>().AddWarningDelegate(delegate { ResetCurrentChassis(chassisPrimaryButton, componentPrimaryButtons, gripPrimaryButton); warningCalled = false; ToggleAllPrimaryCraftingLists(true); }, delegate { warningCalled = false; ToggleAllPrimaryCraftingLists(true); });
        }
        else
        {
            chassisPrimaryButton.secondaryCraftingList.gameObject.SetActive(false);

            GameObject visualChassis = ItemPooler.Instance.visualItemDictionary[Inventory.Instance.chassisDataModels[currentChassisIndex].itemName];
            DisableVisualItem(visualChassis);
            Vector3 chassisLocalHandPos, chassisLocalHandRot;
            ItemPooler.Instance.GetItemHandLocals(Inventory.Instance.chassisDataModels[currentChassisIndex].itemName, out chassisLocalHandPos, out chassisLocalHandRot);
            EnableVisualEquippedItem(visualChassis, itemViewer.handAttachment, chassisLocalHandPos, chassisLocalHandRot);
            itemViewer.SwitchPlayerAnimLayer(0);

            for (int i = 0; i < Inventory.Instance.chassisDataModels[currentChassisIndex].componentItemModels.Count; i++)
            {
                componentPrimaryButtons[i].SetButtonInformation($"Component {i + 1}", "None", null);
                componentPrimaryButtons[i].secondaryCraftingList.gameObject.SetActive(false);

                if (Inventory.Instance.chassisDataModels[currentChassisIndex].componentItemModels[i].HasValue)
                {
                    DisableVisualItem(ItemPooler.Instance.visualItemDictionary[Inventory.Instance.chassisDataModels[currentChassisIndex].componentItemModels[i].Value.itemName]);

                    ///Removes component for current slot if there is one.
                    if (Inventory.Instance.chassisDataModels[currentChassisIndex].isEquipped)
                    {
                        Item currentComponentItem = Inventory.Instance.currentEquippedGO.GetComponent<Item>();

                        Inventory.Instance.AddToInventory(currentComponentItem.chassisComponentTransforms[i].GetComponentTransformItem(), false);
                        currentComponentItem.chassisComponentTransforms[i].ResetComponentTransform();
                    }
                    else
                    {
                        ItemDataModel tempItemDataModel = new ItemDataModel
                        {
                            itemName = Inventory.Instance.chassisDataModels[currentChassisIndex].componentItemModels[i].Value.itemName,
                            itemPosition = Inventory.Instance.chassisDataModels[currentChassisIndex].componentItemModels[i].Value.itemPosition,
                            itemRotation = Inventory.Instance.chassisDataModels[currentChassisIndex].componentItemModels[i].Value.itemRotation,
                            canPickUp = Inventory.Instance.chassisDataModels[currentChassisIndex].componentItemModels[i].Value.canPickUp,
                            isObtained = Inventory.Instance.chassisDataModels[currentChassisIndex].componentItemModels[i].Value.isObtained,
                            isRestored = Inventory.Instance.chassisDataModels[currentChassisIndex].componentItemModels[i].Value.isRestored,
                            isEquipped = false,
                        };

                        Inventory.Instance.chassisDataModels[currentChassisIndex].componentItemModels[i] = tempItemDataModel;
                        Inventory.Instance.itemDataModels.Add(Inventory.Instance.chassisDataModels[currentChassisIndex].componentItemModels[i].Value);
                    }

                    Inventory.Instance.chassisDataModels[currentChassisIndex].componentItemModels[i] = null;
                }
            }

            gripPrimaryButton.SetButtonInformation("Grip", "None", null);
            gripPrimaryButton.secondaryCraftingList.gameObject.SetActive(false);

            if (Inventory.Instance.chassisDataModels[currentChassisIndex].gripItemModel.HasValue)
            {
                DisableVisualItem(ItemPooler.Instance.visualItemDictionary[Inventory.Instance.chassisDataModels[currentChassisIndex].gripItemModel.Value.itemName]);

                ///Removes grip from current slot if there is one.
                if (Inventory.Instance.chassisDataModels[currentChassisIndex].isEquipped)
                {
                    Item currentComponentItem = Inventory.Instance.currentEquippedGO.GetComponent<Item>();
                    Inventory.Instance.playerItemHandler.EquipItem(currentComponentItem);

                    Inventory.Instance.AddToInventory(currentComponentItem.chassisGripTransform.GetGripTransformItem(), false);
                    currentComponentItem.chassisGripTransform.ResetGripTransform();
                }
                else
                {
                    ItemDataModel? tempItemDataModel = new ItemDataModel
                    {
                        itemName = Inventory.Instance.chassisDataModels[currentChassisIndex].gripItemModel.Value.itemName,
                        itemPosition = Inventory.Instance.chassisDataModels[currentChassisIndex].gripItemModel.Value.itemPosition,
                        itemRotation = Inventory.Instance.chassisDataModels[currentChassisIndex].gripItemModel.Value.itemRotation,
                        canPickUp = Inventory.Instance.chassisDataModels[currentChassisIndex].gripItemModel.Value.canPickUp,
                        isObtained = Inventory.Instance.chassisDataModels[currentChassisIndex].gripItemModel.Value.isObtained,
                        isRestored = Inventory.Instance.chassisDataModels[currentChassisIndex].gripItemModel.Value.isRestored,
                        isEquipped = false,
                    };

                    ChassisDataModel tempChassisGripDataModel = new ChassisDataModel
                    {
                        itemName = Inventory.Instance.chassisDataModels[currentChassisIndex].itemName,
                        itemPosition = Inventory.Instance.chassisDataModels[currentChassisIndex].itemPosition,
                        itemRotation = Inventory.Instance.chassisDataModels[currentChassisIndex].itemRotation,
                        canPickUp = Inventory.Instance.chassisDataModels[currentChassisIndex].canPickUp,
                        isObtained = Inventory.Instance.chassisDataModels[currentChassisIndex].isObtained,
                        isRestored = Inventory.Instance.chassisDataModels[currentChassisIndex].isRestored,
                        isEquipped = Inventory.Instance.chassisDataModels[currentChassisIndex].isEquipped,
                        componentItemModels = Inventory.Instance.chassisDataModels[currentChassisIndex].componentItemModels,
                        gripItemModel = tempItemDataModel,
                    };

                    Inventory.Instance.chassisDataModels[currentChassisIndex] = tempChassisGripDataModel;
                    Inventory.Instance.itemDataModels.Add(Inventory.Instance.chassisDataModels[currentChassisIndex].gripItemModel.Value);
                }

                ChassisDataModel tempChassisDataModel = new ChassisDataModel
                {
                    itemName = Inventory.Instance.chassisDataModels[currentChassisIndex].itemName,
                    itemPosition = Inventory.Instance.chassisDataModels[currentChassisIndex].itemPosition,
                    itemRotation = Inventory.Instance.chassisDataModels[currentChassisIndex].itemRotation,
                    canPickUp = Inventory.Instance.chassisDataModels[currentChassisIndex].canPickUp,
                    isObtained = Inventory.Instance.chassisDataModels[currentChassisIndex].isObtained,
                    isRestored = Inventory.Instance.chassisDataModels[currentChassisIndex].isRestored,
                    isEquipped = Inventory.Instance.chassisDataModels[currentChassisIndex].isEquipped,
                    componentItemModels = Inventory.Instance.chassisDataModels[currentChassisIndex].componentItemModels,
                    gripItemModel = null,
                };

                Inventory.Instance.chassisDataModels[currentChassisIndex] = tempChassisDataModel;
            }
        }
    }
    
    /// <summary>
    /// Applies a new component to the current selected component slot.
    /// </summary>
    /// <param name="componentIndex"></param>
    /// <param name="componentList"></param>
    /// <param name="componentTransformIndex"></param>
    /// <param name="parentButton"></param>
    void ChooseNewComponent(int componentIndex, List<ItemDataModel> componentList, int componentTransformIndex, GameObject parentButton)
    {
        if (componentIndex == -1)
        {   
            parentButton.GetComponentInParent<PrimaryCraftingUIDescriptor>().SetButtonInformation($"Component { componentTransformIndex + 1 }", "None", null);
            
            if (Inventory.Instance.chassisDataModels[currentChassisIndex].componentItemModels[componentTransformIndex].HasValue)
            {
                DisableVisualItem(ItemPooler.Instance.visualItemDictionary[Inventory.Instance.chassisDataModels[currentChassisIndex].componentItemModels[componentTransformIndex].Value.itemName]);

                ///Removes component for current slot if there is one.
                if (Inventory.Instance.chassisDataModels[currentChassisIndex].isEquipped)
                {
                    Item currentComponentItem = Inventory.Instance.currentEquippedGO.GetComponent<Item>();

                    Inventory.Instance.AddToInventory(currentComponentItem.chassisComponentTransforms[componentTransformIndex].GetComponentTransformItem(), false);
                    currentComponentItem.chassisComponentTransforms[componentTransformIndex].ResetComponentTransform();
                }
                else
                {
                    ItemDataModel tempItemDataModel = new ItemDataModel
                    {
                        itemName = Inventory.Instance.chassisDataModels[currentChassisIndex].componentItemModels[componentTransformIndex].Value.itemName,
                        itemPosition = Inventory.Instance.chassisDataModels[currentChassisIndex].componentItemModels[componentTransformIndex].Value.itemPosition,
                        itemRotation = Inventory.Instance.chassisDataModels[currentChassisIndex].componentItemModels[componentTransformIndex].Value.itemRotation,
                        canPickUp = Inventory.Instance.chassisDataModels[currentChassisIndex].componentItemModels[componentTransformIndex].Value.canPickUp,
                        isObtained = Inventory.Instance.chassisDataModels[currentChassisIndex].componentItemModels[componentTransformIndex].Value.isObtained,
                        isRestored = Inventory.Instance.chassisDataModels[currentChassisIndex].componentItemModels[componentTransformIndex].Value.isRestored,
                        isEquipped = false,
                    };

                    Inventory.Instance.chassisDataModels[currentChassisIndex].componentItemModels[componentTransformIndex] = tempItemDataModel;
                    Inventory.Instance.itemDataModels.Add(Inventory.Instance.chassisDataModels[currentChassisIndex].componentItemModels[componentTransformIndex].Value);
                }

                Inventory.Instance.chassisDataModels[currentChassisIndex].componentItemModels[componentTransformIndex] = null;

                GameManager.Instance.SaveScene();
            }
            return;
        }
        else
        {
            if (Inventory.Instance.chassisDataModels[currentChassisIndex].componentItemModels[componentTransformIndex].HasValue)
            {
                if (Inventory.Instance.chassisDataModels[currentChassisIndex].componentItemModels[componentTransformIndex].Value.itemName == componentList[componentIndex].itemName)
                {
                    return;
                }
                DisableVisualItem(ItemPooler.Instance.visualItemDictionary[Inventory.Instance.chassisDataModels[currentChassisIndex].componentItemModels[componentTransformIndex].Value.itemName]);

                ///Removes whatever component was in the slot prior to a new one being added.
                if (Inventory.Instance.chassisDataModels[currentChassisIndex].isEquipped)
                {
                    Item currentComponentItem = Inventory.Instance.currentEquippedGO.GetComponent<Item>();

                    Inventory.Instance.AddToInventory(currentComponentItem.chassisComponentTransforms[componentTransformIndex].GetComponentTransformItem(), false);
                    currentComponentItem.chassisComponentTransforms[componentTransformIndex].ResetComponentTransform();
                }
                else
                {
                    ItemDataModel tempItemDataModel = new ItemDataModel
                    {
                        itemName = Inventory.Instance.chassisDataModels[currentChassisIndex].componentItemModels[componentTransformIndex].Value.itemName,
                        itemPosition = Inventory.Instance.chassisDataModels[currentChassisIndex].componentItemModels[componentTransformIndex].Value.itemPosition,
                        itemRotation = Inventory.Instance.chassisDataModels[currentChassisIndex].componentItemModels[componentTransformIndex].Value.itemRotation,
                        canPickUp = Inventory.Instance.chassisDataModels[currentChassisIndex].componentItemModels[componentTransformIndex].Value.canPickUp,
                        isObtained = Inventory.Instance.chassisDataModels[currentChassisIndex].componentItemModels[componentTransformIndex].Value.isObtained,
                        isRestored = Inventory.Instance.chassisDataModels[currentChassisIndex].componentItemModels[componentTransformIndex].Value.isRestored,
                        isEquipped = false,
                    };

                    Inventory.Instance.chassisDataModels[currentChassisIndex].componentItemModels[componentTransformIndex] = tempItemDataModel;
                    Inventory.Instance.itemDataModels.Add(Inventory.Instance.chassisDataModels[currentChassisIndex].componentItemModels[componentTransformIndex].Value);
                }

                Inventory.Instance.chassisDataModels[currentChassisIndex].componentItemModels[componentTransformIndex] = null;
            }

            bool isComponentinInventory = false;

            for (int i = 0; i < Inventory.Instance.itemDataModels.Count; i++)
            {
                if (Inventory.Instance.itemDataModels[i].itemName == componentList[componentIndex].itemName)
                {
                    isComponentinInventory = true;
                    break;
                }
            }

            if (!isComponentinInventory)
            {
                int attachedComponentChassisIndex = -1;
                for (int i = 0; i < Inventory.Instance.chassisDataModels.Count; i++)
                {
                    for (int j = 0; j < Inventory.Instance.chassisDataModels[i].componentItemModels.Count; j++)
                    {
                        if (Inventory.Instance.chassisDataModels[i].componentItemModels[j].HasValue)
                        {
                            if (Inventory.Instance.chassisDataModels[i].componentItemModels[j].Value.itemName == componentList[componentIndex].itemName)
                            {
                                attachedComponentChassisIndex = i;
                                break;
                            }
                        }
                    }
                }

                if (attachedComponentChassisIndex == currentChassisIndex)
                {
                    GameObject localComponentGameObjectRef = null;
                    ItemDataModel? localComponentItem = null;
                    for (int i = 0; i < Inventory.Instance.chassisDataModels[currentChassisIndex].componentItemModels.Count; i++)
                    {
                        if (!Inventory.Instance.chassisDataModels[currentChassisIndex].componentItemModels[i].HasValue)
                        {
                            continue;
                        }

                        if (Inventory.Instance.chassisDataModels[currentChassisIndex].componentItemModels[i].Value.itemName == componentList[componentIndex].itemName)
                        {
                            foreach (PrimaryCraftingUIDescriptor currentButton in primaryCraftingList.GetComponentsInChildren<PrimaryCraftingUIDescriptor>())
                            {
                                if (currentButton.titleTextMesh.text == $"Component {i + 1}")
                                {
                                    currentButton.SetButtonInformation($"Component {i + 1}", "None", null);
                                    break;
                                }
                            }

                            DisableVisualItem(ItemPooler.Instance.visualItemDictionary[Inventory.Instance.chassisDataModels[currentChassisIndex].componentItemModels[i].Value.itemName]);

                            localComponentItem = Inventory.Instance.chassisDataModels[attachedComponentChassisIndex].componentItemModels[i].Value;
                            if (Inventory.Instance.chassisDataModels[currentChassisIndex].isEquipped)
                            {
                                Item currentComponentItem = Inventory.Instance.currentEquippedGO.GetComponent<Item>();
                                localComponentGameObjectRef = currentComponentItem.chassisComponentTransforms[i].GetComponentTransformItem().gameObject;
                                currentComponentItem.chassisComponentTransforms[i].ResetComponentTransform();
                            }

                            Inventory.Instance.chassisDataModels[currentChassisIndex].componentItemModels[i] = null;
                            break;
                        }
                        else if(i == componentTransformIndex)
                        {
                            continue;
                        }
                    }
                    if (localComponentGameObjectRef != null)
                    {
                        Item localComponentItemRef = localComponentGameObjectRef.GetComponent<Item>();

                        ///Moves component from one slot on current chassis to currently selected one.
                        if (Inventory.Instance.chassisDataModels[currentChassisIndex].isEquipped)
                        {
                            Item currentComponentItem = Inventory.Instance.currentEquippedGO.GetComponent<Item>();

                            localComponentGameObjectRef.transform.position = currentComponentItem.chassisComponentTransforms[componentTransformIndex].componentTransform.position;
                            localComponentGameObjectRef.transform.rotation = currentComponentItem.chassisComponentTransforms[componentTransformIndex].componentTransform.rotation;
                            currentComponentItem.chassisComponentTransforms[componentTransformIndex].AddNewComponentTransform(localComponentGameObjectRef.GetComponent<Item>());
                        }

                        List<float> localComponentPosition = new List<float>();
                        localComponentPosition.Add(localComponentItemRef.transform.position.x);
                        localComponentPosition.Add(localComponentItemRef.transform.position.y);
                        localComponentPosition.Add(localComponentItemRef.transform.position.z);

                        List<float> localComponentRotation = new List<float>();
                        localComponentRotation.Add(localComponentItemRef.transform.rotation.x);
                        localComponentRotation.Add(localComponentItemRef.transform.rotation.y);
                        localComponentRotation.Add(localComponentItemRef.transform.rotation.z);
                        localComponentRotation.Add(localComponentItemRef.transform.rotation.w);

                        ItemDataModel tempItemDataModel = new ItemDataModel
                        {
                            itemName = localComponentItemRef.itemName,
                            itemPosition = localComponentPosition,
                            itemRotation = localComponentRotation,
                            canPickUp = localComponentItemRef.canPickUp,
                            isObtained = localComponentItemRef.isObtained,
                            isRestored = localComponentItemRef.isRestored,
                            isEquipped = localComponentItemRef.isEquipped,
                        };
                        Inventory.Instance.chassisDataModels[currentChassisIndex].componentItemModels[componentTransformIndex] = tempItemDataModel;
                    }
                    else
                    {
                        Inventory.Instance.chassisDataModels[currentChassisIndex].componentItemModels[componentTransformIndex] = localComponentItem;
                    }

                    GameObject visualChassis = ItemPooler.Instance.visualItemDictionary[Inventory.Instance.chassisDataModels[currentChassisIndex].itemName];
                    GameObject visualComponent = ItemPooler.Instance.visualItemDictionary[componentList[componentIndex].itemName];
                    EnableVisualItem(visualComponent, ItemPooler.Instance.visualItemDictionary[Inventory.Instance.chassisDataModels[currentChassisIndex].itemName].transform, visualChassis.GetComponent<ChassisVisualItem>().GetVisualComponentTransforms()[componentTransformIndex].localPosition, ItemPooler.Instance.visualItemDictionary[Inventory.Instance.chassisDataModels[currentChassisIndex].itemName].GetComponent<ChassisVisualItem>().GetVisualComponentTransforms()[componentTransformIndex].rotation);

                    Sprite componentSprite;
                    ItemPooler.Instance.GetItemSprite(componentList[componentIndex].itemName, out componentSprite);
                    parentButton.GetComponentInParent<PrimaryCraftingUIDescriptor>().SetButtonInformation($"Component { componentTransformIndex + 1 }", componentList[componentIndex].itemName, componentSprite);
                    return;
                }
                else
                {
                    string warning = "The component you are trying to use is attached to a different object!";
                    if (!warningCalled)
                    {
                        warningPanel.SetActive(true);
                        ToggleAllPrimaryCraftingLists(false);
                        warningPanel.GetComponent<WarningMessageUI>().SetWarning(warning, delegate { WarningResult(); }, delegate { WarningResult(); });
                        warningPanel.GetComponent<WarningMessageUI>().AddWarningDelegate(delegate { ChooseNewComponent(componentIndex, componentList, componentTransformIndex, parentButton); warningCalled = false; ToggleAllPrimaryCraftingLists(true); }, delegate { warningCalled = false; ToggleAllPrimaryCraftingLists(true); });
                    }
                    else
                    {
                        GameObject otherComponentGameObjectRef = null;
                        ItemDataModel? otherComponentItem = null;
                        for (int i = 0; i < Inventory.Instance.chassisDataModels.Count; i++)
                        {
                            if (i == attachedComponentChassisIndex)
                            {
                                for(int j = 0; j < Inventory.Instance.chassisDataModels[attachedComponentChassisIndex].componentItemModels.Count; j++)
                                {
                                    if (!Inventory.Instance.chassisDataModels[attachedComponentChassisIndex].componentItemModels[j].HasValue)
                                    {
                                        continue;
                                    }

                                    if(Inventory.Instance.chassisDataModels[attachedComponentChassisIndex].componentItemModels[j].Value.itemName == componentList[componentIndex].itemName)
                                    {
                                        otherComponentItem = Inventory.Instance.chassisDataModels[attachedComponentChassisIndex].componentItemModels[j].Value;
                                        if (Inventory.Instance.chassisDataModels[attachedComponentChassisIndex].isEquipped)
                                        {
                                            Item currentComponentItem = Inventory.Instance.currentEquippedGO.GetComponent<Item>();
                                            Item tempComponent = currentComponentItem.chassisComponentTransforms[j].GetComponentTransformItem();
                                            currentComponentItem.chassisComponentTransforms[j].ResetComponentTransform();

                                            Destroy(tempComponent.gameObject);
                                        }
                                        else if (Inventory.Instance.chassisDataModels[currentChassisIndex].isEquipped)
                                        {
                                            otherComponentGameObjectRef = ItemPooler.Instance.InstantiateItemByName(Inventory.Instance.chassisDataModels[attachedComponentChassisIndex].componentItemModels[j].Value.itemName);
                                        }

                                        Inventory.Instance.chassisDataModels[attachedComponentChassisIndex].componentItemModels[j] = null;
                                        break;
                                    }
                                }
                                break;
                            }
                        }
                        if (otherComponentGameObjectRef != null)
                        {
                            Item otherComponentItemRef = otherComponentGameObjectRef.GetComponent<Item>();

                            ///Removes component from other chassis and places it on this one.
                            if (Inventory.Instance.chassisDataModels[currentChassisIndex].isEquipped)
                            {
                                Item currentComponentItem = Inventory.Instance.currentEquippedGO.GetComponent<Item>();

                                otherComponentGameObjectRef.transform.parent = currentComponentItem.gameObject.transform;
                                otherComponentGameObjectRef.transform.position = currentComponentItem.chassisComponentTransforms[componentTransformIndex].componentTransform.position;
                                otherComponentGameObjectRef.transform.rotation = currentComponentItem.chassisComponentTransforms[componentTransformIndex].componentTransform.rotation;

                                otherComponentGameObjectRef.GetComponent<Rigidbody>().isKinematic = true;
                                otherComponentGameObjectRef.GetComponent<Collider>().enabled = false;
                                otherComponentGameObjectRef.SetActive(true);

                                currentComponentItem.chassisComponentTransforms[componentTransformIndex].AddNewComponentTransform(otherComponentGameObjectRef.GetComponent<Item>());
                            }

                            List<float> otherComponentPosition = new List<float>();
                            otherComponentPosition.Add(otherComponentItemRef.transform.position.x);
                            otherComponentPosition.Add(otherComponentItemRef.transform.position.y);
                            otherComponentPosition.Add(otherComponentItemRef.transform.position.z);

                            List<float> otherComponentRotation = new List<float>();
                            otherComponentRotation.Add(otherComponentItemRef.transform.rotation.x);
                            otherComponentRotation.Add(otherComponentItemRef.transform.rotation.y);
                            otherComponentRotation.Add(otherComponentItemRef.transform.rotation.z);
                            otherComponentRotation.Add(otherComponentItemRef.transform.rotation.w);

                            ItemDataModel tempItemDataModel = new ItemDataModel
                            {
                                itemName = otherComponentItemRef.itemName,
                                itemPosition = otherComponentPosition,
                                itemRotation = otherComponentRotation,
                                canPickUp = otherComponentItemRef.canPickUp,
                                isObtained = otherComponentItemRef.isObtained,
                                isRestored = otherComponentItemRef.isRestored,
                                isEquipped = otherComponentItemRef.isEquipped,
                            };
                            Inventory.Instance.chassisDataModels[currentChassisIndex].componentItemModels[componentTransformIndex] = tempItemDataModel;
                        }
                        else
                        {
                            Inventory.Instance.chassisDataModels[currentChassisIndex].componentItemModels[componentTransformIndex] = otherComponentItem;
                        }

                        if (QuestManager.Instance.IsCurrentQuestActive())
                        {
                            Objective currentObjective = QuestManager.Instance.GetCurrentQuest().GetCurrentObjective();
                            if (currentObjective != null)
                            {
                                currentObjective.CraftItem(otherComponentItem.Value.itemName);
                            }
                        }

                        GameManager.Instance.SaveScene();

                        GameObject visualChassis = ItemPooler.Instance.visualItemDictionary[Inventory.Instance.chassisDataModels[currentChassisIndex].itemName];
                        GameObject visualComponent = ItemPooler.Instance.visualItemDictionary[componentList[componentIndex].itemName];
                        EnableVisualItem(visualComponent, ItemPooler.Instance.visualItemDictionary[Inventory.Instance.chassisDataModels[currentChassisIndex].itemName].transform, visualChassis.GetComponent<ChassisVisualItem>().GetVisualComponentTransforms()[componentTransformIndex].localPosition, ItemPooler.Instance.visualItemDictionary[Inventory.Instance.chassisDataModels[currentChassisIndex].itemName].GetComponent<ChassisVisualItem>().GetVisualComponentTransforms()[componentTransformIndex].rotation);

                        Sprite componentSprite;
                        ItemPooler.Instance.GetItemSprite(componentList[componentIndex].itemName, out componentSprite);
                        parentButton.GetComponentInParent<PrimaryCraftingUIDescriptor>().SetButtonInformation($"Component { componentTransformIndex + 1 }", componentList[componentIndex].itemName, componentSprite); 
                        return;
                    }
                }
            }
            else
            {
                
                int currentItemModelToAddIndex = -1;
                for (int i = 0; i < Inventory.Instance.itemDataModels.Count; i++)
                {
                    if (Inventory.Instance.itemDataModels[i].itemName == componentList[componentIndex].itemName)
                    {
                        currentItemModelToAddIndex = i;
                        break;
                    }
                }
                if (currentItemModelToAddIndex == -1)
                {
                    Debug.LogError("Component not found!");
                    return;
                }

                ItemDataModel tempItemDataModelToAdd = new ItemDataModel
                {
                    itemName = Inventory.Instance.itemDataModels[currentItemModelToAddIndex].itemName,
                    itemPosition = Inventory.Instance.itemDataModels[currentItemModelToAddIndex].itemPosition,
                    itemRotation = Inventory.Instance.itemDataModels[currentItemModelToAddIndex].itemRotation,
                    canPickUp = Inventory.Instance.itemDataModels[currentItemModelToAddIndex].canPickUp,
                    isObtained = Inventory.Instance.itemDataModels[currentItemModelToAddIndex].isObtained,
                    isRestored = Inventory.Instance.itemDataModels[currentItemModelToAddIndex].isRestored,
                    isEquipped = true,
                };

                ///Adds component into current slot, removes from inventory.
                if (Inventory.Instance.chassisDataModels[currentChassisIndex].isEquipped)
                {
                    GameObject currentComponentObj = ItemPooler.Instance.InstantiateItemByName(componentList[componentIndex].itemName);
                    currentComponentObj.GetComponent<Item>().LoadItemModelInfo(tempItemDataModelToAdd);
                    Item currentComponentItem = Inventory.Instance.currentEquippedGO.GetComponent<Item>();

                    currentComponentObj.transform.parent = currentComponentItem.gameObject.transform;
                    currentComponentObj.transform.position = currentComponentItem.chassisComponentTransforms[componentTransformIndex].componentTransform.position;
                    currentComponentObj.transform.rotation = currentComponentItem.chassisComponentTransforms[componentTransformIndex].componentTransform.rotation;

                    currentComponentObj.GetComponent<Rigidbody>().isKinematic = true;
                    currentComponentObj.GetComponent<Collider>().enabled = false;
                    currentComponentObj.SetActive(true);

                    currentComponentItem.chassisComponentTransforms[componentTransformIndex].AddNewComponentTransform(currentComponentObj.GetComponent<Item>());
                }
                Inventory.Instance.chassisDataModels[currentChassisIndex].componentItemModels[componentTransformIndex] = tempItemDataModelToAdd;
                componentList[componentIndex] = tempItemDataModelToAdd;

                if (QuestManager.Instance.IsCurrentQuestActive())
                {
                    Objective currentObjective = QuestManager.Instance.GetCurrentQuest().GetCurrentObjective();
                    if (currentObjective != null)
                    {
                        currentObjective.CraftItem(tempItemDataModelToAdd.itemName);
                    }
                }

                GameManager.Instance.SaveScene();
                    
                GameObject visualChassis = ItemPooler.Instance.visualItemDictionary[Inventory.Instance.chassisDataModels[currentChassisIndex].itemName];
                GameObject visualComponent = ItemPooler.Instance.visualItemDictionary[componentList[componentIndex].itemName];
                EnableVisualItem(visualComponent, ItemPooler.Instance.visualItemDictionary[Inventory.Instance.chassisDataModels[currentChassisIndex].itemName].transform, visualChassis.GetComponent<ChassisVisualItem>().GetVisualComponentTransforms()[componentTransformIndex].localPosition, ItemPooler.Instance.visualItemDictionary[Inventory.Instance.chassisDataModels[currentChassisIndex].itemName].GetComponent<ChassisVisualItem>().GetVisualComponentTransforms()[componentTransformIndex].rotation);

                Sprite componentSprite;
                ItemPooler.Instance.GetItemSprite(componentList[componentIndex].itemName, out componentSprite);
                parentButton.GetComponentInParent<PrimaryCraftingUIDescriptor>().SetButtonInformation($"Component { componentTransformIndex + 1 }", componentList[componentIndex].itemName, componentSprite);

                Inventory.Instance.itemDataModels.RemoveAt(currentItemModelToAddIndex);
            }
        }
    }

    /// <summary>
    /// Applies a new grip to the grip slot.
    /// </summary>
    /// <param name="gripIndex"></param>
    /// <param name="parentButton"></param>
    void ChooseNewGrip(int gripIndex, GameObject parentButton)
    {
        if (gripIndex == -1)
        {
            parentButton.GetComponentInParent<PrimaryCraftingUIDescriptor>().SetButtonInformation("Grip", "None", null);

            if (Inventory.Instance.chassisDataModels[currentChassisIndex].gripItemModel.HasValue)
            {
                DisableVisualItem(ItemPooler.Instance.visualItemDictionary[Inventory.Instance.chassisDataModels[currentChassisIndex].gripItemModel.Value.itemName]);

                ///Removes grip from current slot if there is one.
                if (Inventory.Instance.chassisDataModels[currentChassisIndex].isEquipped)
                {
                    Item currentComponentItem = Inventory.Instance.currentEquippedGO.GetComponent<Item>();
                    Inventory.Instance.playerItemHandler.EquipItem(currentComponentItem);

                    Inventory.Instance.AddToInventory(currentComponentItem.chassisGripTransform.GetGripTransformItem(), false);
                    currentComponentItem.chassisGripTransform.ResetGripTransform();
                }
                else
                {
                    ItemDataModel? tempItemDataModel = new ItemDataModel
                    {
                        itemName = Inventory.Instance.chassisDataModels[currentChassisIndex].gripItemModel.Value.itemName,
                        itemPosition = Inventory.Instance.chassisDataModels[currentChassisIndex].gripItemModel.Value.itemPosition,
                        itemRotation = Inventory.Instance.chassisDataModels[currentChassisIndex].gripItemModel.Value.itemRotation,
                        canPickUp = Inventory.Instance.chassisDataModels[currentChassisIndex].gripItemModel.Value.canPickUp,
                        isObtained = Inventory.Instance.chassisDataModels[currentChassisIndex].gripItemModel.Value.isObtained,
                        isRestored = Inventory.Instance.chassisDataModels[currentChassisIndex].gripItemModel.Value.isRestored,
                        isEquipped = false,
                    };

                    ChassisDataModel tempChassisGripDataModel = new ChassisDataModel
                    {
                        itemName = Inventory.Instance.chassisDataModels[currentChassisIndex].itemName,
                        itemPosition = Inventory.Instance.chassisDataModels[currentChassisIndex].itemPosition,
                        itemRotation = Inventory.Instance.chassisDataModels[currentChassisIndex].itemRotation,
                        canPickUp = Inventory.Instance.chassisDataModels[currentChassisIndex].canPickUp,
                        isObtained = Inventory.Instance.chassisDataModels[currentChassisIndex].isObtained,
                        isRestored = Inventory.Instance.chassisDataModels[currentChassisIndex].isRestored,
                        isEquipped = Inventory.Instance.chassisDataModels[currentChassisIndex].isEquipped,
                        componentItemModels = Inventory.Instance.chassisDataModels[currentChassisIndex].componentItemModels,
                        gripItemModel = tempItemDataModel,
                    };

                    Inventory.Instance.chassisDataModels[currentChassisIndex] = tempChassisGripDataModel;
                    Inventory.Instance.itemDataModels.Add(Inventory.Instance.chassisDataModels[currentChassisIndex].gripItemModel.Value);
                }

                ChassisDataModel tempChassisDataModel = new ChassisDataModel
                {
                    itemName = Inventory.Instance.chassisDataModels[currentChassisIndex].itemName,
                    itemPosition = Inventory.Instance.chassisDataModels[currentChassisIndex].itemPosition,
                    itemRotation = Inventory.Instance.chassisDataModels[currentChassisIndex].itemRotation,
                    canPickUp = Inventory.Instance.chassisDataModels[currentChassisIndex].canPickUp,
                    isObtained = Inventory.Instance.chassisDataModels[currentChassisIndex].isObtained,
                    isRestored = Inventory.Instance.chassisDataModels[currentChassisIndex].isRestored,
                    isEquipped = Inventory.Instance.chassisDataModels[currentChassisIndex].isEquipped,
                    componentItemModels = Inventory.Instance.chassisDataModels[currentChassisIndex].componentItemModels,
                    gripItemModel = null,
                };

                Inventory.Instance.chassisDataModels[currentChassisIndex] = tempChassisDataModel;

                Vector3 chassisLocalHandPos, chassisLocalHandRot;
                ItemPooler.Instance.GetItemHandLocals(Inventory.Instance.chassisDataModels[currentChassisIndex].itemName, out chassisLocalHandPos, out chassisLocalHandRot);
                EnableVisualEquippedItem(ItemPooler.Instance.visualItemDictionary[Inventory.Instance.chassisDataModels[currentChassisIndex].itemName], itemViewer.handAttachment, chassisLocalHandPos, chassisLocalHandRot);
                itemViewer.SwitchPlayerAnimLayer(0);

                GameManager.Instance.SaveScene();
            }
            return;
        }
        else
        {
            if (Inventory.Instance.chassisDataModels[currentChassisIndex].gripItemModel.HasValue)
            {
                if (Inventory.Instance.chassisDataModels[currentChassisIndex].gripItemModel.Value.itemName == gripList[gripIndex].itemName)
                {
                    return;
                }
                DisableVisualItem(ItemPooler.Instance.visualItemDictionary[Inventory.Instance.chassisDataModels[currentChassisIndex].gripItemModel.Value.itemName]);

                ///Removes whatever grip was in the slot prior to a new one being added.
                if (Inventory.Instance.chassisDataModels[currentChassisIndex].isEquipped)
                {
                    Item currentComponentItem = Inventory.Instance.currentEquippedGO.GetComponent<Item>();
                    Inventory.Instance.playerItemHandler.EquipItem(currentComponentItem);

                    Inventory.Instance.AddToInventory(currentComponentItem.chassisGripTransform.GetGripTransformItem(), false);
                    currentComponentItem.chassisGripTransform.ResetGripTransform();
                }
                else
                {
                    ItemDataModel? tempItemDataModel = new ItemDataModel
                    {
                        itemName = Inventory.Instance.chassisDataModels[currentChassisIndex].gripItemModel.Value.itemName,
                        itemPosition = Inventory.Instance.chassisDataModels[currentChassisIndex].gripItemModel.Value.itemPosition,
                        itemRotation = Inventory.Instance.chassisDataModels[currentChassisIndex].gripItemModel.Value.itemRotation,
                        canPickUp = Inventory.Instance.chassisDataModels[currentChassisIndex].gripItemModel.Value.canPickUp,
                        isObtained = Inventory.Instance.chassisDataModels[currentChassisIndex].gripItemModel.Value.isObtained,
                        isRestored = Inventory.Instance.chassisDataModels[currentChassisIndex].gripItemModel.Value.isRestored,
                        isEquipped = false,
                    };

                    ChassisDataModel tempChassisGripDataModel = new ChassisDataModel
                    {
                        itemName = Inventory.Instance.chassisDataModels[currentChassisIndex].itemName,
                        itemPosition = Inventory.Instance.chassisDataModels[currentChassisIndex].itemPosition,
                        itemRotation = Inventory.Instance.chassisDataModels[currentChassisIndex].itemRotation,
                        canPickUp = Inventory.Instance.chassisDataModels[currentChassisIndex].canPickUp,
                        isObtained = Inventory.Instance.chassisDataModels[currentChassisIndex].isObtained,
                        isRestored = Inventory.Instance.chassisDataModels[currentChassisIndex].isRestored,
                        isEquipped = Inventory.Instance.chassisDataModels[currentChassisIndex].isEquipped,
                        componentItemModels = Inventory.Instance.chassisDataModels[currentChassisIndex].componentItemModels,
                        gripItemModel = tempItemDataModel,
                    };

                    Inventory.Instance.chassisDataModels[currentChassisIndex] = tempChassisGripDataModel;
                    Inventory.Instance.itemDataModels.Add(Inventory.Instance.chassisDataModels[currentChassisIndex].gripItemModel.Value);
                }

                ChassisDataModel tempChassisDataModel = new ChassisDataModel
                {
                    itemName = Inventory.Instance.chassisDataModels[currentChassisIndex].itemName,
                    itemPosition = Inventory.Instance.chassisDataModels[currentChassisIndex].itemPosition,
                    itemRotation = Inventory.Instance.chassisDataModels[currentChassisIndex].itemRotation,
                    canPickUp = Inventory.Instance.chassisDataModels[currentChassisIndex].canPickUp,
                    isObtained = Inventory.Instance.chassisDataModels[currentChassisIndex].isObtained,
                    isRestored = Inventory.Instance.chassisDataModels[currentChassisIndex].isRestored,
                    isEquipped = Inventory.Instance.chassisDataModels[currentChassisIndex].isEquipped,
                    componentItemModels = Inventory.Instance.chassisDataModels[currentChassisIndex].componentItemModels,
                    gripItemModel = null,
                };

                Inventory.Instance.chassisDataModels[currentChassisIndex] = tempChassisDataModel;

                Vector3 chassisLocalHandPos, chassisLocalHandRot;
                ItemPooler.Instance.GetItemHandLocals(Inventory.Instance.chassisDataModels[currentChassisIndex].itemName, out chassisLocalHandPos, out chassisLocalHandRot);
                EnableVisualEquippedItem(ItemPooler.Instance.visualItemDictionary[Inventory.Instance.chassisDataModels[currentChassisIndex].itemName], itemViewer.handAttachment, chassisLocalHandPos, chassisLocalHandRot);    
            }

            bool isGripinInventory = false;

            for (int i = 0; i < Inventory.Instance.itemDataModels.Count; i++)
            {
                if (Inventory.Instance.itemDataModels[i].itemName == gripList[gripIndex].itemName)
                {
                    isGripinInventory = true;
                    break;
                }
            }

            if (!isGripinInventory)
            {
                int attachedComponentChassisIndex = -1;
                for (int i = 0; i < Inventory.Instance.chassisDataModels.Count; i++)
                {
                    if (Inventory.Instance.chassisDataModels[i].gripItemModel.HasValue)
                    {
                        if (Inventory.Instance.chassisDataModels[i].gripItemModel.Value.itemName == gripList[gripIndex].itemName)
                        {
                            attachedComponentChassisIndex = i;
                            break;
                        }
                    }
                }

                GameObject otherGripGameObjectRef = null;
                ItemDataModel? otherGripItem = null;
                if (attachedComponentChassisIndex == currentChassisIndex)
                {
                    return;
                }

                string warning = "The grip you are trying to use is attached to a different object!";
                if (!warningCalled)
                {
                    warningPanel.SetActive(true);
                    ToggleAllPrimaryCraftingLists(false);
                    warningPanel.GetComponent<WarningMessageUI>().SetWarning(warning, delegate { WarningResult(); }, delegate { WarningResult(); });
                    warningPanel.GetComponent<WarningMessageUI>().AddWarningDelegate(delegate { ChooseNewGrip(gripIndex, parentButton); warningCalled = false; ToggleAllPrimaryCraftingLists(true); }, delegate { warningCalled = false; ToggleAllPrimaryCraftingLists(true); });
                }
                else
                {
                    if (attachedComponentChassisIndex != currentChassisIndex)
                    {
                        otherGripItem = Inventory.Instance.chassisDataModels[attachedComponentChassisIndex].gripItemModel.Value;
                        if (Inventory.Instance.chassisDataModels[attachedComponentChassisIndex].isEquipped)
                        {
                            Item currentComponentItem = Inventory.Instance.currentEquippedGO.GetComponent<Item>();
                            Inventory.Instance.playerItemHandler.EquipItem(currentComponentItem);
                            
                            Item currentGrip = currentComponentItem.chassisGripTransform.GetGripTransformItem();
                            currentComponentItem.chassisGripTransform.ResetGripTransform();

                            Destroy(currentGrip.gameObject);
                        }
                        else if (Inventory.Instance.chassisDataModels[currentChassisIndex].isEquipped)
                        {
                            otherGripGameObjectRef = ItemPooler.Instance.InstantiateItemByName(Inventory.Instance.chassisDataModels[attachedComponentChassisIndex].gripItemModel.Value.itemName);
                        }

                        ChassisDataModel tempChassisDataModel = new ChassisDataModel
                        {
                            itemName = Inventory.Instance.chassisDataModels[attachedComponentChassisIndex].itemName,
                            itemPosition = Inventory.Instance.chassisDataModels[attachedComponentChassisIndex].itemPosition,
                            itemRotation = Inventory.Instance.chassisDataModels[attachedComponentChassisIndex].itemRotation,
                            canPickUp = Inventory.Instance.chassisDataModels[attachedComponentChassisIndex].canPickUp,
                            isObtained = Inventory.Instance.chassisDataModels[attachedComponentChassisIndex].isObtained,
                            isRestored = Inventory.Instance.chassisDataModels[attachedComponentChassisIndex].isRestored,
                            isEquipped = Inventory.Instance.chassisDataModels[attachedComponentChassisIndex].isEquipped,
                            componentItemModels = Inventory.Instance.chassisDataModels[attachedComponentChassisIndex].componentItemModels,
                            gripItemModel = null,
                        };

                        Inventory.Instance.chassisDataModels[attachedComponentChassisIndex] = tempChassisDataModel;
                    }

                    if (otherGripGameObjectRef != null)
                    {
                        Item currentGripItem = otherGripGameObjectRef.GetComponent<Item>();

                        ///Removes grip from other chassis and places it on this one.
                        if (Inventory.Instance.chassisDataModels[currentChassisIndex].isEquipped)
                        {
                            Item currentComponentItem = Inventory.Instance.currentEquippedGO.GetComponent<Item>();

                            currentComponentItem.gameObject.transform.parent = otherGripGameObjectRef.transform;
                            currentComponentItem.gameObject.transform.position = otherGripGameObjectRef.gameObject.transform.position;
                            currentComponentItem.gameObject.transform.localRotation = Quaternion.Euler(0, currentGripItem.localHandRot.y, 0);
                            currentComponentItem.gameObject.SetActive(true);

                            currentComponentItem.chassisGripTransform.AddNewGripTransform(currentGripItem);
                            Inventory.Instance.playerItemHandler.EquipItem(currentGripItem);
                        }

                        List<float> currentGripPosition = new List<float>();
                        currentGripPosition.Add(currentGripItem.transform.position.x);
                        currentGripPosition.Add(currentGripItem.transform.position.y);
                        currentGripPosition.Add(currentGripItem.transform.position.z);

                        List<float> currentGripRotation = new List<float>();
                        currentGripRotation.Add(currentGripItem.transform.rotation.x);
                        currentGripRotation.Add(currentGripItem.transform.rotation.y);
                        currentGripRotation.Add(currentGripItem.transform.rotation.z);
                        currentGripRotation.Add(currentGripItem.transform.rotation.w);

                        ItemDataModel? tempItemDataModel = new ItemDataModel
                        {
                            itemName = currentGripItem.itemName,
                            itemPosition = currentGripPosition,
                            itemRotation = currentGripRotation,
                            canPickUp = currentGripItem.canPickUp,
                            isObtained = currentGripItem.isObtained,
                            isRestored = currentGripItem.isRestored,
                            isEquipped = currentGripItem.isEquipped,
                        };

                        ChassisDataModel tempChassisGripDataModel = new ChassisDataModel
                        {
                            itemName = Inventory.Instance.chassisDataModels[currentChassisIndex].itemName,
                            itemPosition = Inventory.Instance.chassisDataModels[currentChassisIndex].itemPosition,
                            itemRotation = Inventory.Instance.chassisDataModels[currentChassisIndex].itemRotation,
                            canPickUp = Inventory.Instance.chassisDataModels[currentChassisIndex].canPickUp,
                            isObtained = Inventory.Instance.chassisDataModels[currentChassisIndex].isObtained,
                            isRestored = Inventory.Instance.chassisDataModels[currentChassisIndex].isRestored,
                            isEquipped = Inventory.Instance.chassisDataModels[currentChassisIndex].isEquipped,
                            componentItemModels = Inventory.Instance.chassisDataModels[currentChassisIndex].componentItemModels,
                            gripItemModel = tempItemDataModel,
                        };
                        Inventory.Instance.chassisDataModels[currentChassisIndex] = tempChassisGripDataModel;
                    }
                    else
                    {
                        ChassisDataModel tempChassisGripDataModel = new ChassisDataModel
                        {
                            itemName = Inventory.Instance.chassisDataModels[currentChassisIndex].itemName,
                            itemPosition = Inventory.Instance.chassisDataModels[currentChassisIndex].itemPosition,
                            itemRotation = Inventory.Instance.chassisDataModels[currentChassisIndex].itemRotation,
                            canPickUp = Inventory.Instance.chassisDataModels[currentChassisIndex].canPickUp,
                            isObtained = Inventory.Instance.chassisDataModels[currentChassisIndex].isObtained,
                            isRestored = Inventory.Instance.chassisDataModels[currentChassisIndex].isRestored,
                            isEquipped = Inventory.Instance.chassisDataModels[currentChassisIndex].isEquipped,
                            componentItemModels = Inventory.Instance.chassisDataModels[currentChassisIndex].componentItemModels,
                            gripItemModel = otherGripItem,
                        };

                        Inventory.Instance.chassisDataModels[currentChassisIndex] = tempChassisGripDataModel;
                    }

                    if (QuestManager.Instance.IsCurrentQuestActive())
                    {
                        Objective currentObjective = QuestManager.Instance.GetCurrentQuest().GetCurrentObjective();
                        if (currentObjective != null)
                        {
                            currentObjective.CraftItem(otherGripItem.Value.itemName);
                        }
                    }

                    GameManager.Instance.SaveScene();

                    ///Visual
                    GameObject visualGrip = ItemPooler.Instance.visualItemDictionary[gripList[gripIndex].itemName];
                        GameObject visualChassis = ItemPooler.Instance.visualItemDictionary[Inventory.Instance.chassisDataModels[currentChassisIndex].itemName];
                        Vector3 localGripHandPos, localGripHandRot;
                        ItemPooler.Instance.GetItemHandLocals(gripList[gripIndex].itemName, out localGripHandPos, out localGripHandRot);
                        EnableVisualEquippedItem(visualGrip, itemViewer.handAttachment, localGripHandPos, localGripHandRot);
                        EnableVisualItem(visualChassis, visualGrip.transform, visualChassis.GetComponent<ChassisVisualItem>().GetVisualGripTransform().localPosition, Quaternion.Euler(0, localGripHandRot.y, 0), true);

                        GripItem.GripType gripType;
                        ItemPooler.Instance.GetGripAnimEnum(gripList[gripIndex].itemName, out gripType);
                        itemViewer.SwitchPlayerAnimLayer((int)gripType);

                        Sprite gripSprite;
                        ItemPooler.Instance.GetItemSprite(gripList[gripIndex].itemName, out gripSprite);
                        parentButton.GetComponentInParent<PrimaryCraftingUIDescriptor>().SetButtonInformation("Grip", gripList[gripIndex].itemName, gripSprite);
                    return;
                }
            }
            else
            {
                int currentItemModelToAddIndex = -1;
                for (int i = 0; i < Inventory.Instance.itemDataModels.Count; i++)
                {
                    if (Inventory.Instance.itemDataModels[i].itemName == gripList[gripIndex].itemName)
                    {
                        currentItemModelToAddIndex = i;
                        break;
                    }
                }
                if (currentItemModelToAddIndex == -1)
                {
                    Debug.LogError("Component not found!");
                    return;
                }

                ItemDataModel tempItemDataModelToAdd = new ItemDataModel
                {
                    itemName = Inventory.Instance.itemDataModels[currentItemModelToAddIndex].itemName,
                    itemPosition = Inventory.Instance.itemDataModels[currentItemModelToAddIndex].itemPosition,
                    itemRotation = Inventory.Instance.itemDataModels[currentItemModelToAddIndex].itemRotation,
                    canPickUp = Inventory.Instance.itemDataModels[currentItemModelToAddIndex].canPickUp,
                    isObtained = Inventory.Instance.itemDataModels[currentItemModelToAddIndex].isObtained,
                    isRestored = Inventory.Instance.itemDataModels[currentItemModelToAddIndex].isRestored,
                    isEquipped = true,
                };

                ChassisDataModel tempChassisGripDataModelToAdd = new ChassisDataModel
                {
                    itemName = Inventory.Instance.chassisDataModels[currentChassisIndex].itemName,
                    itemPosition = Inventory.Instance.chassisDataModels[currentChassisIndex].itemPosition,
                    itemRotation = Inventory.Instance.chassisDataModels[currentChassisIndex].itemRotation,
                    canPickUp = Inventory.Instance.chassisDataModels[currentChassisIndex].canPickUp,
                    isObtained = Inventory.Instance.chassisDataModels[currentChassisIndex].isObtained,
                    isRestored = Inventory.Instance.chassisDataModels[currentChassisIndex].isRestored,
                    isEquipped = Inventory.Instance.chassisDataModels[currentChassisIndex].isEquipped,
                    componentItemModels = Inventory.Instance.chassisDataModels[currentChassisIndex].componentItemModels,
                    gripItemModel = tempItemDataModelToAdd,
                };

                ///Adds grip into current slot, removes from inventory.
                if (Inventory.Instance.chassisDataModels[currentChassisIndex].isEquipped)
                {
                    GameObject currentGripObj = ItemPooler.Instance.InstantiateItemByName(gripList[gripIndex].itemName);
                    Item currentGripItem = currentGripObj.GetComponent<Item>();
                    currentGripItem.LoadItemModelInfo(tempItemDataModelToAdd);
                    Item currentComponentItem = Inventory.Instance.currentEquippedGO.GetComponent<Item>();
                    
                    currentComponentItem.gameObject.transform.parent = currentGripObj.transform;
                    currentComponentItem.gameObject.transform.position = currentGripObj.gameObject.transform.position;
                    currentComponentItem.gameObject.transform.localRotation = Quaternion.Euler(0, currentGripItem.localHandRot.y, 0);

                    currentGripObj.GetComponent<Rigidbody>().isKinematic = true;
                    currentGripObj.GetComponent<Collider>().enabled = false;
                    currentGripObj.gameObject.SetActive(true);

                    currentComponentItem.chassisGripTransform.AddNewGripTransform(currentGripItem);
                    Inventory.Instance.playerItemHandler.EquipItem(currentGripItem);
                }
                Inventory.Instance.chassisDataModels[currentChassisIndex] = tempChassisGripDataModelToAdd;

                if (QuestManager.Instance.IsCurrentQuestActive())
                {
                    Objective currentObjective = QuestManager.Instance.GetCurrentQuest().GetCurrentObjective();
                    if (currentObjective != null)
                    {
                        currentObjective.CraftItem(tempItemDataModelToAdd.itemName);
                    }
                }

                GameManager.Instance.SaveScene();

                ///Visual
                GameObject visualGrip = ItemPooler.Instance.visualItemDictionary[gripList[gripIndex].itemName];
                GameObject visualChassis = ItemPooler.Instance.visualItemDictionary[Inventory.Instance.chassisDataModels[currentChassisIndex].itemName];
                Vector3 localGripHandPos, localGripHandRot;
                ItemPooler.Instance.GetItemHandLocals(gripList[gripIndex].itemName, out localGripHandPos, out localGripHandRot);
                EnableVisualEquippedItem(visualGrip, itemViewer.handAttachment, localGripHandPos, localGripHandRot);
                EnableVisualItem(visualChassis, visualGrip.transform, visualChassis.GetComponent<ChassisVisualItem>().GetVisualGripTransform().localPosition, Quaternion.Euler(0, localGripHandRot.y, 0), true);

                GripItem.GripType gripType;
                ItemPooler.Instance.GetGripAnimEnum(gripList[gripIndex].itemName, out gripType);
                itemViewer.SwitchPlayerAnimLayer((int)gripType);

                Sprite gripSprite;
                ItemPooler.Instance.GetItemSprite(gripList[gripIndex].itemName, out gripSprite);
                parentButton.GetComponentInParent<PrimaryCraftingUIDescriptor>().SetButtonInformation("Grip", gripList[gripIndex].itemName, gripSprite);

                Inventory.Instance.itemDataModels.RemoveAt(currentItemModelToAddIndex);
            }
        }

    }

    void SetEventTrigger(ItemDataModel currentItemModel, out EventTrigger.Entry eventTypeEnter, out EventTrigger.Entry eventTypeExit)
    {
        Item.TypeTag itemType;
        ItemPooler.Instance.GetItemType(currentItemModel.itemName, out itemType);
        string itemTypeStr = (itemType).ToString();
        itemTypeStr = char.ToUpper(itemTypeStr[0]) + itemTypeStr.Substring(1);
        string itemTitle = $"{currentItemModel.itemName} ({itemTypeStr})";

        eventTypeEnter = new EventTrigger.Entry();
        eventTypeEnter.eventID = EventTriggerType.PointerEnter;
        eventTypeExit = new EventTrigger.Entry();
        eventTypeExit.eventID = EventTriggerType.PointerExit;

        string description;
        ItemPooler.Instance.GetItemDescription(currentItemModel.itemName, out description);

        eventTypeEnter.callback.AddListener((PointerEventData) => { tooltip.GetComponent<Tooltip>().EnableTooltip(itemTitle, description); });
        eventTypeExit.callback.AddListener((PointerEventData) => { tooltip.GetComponent<Tooltip>().DisableTooltip(); });
    }

    void SetEventTrigger(ChassisDataModel currentChassisModel, out EventTrigger.Entry eventTypeEnter, out EventTrigger.Entry eventTypeExit)
    {
        Item.TypeTag itemType;
        ItemPooler.Instance.GetItemType(currentChassisModel.itemName, out itemType);
        string itemTypeStr = (itemType).ToString();
        itemTypeStr = char.ToUpper(itemTypeStr[0]) + itemTypeStr.Substring(1);
        string itemTitle = $"{currentChassisModel.itemName} ({itemTypeStr})";

        eventTypeEnter = new EventTrigger.Entry();
        eventTypeEnter.eventID = EventTriggerType.PointerEnter;
        eventTypeExit = new EventTrigger.Entry();
        eventTypeExit.eventID = EventTriggerType.PointerExit;

        string description;
        ItemPooler.Instance.GetItemDescription(currentChassisModel.itemName, out description);

        eventTypeEnter.callback.AddListener((PointerEventData) => { tooltip.GetComponent<Tooltip>().EnableTooltip(itemTitle, description); });
        eventTypeExit.callback.AddListener((PointerEventData) => { tooltip.GetComponent<Tooltip>().DisableTooltip(); });
    }

    GameObject SpawnPrimaryButton(string title, string itemTitle, Sprite itemIcon,ref float heightSpacing)
    {
        GameObject obj = ObjectPooler.GetPooler(primaryButtonUIKey).GetPooledObject();
        obj.GetComponent<PrimaryCraftingUIDescriptor>().ResetSecondaryCraftingRect();
        obj.GetComponent<PrimaryCraftingUIDescriptor>().SetButtonInformation(title, itemTitle, itemIcon);
        obj.transform.SetParent(primaryCraftingList.transform, false);
        obj.GetComponent<PrimaryCraftingUIDescriptor>().MoveSecondaryCraftingRect(0, heightSpacing);
        heightSpacing += craftingModButtonPrefab.GetComponent<RectTransform>().rect.height;
        obj.SetActive(true);
        return obj.GetComponent<PrimaryCraftingUIDescriptor>().internalCraftingList;
    }

    void SpawnMultiComponentSecondaryButtons(List<ItemDataModel> componentList, Transform secondaryButtonParent, int currentComponentIndexOnChassis)
    {
        GameObject noneObj = ObjectPooler.GetPooler(secondaryButtonUIKey).GetPooledObject();
        noneObj.GetComponent<ItemUIDescriptor>().ApplyDescriptors(null, "None");
        noneObj.GetComponentInChildren<Button>().onClick.AddListener(delegate { secondaryButtonParent.GetComponentInParent<PrimaryCraftingUIDescriptor>().DisableListItems(); });
        noneObj.GetComponentInChildren<Button>().onClick.AddListener(delegate { ChooseNewComponent(-1, null, currentComponentIndexOnChassis, secondaryButtonParent.GetComponentInParent<PrimaryCraftingUIDescriptor>().gameObject); });
        noneObj.transform.SetParent(secondaryButtonParent, false);
        noneObj.SetActive(true);

        for (int i = 0; i < componentList.Count; i++)
        {
            Item.TypeTag typeTag;
            string currentComponentName = componentList[i].itemName;
            ItemPooler.Instance.GetItemType(currentComponentName, out typeTag);

            switch (typeTag)
            {
                case Item.TypeTag.effector:
                    int effectorIndex = i;
                    GameObject effectorObj = ObjectPooler.GetPooler(secondaryButtonUIKey).GetPooledObject();
                    Sprite effectorSprite;
                    ItemPooler.Instance.GetItemSprite(currentComponentName, out effectorSprite);
                    effectorObj.GetComponent<ItemUIDescriptor>().ApplyDescriptors(effectorSprite, currentComponentName);
                    effectorObj.GetComponentInChildren<Button>().onClick.AddListener(delegate { ChooseNewComponent(effectorIndex, componentList, currentComponentIndexOnChassis, secondaryButtonParent.GetComponentInParent<PrimaryCraftingUIDescriptor>().gameObject); });
                    effectorObj.GetComponentInChildren<Button>().onClick.AddListener(delegate { secondaryButtonParent.GetComponentInParent<PrimaryCraftingUIDescriptor>().DisableListItems(); });
                    effectorObj.GetComponentInChildren<Button>().onClick.AddListener(delegate { tooltip.GetComponent<Tooltip>().DisableTooltip(); });

                    EventTrigger.Entry effectorEventTypeEnter;
                    EventTrigger.Entry effectorEventTypeExit;
                    SetEventTrigger(componentList[effectorIndex], out effectorEventTypeEnter, out effectorEventTypeExit);
                    effectorObj.GetComponentInChildren<EventTrigger>().triggers.Add(effectorEventTypeEnter);
                    effectorObj.GetComponentInChildren<EventTrigger>().triggers.Add(effectorEventTypeExit);

                    effectorObj.transform.SetParent(secondaryButtonParent, false);
                    effectorObj.SetActive(true);

                    break;
                case Item.TypeTag.modifier:
                    int modifierIndex = i;
                    GameObject modifierObj = ObjectPooler.GetPooler(secondaryButtonUIKey).GetPooledObject();
                    Sprite modifierSprite;
                    ItemPooler.Instance.GetItemSprite(currentComponentName, out modifierSprite);
                    modifierObj.GetComponent<ItemUIDescriptor>().ApplyDescriptors(modifierSprite, currentComponentName);
                    modifierObj.GetComponentInChildren<Button>().onClick.AddListener(delegate { ChooseNewComponent(modifierIndex, componentList, currentComponentIndexOnChassis, secondaryButtonParent.GetComponentInParent<PrimaryCraftingUIDescriptor>().gameObject); });
                    modifierObj.GetComponentInChildren<Button>().onClick.AddListener(delegate { secondaryButtonParent.GetComponentInParent<PrimaryCraftingUIDescriptor>().DisableListItems(); });
                    modifierObj.GetComponentInChildren<Button>().onClick.AddListener(delegate { tooltip.GetComponent<Tooltip>().DisableTooltip(); });

                    EventTrigger.Entry modifierEventTypeEnter;
                    EventTrigger.Entry modifierEventTypeExit;
                    SetEventTrigger(componentList[modifierIndex], out modifierEventTypeEnter, out modifierEventTypeExit);
                    modifierObj.GetComponentInChildren<EventTrigger>().triggers.Add(modifierEventTypeEnter);
                    modifierObj.GetComponentInChildren<EventTrigger>().triggers.Add(modifierEventTypeExit);

                    modifierObj.transform.SetParent(secondaryButtonParent, false);
                    modifierObj.SetActive(true);

                    break;
                case Item.TypeTag.ammo:
                    int ammoIndex = i;
                    GameObject ammoObj = ObjectPooler.GetPooler(secondaryButtonUIKey).GetPooledObject();
                    Sprite ammoSprite;
                    ItemPooler.Instance.GetItemSprite(currentComponentName, out ammoSprite);
                    ammoObj.GetComponent<ItemUIDescriptor>().ApplyDescriptors(ammoSprite, currentComponentName);
                    ammoObj.GetComponentInChildren<Button>().onClick.AddListener(delegate { ChooseNewComponent(ammoIndex, componentList, currentComponentIndexOnChassis, secondaryButtonParent.GetComponentInParent<PrimaryCraftingUIDescriptor>().gameObject); });
                    ammoObj.GetComponentInChildren<Button>().onClick.AddListener(delegate { secondaryButtonParent.GetComponentInParent<PrimaryCraftingUIDescriptor>().DisableListItems(); });
                    ammoObj.GetComponentInChildren<Button>().onClick.AddListener(delegate { tooltip.GetComponent<Tooltip>().DisableTooltip(); });

                    EventTrigger.Entry ammoEventTypeEnter;
                    EventTrigger.Entry ammoEventTypeExit;
                    SetEventTrigger(componentList[ammoIndex], out ammoEventTypeEnter, out ammoEventTypeExit);
                    ammoObj.GetComponentInChildren<EventTrigger>().triggers.Add(ammoEventTypeEnter);
                    ammoObj.GetComponentInChildren<EventTrigger>().triggers.Add(ammoEventTypeExit);

                    ammoObj.transform.SetParent(secondaryButtonParent, false);
                    ammoObj.SetActive(true);

                    break;
            }
        }

        secondaryButtonParent.GetComponentInParent<PrimaryCraftingUIDescriptor>().UpdateScrollParameters(componentList.Count + 1);
    }

    void SpawnSecondaryButtons(Item.TypeTag typeTag, Transform secondaryButtonParent)
    {
        GameObject noneObj = ObjectPooler.GetPooler(secondaryButtonUIKey).GetPooledObject();
        noneObj.GetComponent<ItemUIDescriptor>().ApplyDescriptors(null, "None");
        noneObj.GetComponentInChildren<Button>().onClick.AddListener(delegate { secondaryButtonParent.GetComponentInParent<PrimaryCraftingUIDescriptor>().DisableListItems(); });
        noneObj.transform.SetParent(secondaryButtonParent, false);
        noneObj.SetActive(true);

        switch (typeTag)
        {
            case Item.TypeTag.chassis:
                noneObj.GetComponentInChildren<Button>().onClick.AddListener(delegate { ChooseNewChassis(-1); });

                for (int i = 0; i < Inventory.Instance.chassisDataModels.Count; i++)
                {
                    if (!Inventory.Instance.chassisDataModels[i].isRestored)
                    {
                        continue;
                    }

                    int chassisIndex = i;
                    GameObject chassisObj = ObjectPooler.GetPooler(secondaryButtonUIKey).GetPooledObject();
                    Sprite chassisSprite;
                    ItemPooler.Instance.GetItemSprite(Inventory.Instance.chassisDataModels[chassisIndex].itemName, out chassisSprite);
                    chassisObj.GetComponent<ItemUIDescriptor>().ApplyDescriptors(chassisSprite, Inventory.Instance.chassisDataModels[chassisIndex].itemName);
                    chassisObj.GetComponentInChildren<Button>().onClick.AddListener(delegate { ChooseNewChassis(chassisIndex); });
                    chassisObj.GetComponentInChildren<Button>().onClick.AddListener(delegate { secondaryButtonParent.GetComponentInParent<PrimaryCraftingUIDescriptor>().DisableListItems(); });
                    chassisObj.GetComponentInChildren<Button>().onClick.AddListener(delegate { tooltip.GetComponent<Tooltip>().DisableTooltip(); });

                    EventTrigger.Entry chassisEventTypeEnter;
                    EventTrigger.Entry chassisEventTypeExit;
                    SetEventTrigger(Inventory.Instance.chassisDataModels[chassisIndex], out chassisEventTypeEnter, out chassisEventTypeExit);
                    chassisObj.GetComponentInChildren<EventTrigger>().triggers.Add(chassisEventTypeEnter);
                    chassisObj.GetComponentInChildren<EventTrigger>().triggers.Add(chassisEventTypeExit);

                    chassisObj.transform.SetParent(secondaryButtonParent, false);
                    chassisObj.SetActive(true);
                }

                secondaryButtonParent.GetComponentInParent<PrimaryCraftingUIDescriptor>().UpdateScrollParameters(Inventory.Instance.chassisDataModels.Count + 1);
                break;
            case Item.TypeTag.grip:
                noneObj.GetComponentInChildren<Button>().onClick.AddListener(delegate { ChooseNewGrip(-1, secondaryButtonParent.GetComponentInParent<PrimaryCraftingUIDescriptor>().gameObject); });

                for (int i = 0; i < gripList.Count; i++)
                {
                    int gripIndex = i;
                    GameObject gripObj = ObjectPooler.GetPooler(secondaryButtonUIKey).GetPooledObject();
                    Sprite gripSprite;
                    ItemPooler.Instance.GetItemSprite(gripList[gripIndex].itemName, out gripSprite);
                    gripObj.GetComponent<ItemUIDescriptor>().ApplyDescriptors(gripSprite, gripList[gripIndex].itemName);
                    gripObj.GetComponentInChildren<Button>().onClick.AddListener(delegate { ChooseNewGrip(gripIndex, secondaryButtonParent.GetComponentInParent<PrimaryCraftingUIDescriptor>().gameObject); });
                    gripObj.GetComponentInChildren<Button>().onClick.AddListener(delegate { secondaryButtonParent.GetComponentInParent<PrimaryCraftingUIDescriptor>().DisableListItems(); });
                    gripObj.GetComponentInChildren<Button>().onClick.AddListener(delegate { tooltip.GetComponent<Tooltip>().DisableTooltip(); });

                    EventTrigger.Entry gripEventTypeEnter;
                    EventTrigger.Entry gripEventTypeExit;
                    SetEventTrigger(gripList[gripIndex], out gripEventTypeEnter, out gripEventTypeExit);
                    gripObj.GetComponentInChildren<EventTrigger>().triggers.Add(gripEventTypeEnter);
                    gripObj.GetComponentInChildren<EventTrigger>().triggers.Add(gripEventTypeExit);

                    gripObj.transform.SetParent(secondaryButtonParent, false);
                    gripObj.SetActive(true);
                }
                secondaryButtonParent.GetComponentInParent<PrimaryCraftingUIDescriptor>().UpdateScrollParameters(gripList.Count + 1);
                break;
        }
    }

    void RotateItemViewer()
    {
        if (Input.GetKey(KeyCode.RightArrow))
        {
            itemViewerPlayerModel.transform.Rotate(new Vector3(0, -playerModelAngularSpeed * Time.fixedDeltaTime, 0));
        }
        else if(Input.GetKey(KeyCode.LeftArrow))
        {
            itemViewerPlayerModel.transform.Rotate(new Vector3(0, playerModelAngularSpeed * Time.fixedDeltaTime, 0));
        }
        else if (Input.GetKeyDown(KeyCode.DownArrow))
        {
            itemViewerPlayerModel.transform.rotation = originalPlayerModelRotation;
        }
    }

    private void Update()
    {
        if (isActive)
        {
            RotateItemViewer();
        }

        if (craftingPanel.activeSelf && !isActive)
        {
            isActive = true;
            OnEnableCraftingPanel();
        }
        else if (!craftingPanel.activeSelf && isActive)
        {
            isActive = false;
            OnDisableCraftingPanel();
        }
    }
}
