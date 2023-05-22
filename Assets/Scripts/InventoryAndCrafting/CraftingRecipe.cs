using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public struct ItemAmount
{
    public Item Item;
    [Range(1, 999)]
    public int amount;
}

[CreateAssetMenu]
public class CraftingRecipe : ScriptableObject
{
    public List<ItemAmount> Materials;
    public List<ItemAmount> Results;

    public bool CanCraft(IItemContainer itemContainer)
    {
        foreach(ItemAmount itemAmount in Materials)
        {
            if(itemContainer.ItemCount(itemAmount.Item) < itemAmount.amount)
            {
                return false;
            }
        }

        return true;
    }

    public void Craft(IItemContainer itemContainer)
    {
        if(CanCraft(itemContainer))
        {
            foreach(ItemAmount itemAmount in Materials)
            {
                for(int i = 0; i < itemAmount.amount; i++)
                {
                    itemContainer.RemoveItem(itemAmount.Item);
                }
            }

            foreach (ItemAmount itemAmount in Materials)
            {
                for (int i = 0; i < itemAmount.amount; i++)
                {
                    itemContainer.AddItem(itemAmount.Item);
                }
            }
        }
    }
}
