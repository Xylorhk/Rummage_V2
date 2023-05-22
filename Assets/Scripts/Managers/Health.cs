using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// Edited a script from https://github.com/aziztitu
/// </summary>

public class Health : MonoBehaviour, ISaveable
{
    public float maxHealth;
    public float currentHealth;
    public bool startAtMaxHealth = true;
    
    [HideInInspector] public UnityEvent OnHealthDepleated;
    [HideInInspector] public UnityEvent OnHealthRestored;

    private bool healthDepleted = false;

    public object CaptureState()
    {
        return new SaveData
        {
            currentSavedHealth = currentHealth
        };
    }

    public void RestoreState(object state)
    {
        var saveData = (SaveData)state;

        if (saveData.currentSavedHealth > maxHealth || saveData.currentSavedHealth == 0)
        {
            currentHealth = maxHealth;
        }
        else
        {
            currentHealth = saveData.currentSavedHealth;
        }
    }

    [System.Serializable]
    private struct SaveData
    {
        public float currentSavedHealth;
    }

    private void Awake()
    {
        if (startAtMaxHealth)
        {
            currentHealth = maxHealth;
        }
    }

    void Update()
    {
        if (!healthDepleted && currentHealth <= 0)
        {
            healthDepleted = true;
            OnHealthDepleated?.Invoke();
        }
    }

    public void UpdateHealth(float updateAmount)
    {
        currentHealth += updateAmount;
        
        if (currentHealth < 0)
        {
            currentHealth = 0;
        }
        else if (currentHealth > maxHealth)
        {
            currentHealth = maxHealth;
        }

        if (healthDepleted && currentHealth > 0)
        {
            healthDepleted = false;
            OnHealthRestored?.Invoke();
        }

        GameManager.Instance.SaveScene();
    }

    public void TakeDamage(float damage)
    {
        UpdateHealth(-damage);
    }

    public void FullHeal()
    {
        UpdateHealth(maxHealth);
    }
}
