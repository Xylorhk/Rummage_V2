using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InvisibleNPCEffectorActions : EffectorActions, ISaveable
{
    public DynamicNPCController npcController;
    public List<Renderer> characterRenderers = new List<Renderer>();
    public Material[] updateMaterials;
    public bool hasBeenHit = false;

    public object CaptureState()
    {
        return new SaveData
        {
            activatedStatus = hasBeenHit
        };
    }

    public void RestoreState(object state)
    {
        var saveData = (SaveData)state;

        if (saveData.activatedStatus)
        {
            PowerEffectorAction();
        }
    }

    [System.Serializable]
    private struct SaveData
    {
        public bool activatedStatus;
    }

    public override void PowerEffectorAction()
    {
        if (!hasBeenHit)
        {
            for (int i = 0; i < characterRenderers.Count; i++)
            {
                characterRenderers[i].materials = updateMaterials;
            }

            hasBeenHit = true;
            npcController.ShouldTalk(true);
        }
    }
}
