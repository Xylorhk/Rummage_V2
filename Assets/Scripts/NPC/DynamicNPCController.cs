using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DynamicNPCController : NPCCharacter
{
    public GameObject activationTextGO;
    public TMPro.TextMeshPro npcTextMesh;

    public TextAsset basicTextFile = null;
    public Sprite talkerIcon = null;

    public void SetTextFile(TextAsset textFile)
    {
        basicTextFile = textFile;
    }

    public void MoveCharacter(Transform nTransform)
    {
        transform.position = nTransform.position;
        transform.rotation = nTransform.rotation;
    }

    public void ShouldTalk(bool shouldTalk)
    {
        canTalk = shouldTalk;
    }

    void TryTalk()
    {
        if (!canTalk)
        {
            activationTextGO.SetActive(false);
            npcTextMesh.text = "";
            return;
        }

        Collider[] collidersInRange = Physics.OverlapSphere(transform.position, talkRadius);
        
        if (collidersInRange.Length == 0)
        {
            activationTextGO.SetActive(false);
            npcTextMesh.text = "";
            return;
        }

        bool playerInRange = false;

        for (int i = 0; i < collidersInRange.Length; i++)
        {
            if (collidersInRange[i].gameObject.GetComponentInChildren<Player>() != null)
            {
                playerInRange = true;
                if (basicTextFile != null)
                {
                    activationTextGO.SetActive(true);
                    npcTextMesh.text = $"Press 'T' to talk to {characterName}";
                }
                else
                {
                    if (QuestManager.Instance.IsCurrentQuestActive())
                    {
                        Objective currentObjective = QuestManager.Instance.GetCurrentQuest().GetCurrentObjective();
                        if (currentObjective != null)
                        {
                            if (currentObjective.goalType == Objective.GoalType.Talk)
                            {
                                if (currentObjective.npcName == characterName)
                                {
                                    activationTextGO.SetActive(true);
                                    npcTextMesh.text = $"Press 'T' to talk to {characterName}";
                                }
                                else
                                {
                                    break;
                                }
                            }
                        }
                    }
                }

                if (Player.Instance.playerInput.actions["Interact"].WasPressedThisFrame() && Player.Instance.vThirdPersonInput.CanMove())
                {
                    activationTextGO.SetActive(false);

                    if (QuestManager.Instance.IsCurrentQuestActive())
                    {
                        Objective currentObjective = QuestManager.Instance.GetCurrentQuest().GetCurrentObjective();
                        if (currentObjective != null)
                        {
                            if (currentObjective.goalType == Objective.GoalType.Talk)
                            {
                                if (currentObjective.npcName == characterName)
                                {
                                    KeyValuePair<TextAsset, Sprite> textboxInput = currentObjective.NPCTalkedTo(characterName);
                                    Textbox.Instance.EnableTextbox(textboxInput.Key, textboxInput.Value, false);

                                    break;
                                }
                            }
                        }
                    }

                    Textbox.Instance.EnableTextbox(basicTextFile, talkerIcon, false);
                    break;
                }
            }

            if (!playerInRange)
            {
                activationTextGO.SetActive(false);
                npcTextMesh.text = "";
            }
        }
    }

    private void Update()
    {
        TryTalk();
    }
}
