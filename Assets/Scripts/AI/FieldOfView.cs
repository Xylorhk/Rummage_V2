using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FieldOfView : MonoBehaviour
{
    public Enemy enemy;

    [Header("DetectionVariables")]
    public float detectionRadius;

    [Header("Field of view Variables")]
    [Range(0, 360)] public float viewAngle;
    public List<Transform> visibleTargets = new List<Transform>();

    void Awake()
    {
        enemy = GetComponent<Enemy>();
    }

    void OnDrawGizmosSelected()
    {
        if (enemy != null && enemy.showFOV)
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

            if (enemy.enemyStateMachine.GetCurrentState() != EnemyStateMachine.StateType.Chase
                || enemy.enemyStateMachine.GetCurrentState() != EnemyStateMachine.StateType.Attack)
            {
                if (distToTarget > detectionRadius)
                {
                    continue;
                }
            }
            else
            {
                if (distToTarget > enemy.enemyBehavior.chaseRadius)
                {
                    continue;
                }
            }
            

            Vector3 dirToTarget = (targets[i].position - transform.position).normalized;

            if (IsTargetInFOV(dirToTarget))
            {
                LayerMask invertedEnemyMask = ~enemy.enemyMask;
                RaycastHit hitInfo;

                if ((Physics.Raycast(transform.position, dirToTarget, out hitInfo, distToTarget, invertedEnemyMask)))
                {
                    if (hitInfo.collider != Player.Instance.primaryCollider)
                    {
                        continue;
                    }

                    if (enemy.enemyStateMachine.GetCurrentState() == EnemyStateMachine.StateType.Patrol 
                        || enemy.enemyStateMachine.GetCurrentState() == EnemyStateMachine.StateType.LostPlayer)
                    {
                        if (distToTarget < enemy.enemyBehavior.innerAttackRadius)
                        {
                            enemy.enemyStateMachine.switchState(EnemyStateMachine.StateType.Attack);
                        }
                        else
                        {
                            enemy.enemyStateMachine.switchState(EnemyStateMachine.StateType.Chase);
                        }
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
