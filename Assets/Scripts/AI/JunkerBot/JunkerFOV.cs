using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class JunkerFOV : MonoBehaviour
{
    public JunkerBot junker;

    [Header("DetectionVariables")]
    public float detectionRadius;

    [Header("Field of view Variables")]
    [Range(0, 360)] public float viewAngle;
    public List<Transform> visibleTargets = new List<Transform>();

    void Awake()
    {
        junker = GetComponent<JunkerBot>();
    }

    void OnDrawGizmosSelected()
    {
        if (junker != null && junker.showFOV)
        {
            Gizmos.color = Color.blue;
            Gizmos.DrawWireSphere(transform.position, detectionRadius);

            Vector3 viewAngleA = DirFromAngle(-viewAngle / 2, false);
            Vector3 viewAngleB = DirFromAngle(viewAngle / 2, false);

            Gizmos.color = Color.green;
            Gizmos.DrawLine(transform.position, transform.position + viewAngleA * detectionRadius);
            Gizmos.DrawLine(transform.position, transform.position + viewAngleB * detectionRadius);

            Gizmos.color = Color.red;
            foreach (Transform visibleTarget in visibleTargets)
            {
                Gizmos.DrawLine(transform.position, visibleTarget.position);
            }
        }
    }

    public bool FindPlayer()
    {
        visibleTargets.Clear();

        List<Transform> targets = Player.Instance.pickup.raycastOrigins;
        for (int i = 0; i < targets.Count; i++)
        {
            float distToTarget = Vector3.Distance(transform.position, targets[i].transform.position);

            if (junker.stateMachine.GetCurrentState() != JunkerStateMachine.StateType.Chase
                && junker.stateMachine.GetCurrentState() != JunkerStateMachine.StateType.Act)
            {
                if (distToTarget > detectionRadius)
                {
                    continue;
                }
            }
            else
            {
                if (distToTarget > junker.behavior.chaseRadius)
                {
                    continue;
                }
            }


            Vector3 dirToTarget = (targets[i].position - transform.position).normalized;

            if (IsTargetInFOV(dirToTarget))
            {
                LayerMask invertedEnemyMask = ~junker.junkerMask;
                RaycastHit hitInfo;

                if ((Physics.Raycast(transform.position, dirToTarget, out hitInfo, distToTarget, invertedEnemyMask)))
                {
                    if (hitInfo.collider != Player.Instance.primaryCollider)
                    {
                        continue;
                    }

                    NavMeshPath navPath = new NavMeshPath();
                    if (junker.nav.CalculatePath(targets[i].position, navPath))
                    {
                        if (navPath.status != NavMeshPathStatus.PathComplete)
                        {
                            return false;
                        }
                    }
                    else
                    {
                        return false;
                    }

                    if (junker.stateMachine.GetCurrentState() == JunkerStateMachine.StateType.Patrol && Player.Instance.vThirdPersonInput.CanMove())
                    {
                        junker.stateMachine.switchState(JunkerStateMachine.StateType.Chase);
                    }

                    visibleTargets.Add(targets[i]);
                    return true;
                }
            }
        }

        return false;
    }

    public bool IsTargetInFOV(Vector3 dirToTarget)
    {
        if (Vector3.Angle(transform.forward, dirToTarget) < viewAngle / 2)
        {
            return true;
        }
        else
        {
            return false;
        }
    }

    public Vector3 DirFromAngle(float angleInDegrees, bool angleIsGlobal)
    {
        if (!angleIsGlobal)
        {
            angleInDegrees += transform.eulerAngles.y;
        }

        return new Vector3(Mathf.Sin(angleInDegrees * Mathf.Deg2Rad), 0.0f, Mathf.Cos(angleInDegrees * Mathf.Deg2Rad));
    }
}
