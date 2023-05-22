using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ItemPooler : SingletonMonoBehaviour<ItemPooler>
{
    [SerializeField]
    private List<GameObject> gameItems = new List<GameObject>();

    public Dictionary<string, GameObject> itemDictionary = new Dictionary<string, GameObject>();
    public Dictionary<string, GameObject> visualItemDictionary = new Dictionary<string, GameObject>();
    public GameObject visualItemParent;

    new void Awake()
    {
        base.Awake();

        for (int i = 0; i < gameItems.Count; i++)
        {
            Item currentGameItem = gameItems[i].GetComponent<Item>();

            if (currentGameItem != null)
            {
                if (!itemDictionary.ContainsKey(currentGameItem.itemName))
                {
                    itemDictionary.Add(currentGameItem.itemName, gameItems[i]);
                    AddItemToVisualDictionary(currentGameItem);
                }
            }
        }
    }

    void AddItemToVisualDictionary(Item itemToAdd)
    {
        if (visualItemDictionary.ContainsKey(itemToAdd.itemName))
        {
            return;
        }

        GameObject tempGameObj = Instantiate(itemDictionary[itemToAdd.itemName]);
        string tempName = tempGameObj.name;
        if (tempName.Contains("(Clone)"))
        {
            tempName = tempName.Substring(0, Mathf.Abs(tempName.IndexOf("(Clone)")));
        }
        tempGameObj.name = $"{tempName}_Unwrapped";

        if (itemToAdd.itemType == Item.TypeTag.chassis)
        {
            tempGameObj.AddComponent<ChassisVisualItem>();
            tempGameObj.GetComponent<ChassisVisualItem>().AddVisualTransforms(tempGameObj.GetComponent<Item>().chassisComponentTransforms, tempGameObj.GetComponent<Item>().chassisGripTransform);
        }

        foreach (var component in tempGameObj.GetComponents<Component>())
        {
            if (component == tempGameObj.GetComponent<Transform>() || component == tempGameObj.GetComponent<MeshFilter>() ||
                component == tempGameObj.GetComponent<MeshRenderer>() || component == tempGameObj.GetComponent<VisualItem>())
            {
                continue;
            }

            Destroy(component);
        }

        tempGameObj.layer = LayerMask.NameToLayer("ItemRenderer");

        foreach (Transform child in tempGameObj.GetComponentsInChildren<Transform>())
        {
            MeshRenderer tempMR = null;
            tempMR = child.GetComponent<MeshRenderer>();

            if (tempMR == null)
            {
                continue;
            }
            else
            {
                child.gameObject.layer = LayerMask.NameToLayer("ItemRenderer");
            }
        }

        visualItemDictionary.Add(itemToAdd.itemName, tempGameObj);
        tempGameObj.transform.SetParent(visualItemParent.transform);
        tempGameObj.SetActive(false);
    }

    public void ResetVisualItems()
    {
        foreach(KeyValuePair<string, GameObject> currentVisualPair in visualItemDictionary)
        {
            currentVisualPair.Value.transform.SetParent(visualItemParent.transform);
            currentVisualPair.Value.SetActive(false);
        }
    }

    public void GetItemInformation(string itemName, out int restorationAmount, out Item.TypeTag itemType, out string description, out Sprite itemIcon)
    {
        Item prefabItem = itemDictionary[itemName].GetComponent<Item>();

        restorationAmount = prefabItem.restorationScrapAmount;
        itemType = prefabItem.itemType;
        description = prefabItem.description;
        itemIcon = prefabItem.inventorySprite;
    }

    public void GetItemRestorationAmount(string itemName, out int restorationAmount)
    {
        Item prefabItem = itemDictionary[itemName].GetComponent<Item>();

        restorationAmount = prefabItem.restorationScrapAmount;
    }

    public void GetItemType(string itemName, out Item.TypeTag itemType)
    {
        Item prefabItem = itemDictionary[itemName].GetComponent<Item>();

        itemType = prefabItem.itemType;
    }

    public void GetItemDescription(string itemName, out string description)
    {
        Item prefabItem = itemDictionary[itemName].GetComponent<Item>();

        description = prefabItem.description;
    }

    public void GetItemSprite(string itemName, out Sprite itemIcon)
    {
        Item prefabItem = itemDictionary[itemName].GetComponent<Item>();

        itemIcon = prefabItem.inventorySprite;
    }

    public void GetItemHandLocals(string itemName, out Vector3 localPos, out Vector3 localRot)
    {
        Item prefabItem = itemDictionary[itemName].GetComponent<Item>();

        localPos = prefabItem.localHandPos;
        localRot = prefabItem.localHandRot;
    }

    public void GetGripAnimEnum(string itemName, out GripItem.GripType gripType)
    {
        GripItem prefabGripItem = itemDictionary[itemName].GetComponent<GripItem>();

        if (prefabGripItem == null)
        {
            Debug.LogError("Object passed is not grip.");
            gripType = GripItem.GripType.None;
            return;
        }

        gripType = prefabGripItem.gripType;
    }

    public GameObject InstantiateItemByName(string itemName)
    {
        if (!itemDictionary.ContainsKey(itemName))
        {
            Debug.LogError($"Item Dictionary does not contain current item: {itemName}");
            return null;
        }

        GameObject tempGameObj = Instantiate(itemDictionary[itemName]);
        tempGameObj.GetComponent<SaveableEntity>().GenerateCloneIdSceneSpecific(itemDictionary[itemName].GetComponent<SaveableEntity>().Id, itemDictionary[itemName].gameObject.name);

        return tempGameObj;
    }
}