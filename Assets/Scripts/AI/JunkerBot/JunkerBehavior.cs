using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class JunkerBehavior : MonoBehaviour
{
    public JunkerBot junker;

    [Header("Basic Variables")]
    public float baseSpeed = 3.5f;
    public float currentSpeed;
    [HideInInspector] public bool shouldAct = true;

    [Header("Patrol Variables")]
    public List<PatrolPoint> patrolPoints = new List<PatrolPoint>();
    public float patrolSpeed = 0.5f;
    private int currentPatrolPointIndex = 0;

    [HideInInspector] public bool shouldRest = false;
    [HideInInspector] public bool hasRested = false;

    [Range(1, 5)]
    public float maxRandomRange = 5;
    [Range(2, 5)]
    public float minRandomRestTimer = 2;
    [Range(2, 10)]
    public float maxRandomRestTimer = 5;

    private float restTimer = 0;
    private bool shouldRandomPatrol = false;

    [Header("Attack Variables")]
    public float innerActRadius = 0;
    public float outerActRadius = 0;
    private bool isActing = false;

    [Header("Chase Variables")]
    public float chaseRadius = 0;
    public float chaseSpeed = 1.2f;
    public float updatePositionDist = 0.2f;
    private Vector3 lastKnownPosition;

    private void Awake()
    {
        junker = GetComponent<JunkerBot>();
    }

    void OnDrawGizmosSelected()
    {
        if (junker != null && junker.showActRadius)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position, innerActRadius);

            if (outerActRadius <= innerActRadius)
            {
                outerActRadius = innerActRadius + 1;
            }

            Gizmos.color = new Color(1, 0.5f, 0, 1);
            Gizmos.DrawWireSphere(transform.position, outerActRadius);
        }

        if (junker != null && junker.showChaseRadius)
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireSphere(transform.position, chaseRadius);
        }
    }

    /// <summary>
    /// Start of Patrol functions
    /// </summary>

    private void MakeNewPatrolPoint()
    {
        shouldRandomPatrol = true;

        if (patrolPoints.Count > 0)
        {
            if (patrolPoints.Count > 1)
            {
                patrolPoints.RemoveRange(0, patrolPoints.Count - 1);
            }

            patrolPoints[0].patrolTransform.position = transform.position;
            patrolPoints[0].patrolTransform.rotation = transform.rotation;
            patrolPoints[0].restTime = Random.Range(1f, maxRandomRestTimer);
        }
        else
        {
            GameObject patrolGO = new GameObject();
            patrolGO.name = "PatrolPoint1";
            patrolGO.transform.parent = gameObject.transform.root;
            patrolGO.transform.position = transform.position;
            patrolGO.transform.rotation = transform.rotation;

            PatrolPoint newPatrolPoint = new PatrolPoint();
            newPatrolPoint.patrolTransform = patrolGO.transform;
            newPatrolPoint.restTime = Random.Range(1f, maxRandomRestTimer);

            patrolPoints.Add(newPatrolPoint);
        }
    }

    private void MakeRandomPatrolPoint()
    {
        if (!shouldRandomPatrol)
        {
            return;
        }

        Vector3 randomPoint = transform.position + Random.insideUnitSphere * maxRandomRange;
        Vector3 finalPos = transform.position;

        NavMeshHit navHit;
        if (NavMesh.SamplePosition(randomPoint, out navHit, maxRandomRange, NavMesh.AllAreas))
        {
            NavMeshPath tempPath = new NavMeshPath();
            if (junker.nav.CalculatePath(navHit.position, tempPath))
            {
                if (tempPath.status != NavMeshPathStatus.PathComplete)
                {
                    NavMeshHit closestEdge;
                    if (NavMesh.FindClosestEdge(transform.position, out closestEdge, NavMesh.AllAreas))
                    {
                        finalPos = transform.position + ((navHit.position - closestEdge.position).normalized);
                    }
                    else
                    {
                        Debug.Log("Closest edge not found!!");
                    }
                }
                else
                {
                    finalPos = navHit.position;
                }
            }
        }
        else
        {
            Debug.Log("SamplePosition not found!");
        }

        if (patrolPoints.Count == 1)
        {
            GameObject patrolGO = new GameObject();
            patrolGO.name = "PatrolPoint2";
            patrolGO.transform.parent = junker.rootObject.transform;
            patrolGO.transform.position = finalPos;
            if (finalPos != transform.position)
            {
                patrolGO.transform.rotation = Quaternion.LookRotation(finalPos - transform.position);
            }
            else
            {
                patrolGO.transform.rotation = transform.rotation;
            }
            

            PatrolPoint newPatrolPoint = new PatrolPoint();
            newPatrolPoint.patrolTransform = patrolGO.transform;
            newPatrolPoint.restTime = Random.Range(1f, maxRandomRestTimer);

            patrolPoints.Add(newPatrolPoint);
        }
        else
        {
            patrolPoints[1].patrolTransform.position = finalPos;
            if (finalPos != transform.position)
            {
                patrolPoints[1].patrolTransform.rotation = Quaternion.LookRotation(finalPos - transform.position);
            }
            else
            {
                patrolPoints[1].patrolTransform.rotation = transform.rotation;
            }

            patrolPoints[1].restTime = Random.Range(1f, maxRandomRestTimer);
        }
    }

    private bool CanReachPatrolPoint()
    {
        for (int i = 0; i < patrolPoints.Count; i++)
        {
            NavMeshPath tempPath = new NavMeshPath();
            if (junker.nav.CalculatePath(patrolPoints[i].patrolTransform.position, tempPath))
            {
                if (tempPath.status != NavMeshPathStatus.PathComplete)
                {
                    return false;
                }
            }
            else
            {
                return false;
            }
        }

        return true;
    }

    private int FindClosestPatrolPoint()
    {
        if (patrolPoints.Count == 0)
        {
            MakeNewPatrolPoint();
            return 0;
        }

        if (!CanReachPatrolPoint())
        {
            MakeNewPatrolPoint();
            return 0;
        }

        float minDistance = Vector3.Distance(transform.position, patrolPoints[0].patrolTransform.position);
        int minIndex = 0;

        if (patrolPoints.Count == 1)
        {
            return minIndex;
        }

        for (int i = 1; i < patrolPoints.Count; i++)
        {
            float currentDistance = Vector3.Distance(transform.position, patrolPoints[i].patrolTransform.position);

            if (currentDistance < minDistance)
            {
                minDistance = currentDistance;
                minIndex = i;
            }
        }

        return minIndex;
    }

    private bool RotateTowards(Vector3 target)
    {
        Quaternion lookRotation = Quaternion.LookRotation(new Vector3(target.x, 0, target.z));
        transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, Time.deltaTime * 3f);

        if (Mathf.Abs(Quaternion.Angle(transform.rotation, lookRotation)) < 5)
        {
            return false;
        }
        else
        {
            return true;
        }
    }

    private void Rest(float currentRestTime)
    {
        restTimer = currentRestTime;
    }

    public void StartPatrolling()
    {
        currentPatrolPointIndex = FindClosestPatrolPoint();
        currentSpeed = patrolSpeed;

        if (currentPatrolPointIndex == -1)
        {
            return;
        }

        junker.nav.SetDestination(patrolPoints[currentPatrolPointIndex].patrolTransform.position);

        shouldRest = false;
        hasRested = false;
    }

    public void Patrol()
    {
        if (shouldRest || patrolPoints.Count == 0)
        {
            return;
        }

        if (currentPatrolPointIndex == 0 && shouldRandomPatrol)
        {
            MakeRandomPatrolPoint();
        }

        if (junker.nav.remainingDistance < junker.nav.stoppingDistance)
        {
            if (patrolPoints[currentPatrolPointIndex].restTime != 0 && !hasRested && !shouldRest)
            {
                shouldRest = true;
                Rest(patrolPoints[currentPatrolPointIndex].restTime);
            }
            else if ((hasRested && !shouldRest) || patrolPoints[currentPatrolPointIndex].restTime == 0)
            {
                if (currentPatrolPointIndex + 1 == patrolPoints.Count)
                {
                    currentPatrolPointIndex = 0;
                }
                else
                {
                    currentPatrolPointIndex++;
                }

                junker.nav.SetDestination(patrolPoints[currentPatrolPointIndex].patrolTransform.position);
                hasRested = false;
            }
        }
    }

    /// <summary>
    /// Start of Chase functions
    /// </summary>

    public void SetLastKnowPosition(Vector3 lastKnowPos)
    {
        lastKnownPosition = lastKnowPos;
        junker.nav.SetDestination(lastKnownPosition);

        currentSpeed = chaseSpeed;

        shouldRest = false;
        hasRested = false;
    }

    public void Chase()
    {
        Vector3 tempPos = new Vector3(transform.position.x, 0, transform.position.z);
        Vector3 tempLast = new Vector3(lastKnownPosition.x, 0, lastKnownPosition.z);
        if (!junker.junkerFOV.FindPlayer() && Vector3.Distance(tempPos, tempLast) < junker.nav.stoppingDistance)
        {
            junker.stateMachine.switchState(JunkerStateMachine.StateType.Patrol);
            return;
        }

        UnityEngine.AI.NavMeshPath tempPath = new UnityEngine.AI.NavMeshPath();
        if (junker.nav.CalculatePath(Player.Instance.transform.position, tempPath))
        {
            if (tempPath.status != UnityEngine.AI.NavMeshPathStatus.PathComplete)
            {
                junker.stateMachine.switchState(JunkerStateMachine.StateType.Patrol);
                return;
            }
        }

        Vector3 playerPos = Player.Instance.transform.position;

        if (Vector3.Distance(playerPos, lastKnownPosition) > updatePositionDist && junker.junkerFOV.FindPlayer())
        {
            lastKnownPosition = playerPos;
            junker.nav.SetDestination(lastKnownPosition);
        }
    }

    /// <summary>
    /// Start of Act functions
    /// </summary>

    public void PerformAction()
    {
        junker.anim.SetTrigger("StartAction");
        junker.nav.SetDestination(transform.position);
        junker.nav.isStopped = true;
    }

    /// <summary>
    /// Start of Disabled functions
    /// </summary>
    
    public void Disable()
    {
        junker.ToggleActive(false);
    }

    private void Update()
    {
        if (shouldRest && junker.stateMachine.GetCurrentState() == JunkerStateMachine.StateType.Patrol)
        {
            if (!RotateTowards(patrolPoints[currentPatrolPointIndex].patrolTransform.forward))
            {
                if (junker.anim.GetBool("IsWalking"))
                {
                    junker.anim.SetBool("IsWalking", false);
                }
            }

            restTimer -= Time.deltaTime;
            if (restTimer <= 0)
            {
                shouldRest = false;
                hasRested = true;
            }
        }

        if (junker.stateMachine.GetCurrentState() != JunkerStateMachine.StateType.Disabled)
        {
            if (junker.nav.remainingDistance > junker.nav.stoppingDistance)
            {
                junker.anim.SetBool("IsWalking", true);
            }
        }
    }
}
