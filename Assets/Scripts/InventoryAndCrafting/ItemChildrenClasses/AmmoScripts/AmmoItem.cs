using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class AmmoItem : Item
{
    [Header("Ammo Specific Variables")]
    public ObjectPooler.Key ammoPrefabKey;
    public Transform spawnTransform;
    
    [Header("Juice Variables")]
    public List<GameObject> juiceGameObjects = new List<GameObject>();
    public float ammoActiveTimer = 0;
    private int lastIndex = -1;

    private Item currentChassis = null;

    void Awake()
    {
        lastIndex = juiceGameObjects.Count - 1;
    }

    public override void Activate()
    {
        if (currentChassis == null && isEquipped)
        {
            FindCurrentChassis();
            return;
        }

        if (Player.Instance.playerInput.actions["Fire"].WasPressedThisFrame())
        {
            bool gunEffectorCheck = false;
            bool catapultEffectorCheck = false;
            GunEffector currentGun = null;
            CatapultEffector currentCat = null;

            for (int i = 0; i < currentChassis.chassisComponentTransforms.Count; i++)
            {
                if (!currentChassis.chassisComponentTransforms[i].IsComponentTransformOccupied())
                {
                    continue;
                }

                GunEffector tempGun = null;
                CatapultEffector tempCat = null;

                tempGun = currentChassis.chassisComponentTransforms[i].GetComponentTransformItem().GetComponent<GunEffector>();
                tempCat = currentChassis.chassisComponentTransforms[i].GetComponentTransformItem().GetComponent<CatapultEffector>();

                if (tempGun != null)
                {
                    gunEffectorCheck = true;
                    currentGun = tempGun;
                    continue;
                }

                if (tempCat != null)
                {
                    catapultEffectorCheck = true;
                    currentCat = tempCat;
                    continue;
                }
            }

            if (!gunEffectorCheck && !catapultEffectorCheck)
            {
                GameObject currentAmmo = ObjectPooler.GetPooler(ammoPrefabKey).GetPooledObject();
                if (currentAmmo == null)
                {
                    return;
                }

                currentAmmo.transform.position = spawnTransform.position;
                currentAmmo.transform.rotation = UnityEngine.Random.rotation;
                currentAmmo.SetActive(true);

                if (QuestManager.Instance.IsCurrentQuestActive())
                {
                    Objective currentObjective = QuestManager.Instance.GetCurrentQuest().GetCurrentObjective();
                    if (currentObjective != null)
                    {
                        currentObjective.ActivateItem(itemName);
                    }
                }

                DisableUnwrappedJuice();
            }
        }
    }

    public void DisableUnwrappedJuice()
    {
        juiceGameObjects[lastIndex].SetActive(false);
        lastIndex--;
        if (lastIndex < 0)
        {
            lastIndex = 0;
        }
        StartCoroutine(SetJuiceActive());
    }

    public IEnumerator SetJuiceActive()
    {
        yield return new WaitForSeconds(ammoActiveTimer);

        if (lastIndex > -1 && lastIndex < juiceGameObjects.Count)
        {
            juiceGameObjects[lastIndex].SetActive(true);
            lastIndex++;
            if (lastIndex > juiceGameObjects.Count - 1)
            {
                lastIndex = juiceGameObjects.Count - 1;
            }
        }
    }

    void FindCurrentChassis()
    {
        currentChassis = Inventory.Instance.currentEquippedGO.GetComponent<Item>();
    }

    void Update()
    {
        if (itemType != TypeTag.ammo)
        {
            Debug.LogError($"{itemName} is currently of {itemType} type and not Ammo!");
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
