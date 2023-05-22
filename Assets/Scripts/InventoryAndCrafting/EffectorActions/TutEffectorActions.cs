using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TutEffectorActions : EffectorActions, ISaveable
{
    public List<Renderer> characterRenderers = new List<Renderer>();
    public Material[] updateMaterials;
    public string completionObjectiveName;
    public bool hasBeenHit = false;
    public bool canHit = false;

    public object CaptureState()
    {
        return new SaveData
        {
            activatedStatus = hasBeenHit,
            hitStatus = canHit
        };
    }

    public void RestoreState(object state)
    {
        var saveData = (SaveData)state;

        if (saveData.activatedStatus)
        {
            PowerEffectorAction();
        }

        canHit = saveData.hitStatus;
    }

    [System.Serializable]
    private struct SaveData
    {
        public bool activatedStatus;
        public bool hitStatus;
    }

    public void UpdateCanHit(bool nHitStatus)
    {
        canHit = nHitStatus;
    }

    public override void PowerEffectorAction()
    {
        if (!canHit)
        {
            return;
        }

        if (!hasBeenHit)
        {
            for (int i = 0; i < characterRenderers.Count; i++)
            {
                characterRenderers[i].materials = updateMaterials;
            }

            hasBeenHit = true;

            if (QuestManager.Instance.IsCurrentQuestActive())
            {
                Objective currentObjective = QuestManager.Instance.GetCurrentQuest().GetCurrentObjective();
                if (currentObjective != null)
                {
                    currentObjective.ExternalObjective(completionObjectiveName);
                }
            }
        }
    }
}
