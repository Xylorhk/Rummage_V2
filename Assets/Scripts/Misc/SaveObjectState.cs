using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SaveObjectState : MonoBehaviour, ISaveable
{
    public object CaptureState()
    {
        List<float> currentPosition = new List<float>();
        currentPosition.Add(transform.position.x);
        currentPosition.Add(transform.position.y);
        currentPosition.Add(transform.position.z);

        List<float> currentRotation = new List<float>();
        currentRotation.Add(transform.rotation.x);
        currentRotation.Add(transform.rotation.y);
        currentRotation.Add(transform.rotation.z);
        currentRotation.Add(transform.rotation.w);

        bool tempRBState = false;

        if (rb != null)
        {
            tempRBState = rb.isKinematic;
        }

        bool tempParentState = false;

        if (transform.parent != null)
        {
            tempParentState = true;
        }

        return new SaveData
        {
            position = currentPosition,
            rotation = currentRotation,
            savedKinematicState = tempRBState,
            savedParentState = tempParentState
        };
    }

    public void RestoreState(object state)
    {
        var saveData = (SaveData)state;

        if (transform.parent != null)
        {
            if (!hasParent)
            {
                transform.parent = null;
            }
        }

        transform.position = new Vector3(saveData.position[0], saveData.position[1], saveData.position[2]);
        transform.rotation = new Quaternion(saveData.rotation[0], saveData.rotation[1], saveData.rotation[2], saveData.rotation[3]);

        if (rb != null)
        {
            rb.isKinematic = saveData.savedKinematicState;
        }
    }

    [System.Serializable]
    private struct SaveData
    {
        public List<float> position;
        public List<float> rotation;
        public bool savedKinematicState;
        public bool savedParentState;
    }

    Rigidbody rb;
    bool hasParent = false;

    private void Awake()
    {
        if (GetComponent<Rigidbody>())
        {
            rb = GetComponent<Rigidbody>();
        }
    }
}
