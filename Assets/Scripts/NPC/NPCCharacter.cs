using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NPCCharacter : MonoBehaviour, ISaveable
{
    public string characterName;
    public float talkRadius = 2.5f;
    public bool canTalk = true;

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

        return new SaveData
        {
            npcPosition = currentPosition,
            npcRotation = currentRotation,
            talkStatus = canTalk
        };
    }

    public void RestoreState(object state)
    {
        var saveData = (SaveData)state;

        transform.position = new Vector3(saveData.npcPosition[0], saveData.npcPosition[1], saveData.npcPosition[2]);
        transform.rotation = new Quaternion(saveData.npcRotation[0], saveData.npcRotation[1], saveData.npcRotation[2], saveData.npcRotation[3]);
        canTalk = saveData.talkStatus;
    }

    [System.Serializable]
    private struct SaveData
    {
        public List<float> npcPosition;
        public List<float> npcRotation;
        public bool talkStatus;
    }
}
