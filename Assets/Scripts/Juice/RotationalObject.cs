using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RotationalObject : MonoBehaviour, ISaveable
{
    public bool canRotate = true;
    public float rotationalSpeed = 90f;
    private float maxRotationalSpeed = 0f;

    public object CaptureState()
    {
        return new SaveData
        {
            rotationStatus = canRotate
        };
    }

    public void RestoreState(object state)
    {
        var saveData = (SaveData)state;

        canRotate = saveData.rotationStatus;
    }

    [System.Serializable]
    private struct SaveData
    {
        public bool rotationStatus;
    }

    void Awake()
    {
        maxRotationalSpeed = rotationalSpeed;
    }

    public void ShouldRotate(bool shouldRotate)
    {
        canRotate = shouldRotate;
    }

    private void Update()
    {
        if (rotationalSpeed <= 0)
        {
            return;
        }

        transform.RotateAround(transform.position, transform.up, Time.deltaTime * -rotationalSpeed);

        if (!canRotate && rotationalSpeed > 0)
        {
            rotationalSpeed -= (float)(Time.deltaTime * (maxRotationalSpeed * 0.3));
        }
    }
}
