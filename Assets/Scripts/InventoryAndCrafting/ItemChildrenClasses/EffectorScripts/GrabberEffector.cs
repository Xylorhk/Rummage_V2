using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GrabberEffector : Item
{
    [Header("Grab Effector Variables")]
    public Transform grabTransform;
    public float grabRadius = 0;
    public float breakTorque = 0;
    public float breakForce = 0;

    public GameObject currentAttachedObj = null;
    private float currentAttachedMass = 0;

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(grabTransform.position, grabRadius);
    }

    public override void Activate()
    {
        if (currentAttachedObj == null)
        {
            if (Player.Instance.playerInput.actions["Fire"].WasPressedThisFrame() && Player.Instance.playerInput.actions["Aim"].IsPressed())
            {
                TryGrab();

                if (QuestManager.Instance.IsCurrentQuestActive())
                {
                    Objective currentObjective = QuestManager.Instance.GetCurrentQuest().GetCurrentObjective();
                    if (currentObjective != null)
                    {
                        currentObjective.ActivateItem(itemName);
                    }
                }
            }
        }
        else
        {
            if (Player.Instance.playerInput.actions["Fire"].WasPressedThisFrame() && Player.Instance.playerInput.actions["Aim"].IsPressed())
            {
                DropCurrentObj();
            }
        }
    }

    void TryGrab()
    {
        Collider[] collidersInRange = Physics.OverlapSphere(grabTransform.position, grabRadius);

        for (int i = 0; i < collidersInRange.Length; i++)
        {
            if (collidersInRange[i].gameObject == this.gameObject || collidersInRange[i].gameObject == Player.Instance.gameObject)
            {
                continue;
            }

            MeshCollider meshCheck = collidersInRange[i].gameObject.GetComponent<MeshCollider>();
            if (meshCheck != null)
            {
                if(meshCheck == collidersInRange[i])
                {
                    continue;
                }
            }

            Rigidbody tempRB = null;
            tempRB = collidersInRange[i].gameObject.GetComponentInChildren<Rigidbody>();

            if(tempRB != null)
            {
                if (tempRB.mass >= 100)
                {
                    continue;
                }

                Elevator elevator = collidersInRange[i].gameObject.GetComponent<Elevator>();
                if (elevator != null)
                {
                    continue;
                }

                NPCCharacter character = collidersInRange[i].gameObject.GetComponent<NPCCharacter>();
                if (character != null)
                {
                    continue;
                }

                JunkerBot junkerInRange = collidersInRange[i].gameObject.GetComponent<JunkerBot>();
                if (junkerInRange != null)
                {
                    junkerInRange.stateMachine.switchState(JunkerStateMachine.StateType.Disabled);
                    junkerInRange.GrabToggle(true);
                    junkerInRange.junkerScoop.scoopCollider.enabled = false;
                }

                Grab(collidersInRange[i].gameObject, collidersInRange[i], tempRB);
                return;
            }
        }
    }

    void Grab(GameObject nObj, Collider nCollider, Rigidbody nRB)
    {
        currentAttachedObj = nObj;

        currentAttachedMass = nRB.mass;
        nRB.mass = 1;

        FixedJoint addedJoint = nObj.AddComponent<FixedJoint>();
        addedJoint.connectedBody = this.gameObject.GetComponent<Rigidbody>();
        
        Physics.IgnoreCollision(nCollider, Player.Instance.GetComponent<Collider>(), true);
        
        nRB.velocity = Vector3.zero;
        nRB.angularVelocity = Vector3.zero;
    }

    public GameObject DropCurrentObj()
    {
        if (currentAttachedObj == null)
        {
            return null;
        }

        JunkerBot tempJunker = currentAttachedObj.GetComponentInChildren<JunkerBot>();
        if (tempJunker != null)
        {
            Physics.IgnoreCollision(tempJunker.primaryCollider, Player.Instance.primaryCollider, false);
            Destroy(tempJunker.GetComponent<FixedJoint>());

            tempJunker.junkerScoop.scoopCollider.enabled = true;
            tempJunker.primaryRigidbody.mass = currentAttachedMass;
            currentAttachedMass = 0;

            tempJunker.GrabToggle(false);
            currentAttachedObj = null;
            return tempJunker.gameObject;
        }

        Physics.IgnoreCollision(currentAttachedObj.GetComponent<Collider>(), Player.Instance.primaryCollider, false);
        Destroy(currentAttachedObj.GetComponent<FixedJoint>());

        currentAttachedObj.GetComponent<Rigidbody>().mass = currentAttachedMass;
        currentAttachedMass = 0;

        GameObject tempGO = currentAttachedObj;
        currentAttachedObj = null;
        return tempGO;
    }

    public override void OnUnequip()
    {
        if (currentAttachedObj != null)
        {
            DropCurrentObj();
        }
    }

    void Update()
    {
        if (itemType != TypeTag.effector)
        {
            Debug.LogError($"{itemName} is currently of {itemType} type and not effector!");
        }
    }

    private void FixedUpdate()
    {
        if (currentAttachedObj != null)
        {
            FixedJoint currentFixedJoint = currentAttachedObj.GetComponent<FixedJoint>();
            if (currentFixedJoint.currentTorque.magnitude > breakTorque || currentFixedJoint.currentForce.magnitude > breakForce)
            {
                Debug.Log("Tor: " + currentFixedJoint.currentTorque.magnitude + ", For: " + currentFixedJoint.currentForce.magnitude);
                DropCurrentObj();
            }
        }
    }
}
