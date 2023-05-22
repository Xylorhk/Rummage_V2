using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HealthViewer : SingletonMonoBehaviour<HealthViewer>
{
    public GameObject heartUIObject;
    public GameObject healthPanel;

    public List<GameObject> heartUIObjects = new List<GameObject>();

    private int maxPlayerHealth;
    private int currentPlayerHealth = -1;

    private void Start()
    {
        maxPlayerHealth = (int)Player.Instance.health.maxHealth;

        InitHealth();
    }

    void InitHealth()
    {
        for (int i = 0; i < maxPlayerHealth; i++)
        {
            GameObject currentObj = Instantiate(heartUIObject);
            currentObj.transform.SetParent(healthPanel.transform, false);
            heartUIObjects.Add(currentObj);
        }
    }

    public void UpdateHealthView(int currentHealth)
    {
        if (currentPlayerHealth != currentHealth)
        {
            foreach(GameObject go in heartUIObjects)
            {
                go.SetActive(false);
            }

            for (int i = 0; i < currentHealth; i++)
            {
                heartUIObjects[i].SetActive(true);
            }

            currentPlayerHealth = currentHealth;
        }
    }

    private void Update()
    {
        if (currentPlayerHealth != Player.Instance.health.currentHealth)
        {
            UpdateHealthView((int)Player.Instance.health.currentHealth);
        }
    }
}
