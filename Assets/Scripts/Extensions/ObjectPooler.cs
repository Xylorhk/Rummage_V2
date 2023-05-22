using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjectPooler : MonoBehaviour
{
    public static ObjectPooler current;
    public int pooledAmount = 20;
    public bool willGrow = true;

    private int count = 1;

    List<GameObject> pooledObjects;

    public enum Key
    {
        InventoryItemUIButtons,
        PrimaryCraftingUIButtons,
        SecondaryCraftingUIButtons,
        GrenadeAmmo,
        RockAmmo,
        ExplosionParticle,
        InteractionParticle,
        Scrap
    }

    public Key key;
    public GameObject pooledObject;

    public static Dictionary<Key, ObjectPooler> dict = new Dictionary<Key, ObjectPooler>();

    void Awake()
    {
        dict[key] = this;

        pooledObjects = new List<GameObject>();
        for (int i = 0; i < pooledAmount; i++)
        {
            GameObject obj = (GameObject)Instantiate(pooledObject);
            if (obj.GetComponentInChildren<RectTransform>() == null)
            {
                obj.transform.parent = this.transform;
            }
            else
            {
                obj.transform.SetParent(this.transform, false);
            }
            obj.name = pooledObject.name + " " + (count);
            count++;

            obj.SetActive(false);
            pooledObjects.Add(obj);
        }
    }

    public GameObject GetPooledObject()
    {
        for (int i = 0; i < pooledObjects.Count; i++)
        {
            if (!pooledObjects[i].activeSelf)
            {
                return pooledObjects[i];
            }
        }

        if (willGrow)
        {
            Debug.Log("Grown");
            GameObject obj = (GameObject)Instantiate(pooledObject);
            if (obj.GetComponentInChildren<RectTransform>() == null)
            {
                obj.transform.parent = this.transform;
            }
            else
            {
                obj.transform.SetParent(this.transform, false);
            }
            obj.name = pooledObject.name + " " + (count);
            count++;

            obj.SetActive(false);
            pooledObjects.Add(obj);
            return obj;
        }

        Debug.LogError("No more pooled objects available");
        return null;
    }

    public static ObjectPooler GetPooler(Key key)
    {
        return dict[key];
    }
}