using BasicTools.ButtonInspector;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LiquidDrain : MonoBehaviour, ISaveable
{
    public GameObject liquidPlane;
    public Transform drainTransform;
    public bool shouldDrain = false;
    public bool hasDrainedPrior = false;
    public bool stopDraining = false;
    public float drainSpeed = 5f;

    [Header("Debugging Buttons")]
    [Button("Drain Oil", "StartDraining")]
    [SerializeField] private bool _btnDrain;

    public object CaptureState()
    {
        return new SaveData
        {
            hasLiquidDrained = hasDrainedPrior
        };
    }

    public void RestoreState(object state)
    {
        var saveData = (SaveData)state;

        hasDrainedPrior = saveData.hasLiquidDrained;
    }

    [Serializable]
    private struct SaveData
    {
        public bool hasLiquidDrained;
    }

    private void OnDrawGizmosSelected()
    {
        if (drainTransform != null)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawSphere(drainTransform.position, 0.5f);
        }
    }

    public void StartDraining()
    {
        if (!hasDrainedPrior)
        {
            shouldDrain = true;
            hasDrainedPrior = true;
        }
    }

    private void Update()
    {
        if (stopDraining)
        {
            return;
        }

        if (hasDrainedPrior && !shouldDrain)
        {
            liquidPlane.transform.position = new Vector3(liquidPlane.transform.position.x, drainTransform.transform.position.y, liquidPlane.transform.position.z);
            stopDraining = true;
        }

        if (shouldDrain && liquidPlane.transform.position.y > drainTransform.position.y)
        {
            liquidPlane.transform.position = liquidPlane.transform.position - Vector3.up * drainSpeed * Time.deltaTime;
        }
        else if (shouldDrain && liquidPlane.transform.position.y <= drainTransform.position.y)
        {
            GameManager.Instance.SaveScene();
            stopDraining = true;
        }
    }
}
