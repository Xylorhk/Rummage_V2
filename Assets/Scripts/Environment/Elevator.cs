using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Elevator : MonoBehaviour
{
    [SerializeField] private bool isActivated;
    
    public bool startAtTop = false;
    public Transform topTransform;
    public Transform bottomTransform;
    [Range(1,10)]
    public float speed = 3;

    public float floorWaitingTimer = 5f;
    private float maxFloorWaitingTimer = 0f;

    public float elevatorStuckTimer = 4f;
    private float maxElevatorStuckTimer = 0f;

    public BoxCollider primaryCollider;
    public BoxCollider triggerCollider;

    private bool shouldWait = true;
    private bool atBottomFloor;
    private bool isStuck = false;

    public void ShouldActivate(bool shouldActivate)
    {
        isActivated = shouldActivate;
    }

    private void Awake()
    {
        if (startAtTop)
        {
            topTransform.position = transform.position;
        }
        else
        {
            bottomTransform.position = transform.position;
        }

        atBottomFloor = !startAtTop;
        maxFloorWaitingTimer = floorWaitingTimer;
        maxElevatorStuckTimer = elevatorStuckTimer;
    }

    private void Update()
    {
        if (!isActivated || isStuck)
        {
            if (isStuck && isActivated)
            {

                elevatorStuckTimer -= Time.deltaTime;
                if (elevatorStuckTimer <= 0)
                {
                    isStuck = false;
                    elevatorStuckTimer = maxElevatorStuckTimer;
                    atBottomFloor = true;

                    return;
                }

                Collider[] collidersInRange = Physics.OverlapBox(transform.position + triggerCollider.center, triggerCollider.bounds.extents);
                bool somethingUnderElevator = false;
                if (collidersInRange.Length > 0)
                {
                    for (int i = 0; i < collidersInRange.Length; i++)
                    {
                        if (collidersInRange[i].gameObject.isStatic)
                        {
                            continue;
                        }
                        else if (collidersInRange[i] == primaryCollider)
                        {
                            continue;
                        }
                        else if (collidersInRange[i] == triggerCollider)
                        {
                            continue;
                        }
                        else if (collidersInRange[i].gameObject.transform.position.y < transform.position.y)
                        {
                            somethingUnderElevator = true;
                            break;
                        }
                    }

                    if (!somethingUnderElevator)
                    {
                        isStuck = false;
                        elevatorStuckTimer = maxElevatorStuckTimer;
                    }
                }
            }

            return;
        }

        if (shouldWait)
        {

            floorWaitingTimer -= Time.deltaTime;
            if (floorWaitingTimer <= 0)
            {
                shouldWait = false;
                floorWaitingTimer = maxFloorWaitingTimer;
            }

            return;
        }

        if (atBottomFloor)
        {
            transform.parent.transform.position = transform.parent.transform.position + Vector3.up * speed * Time.deltaTime;

            if (transform.position.y >= topTransform.position.y)
            {
                shouldWait = true;
                atBottomFloor = false;
            }
        }
        else
        {
            transform.parent.transform.position = transform.parent.transform.position + (-Vector3.up) * speed * Time.deltaTime;

            if (transform.position.y <= bottomTransform.position.y)
            {
                shouldWait = true;
                atBottomFloor = true;
            }
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.transform.position.y > transform.position.y)
        {
            if (!other.gameObject.isStatic)
            {
                GrabberEffector grabberCheck = Player.Instance.gameObject.GetComponentInChildren<GrabberEffector>();
                if (grabberCheck != null)
                {
                    if (grabberCheck.currentAttachedObj == other.gameObject)
                    {
                        return;
                    }
                }

                for (int i = 0; i < Player.Instance.ragdoll.ragdollColliders.Count; i++)
                {
                    if (Player.Instance.ragdoll.ragdollColliders[i] == other)
                    {
                        return;
                    }
                }

                Pitfall pitfallCheck = other.gameObject.GetComponent<Pitfall>();
                if (pitfallCheck != null)
                {
                    return;
                }

                other.gameObject.transform.parent = this.transform.parent;
            }
        }
        else
        {
            if (atBottomFloor || isStuck)
            {
                return;
            }

            Player playerCheck = other.gameObject.GetComponentInChildren<Player>();
            if (playerCheck != null)
            {
                if (Player.Instance.IsAlive() && !Player.Instance.ragdoll.IsRagdolled())
                {
                    Player.Instance.FallDeath();
                }
                else if (Player.Instance.ragdoll.IsRagdolled())
                {
                    isStuck = true;
                }
            }
            else
            {
                if (!other.gameObject.isStatic)
                {
                    isStuck = true;
                }
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject.transform.position.y > transform.position.y)
        {
            Player playerCheck = other.gameObject.GetComponent<Player>();
            if (playerCheck != null)
            {
                Player.Instance.transform.parent = Player.Instance.rootObj.transform;
            }
        }
        else
        {
            isStuck = false;
        }
    }
}
