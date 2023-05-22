using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class QuestExternalManager : MonoBehaviour
{
    public void ActivateExternalObjective(string externalObjectiveName)
    {
        Debug.Log(externalObjectiveName);

        if (QuestManager.Instance.IsCurrentQuestActive())
        {
            Debug.Log("Current Quest Active");
            Objective currentObjective = QuestManager.Instance.GetCurrentQuest().GetCurrentObjective();
            if (currentObjective != null)
            {
                Debug.Log("Called");
                currentObjective.ExternalObjective(externalObjectiveName);
            }
            else
            {
                Debug.Log("Objective is null");
            }
        }
    }
}
