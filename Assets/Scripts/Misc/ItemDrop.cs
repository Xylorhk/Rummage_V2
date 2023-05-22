using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemDrop : MonoBehaviour
{
    public ObjectPooler.Key objectKeyToSpawn;

    public void DropItems(Vector3 spawnOrigin, float yLevel, float radius, int numbToSpawn)
    {
        Inventory tempInventory = Inventory.Instance;
        int finalNumbToSpawn = 0;
        if(tempInventory.amountOfScrap == 0)
        {
            return;
        }
        else
        {
            finalNumbToSpawn = Mathf.Min(numbToSpawn, tempInventory.amountOfScrap);
            tempInventory.amountOfScrap -= finalNumbToSpawn;
        }

        for (int i = 0; i < finalNumbToSpawn; i++)
        {
            Vector3 currentSpawnPoint = spawnOrigin + (((spawnOrigin +Random.insideUnitSphere) * radius) - spawnOrigin);
            currentSpawnPoint = new Vector3(currentSpawnPoint.x, yLevel, currentSpawnPoint.z);

            GameObject obj  = ObjectPooler.GetPooler(objectKeyToSpawn).GetPooledObject();
            obj.transform.position = currentSpawnPoint;
            obj.transform.rotation = Quaternion.LookRotation(obj.transform.position - currentSpawnPoint);

            obj.SetActive(true);
            obj.GetComponent<Rigidbody>().AddForce(obj.transform.forward * 5);
        }
    }
}
