using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class PatrolPoint
{
    public Transform patrolTransform;
    public float restTime = 0;
}

public class EnemyBehavior : MonoBehaviour
{
    public Enemy enemy;

    [Header("Basic Variables")]
    public float baseSpeed = 3.5f;
    public float currentSpeed;
    [HideInInspector] public bool shouldAct = true;

    [Header("Patrol Variables")]
    public List<PatrolPoint> patrolPoints = new List<PatrolPoint>();
    public float patrolSpeed = 0.5f;
    private int currentPatrolPointIndex = 0;
    public bool shouldRest = false;
    public bool hasRested = false;
    private float restTimer = 0;

    [Header("Attack Variables")]
    public float innerAttackRadius = 0;
    public float outerAttackRadius = 0;
    public float attackTimer = 1.2f;
    private bool isAttacking = false;
    private float maxAttackTimer = 0;

    [Header("Chase Variables")]
    public float chaseRadius = 0;
    public float chaseSpeed = 1.2f;
    public float updatePositionDist = 0.2f;
    private Vector3 lastKnownPosition;

    [Header("LostPlayer")]
    public float lostSearchTime = 12.5f;

    [Header("Search Variables")]
    public bool turnRight = true;
    private bool initialTurnRight;

    [Range(10, 180)]
    public float maxTurnAngle = 120;
    public float turnSpeed = 5;
    
    public float searchTimer = 1;
    private float maxSearchTimer;
    private bool stopTurning = false;

    private Vector3 initialForward;
    private float initialEulerY;

    [Header("Stunned Variables")]
    public float stunTimer = 3f;
    private bool isStunned = false;
    private float maxStunTimer = 0;

    [Header("Juice")]
    public ObjectPooler.Key explosionParticleKey = ObjectPooler.Key.ExplosionParticle;

    private void Awake()
    {
        enemy = GetComponent<Enemy>();

        currentSpeed = baseSpeed;
        maxAttackTimer = attackTimer;
        maxSearchTimer = searchTimer;
        initialTurnRight = turnRight;
        maxStunTimer = stunTimer;
    }

    void OnDrawGizmosSelected()
    {
        if (enemy.showAttackRadius)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position, innerAttackRadius);

            if (outerAttackRadius <= innerAttackRadius)
            {
                outerAttackRadius = innerAttackRadius + 1;
            }

            Gizmos.color = new Color(1, 0.5f, 0, 1);
            Gizmos.DrawWireSphere(transform.position, outerAttackRadius);
        }

        if (enemy.showChaseRadius)
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireSphere(transform.position, chaseRadius);
        }
    }

    private int FindClosestPatrolPoint()
    {
        if (patrolPoints.Count == 0)
        {
            return -1;
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

    /// <summary>
    /// Start of Patrol functions
    /// </summary>

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

        enemy.nav.SetDestination(patrolPoints[currentPatrolPointIndex].patrolTransform.position);

        shouldRest = false;
        hasRested = false;
        stopTurning = false;
        turnRight = initialTurnRight;
        searchTimer = maxSearchTimer;
    }

    public void Patrol()
    {
        if (shouldRest || patrolPoints.Count == 0)
        {
            return;
        }

        if (enemy.nav.remainingDistance > enemy.nav.stoppingDistance)
        {
            enemy.thirdPersonCharacter.Move(enemy.nav.desiredVelocity, false, false);
        }
        else
        {
            enemy.thirdPersonCharacter.Move(Vector3.zero, false, false);

            if (patrolPoints[currentPatrolPointIndex].restTime != 0 && !hasRested && !shouldRest)
            {
                shouldRest = true;
                Rest(patrolPoints[currentPatrolPointIndex].restTime);
                StartSearch();
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

                enemy.nav.SetDestination(patrolPoints[currentPatrolPointIndex].patrolTransform.position);
                hasRested = false;
            }
        }
    }

    /// <summary>
    /// Start of Attack Functions
    /// </summary>

    public void StartAttack()
    {
        isAttacking = true;
        attackTimer = maxAttackTimer;

        enemy.nav.SetDestination(transform.position);
        enemy.anim.SetBool("IsAttacking", true);

        shouldRest = false;
        hasRested = false;
    }

    public void Attack()
    {
        Vector3 playerPos = Player.Instance.transform.position;

        if (!isAttacking)
        {
            if (!enemy.enemyFOV.FindPlayer())
            {
                enemy.enemyStateMachine.switchState(EnemyStateMachine.StateType.LostPlayer);
                isAttacking = false;
                return;
            }

            if (Vector3.Distance(playerPos, transform.position) > innerAttackRadius)
            {
                enemy.enemyStateMachine.switchState(EnemyStateMachine.StateType.Chase);
                isAttacking = false;
                return;
            }

            isAttacking = true;
        }

        enemy.thirdPersonCharacter.Move(Vector3.zero, false, false);
    }

    /// <summary>
    /// Start of Chase functions
    /// </summary>

    public void SetLastKnowPosition(Vector3 lastKnowPos)
    {
        lastKnownPosition = lastKnowPos;
        enemy.nav.SetDestination(lastKnownPosition);

        currentSpeed = chaseSpeed;

        shouldRest = false;
        hasRested = false;
    }

    public void Chase()
    {
        if (!enemy.enemyFOV.FindPlayer())
        {
            enemy.enemyStateMachine.switchState(EnemyStateMachine.StateType.LostPlayer);
            return;
        }
        Vector3 playerPos = Player.Instance.transform.position;

        if (Vector3.Distance(playerPos, transform.position) < innerAttackRadius)
        {
            enemy.enemyStateMachine.switchState(EnemyStateMachine.StateType.Attack);
            return;
        }

        if (Vector3.Distance(playerPos, lastKnownPosition) > updatePositionDist)
        {
            lastKnownPosition = playerPos;
            enemy.nav.SetDestination(lastKnownPosition);
        }

        if (enemy.nav.remainingDistance > enemy.nav.stoppingDistance)
        {
            enemy.thirdPersonCharacter.Move(enemy.nav.desiredVelocity, false, false);
        }
        else
        {
            enemy.thirdPersonCharacter.Move(Vector3.zero, false, false);
        }
    }

    /// <summary>
    /// Start of Lost Player Functions
    /// </summary>

    public void LostPlayer()
    {
        if (enemy.nav.remainingDistance > enemy.nav.stoppingDistance)
        {
            enemy.thirdPersonCharacter.Move(enemy.nav.desiredVelocity, false, false);
            Debug.Log("First");
        }
        else
        {
            enemy.thirdPersonCharacter.Move(Vector3.zero, false, false);

            if (!hasRested && !shouldRest)
            {
                shouldRest = true;
                Rest(lostSearchTime);
                StartSearch();
                Debug.Log("Second");
            }
            else if (hasRested && !shouldRest)
            {
                hasRested = false;
                Debug.Log("Third");

                enemy.enemyStateMachine.switchState(EnemyStateMachine.StateType.Patrol);
            }
        }
    }

    void StartSearch()
    {
        initialForward = transform.forward;
        initialEulerY = transform.rotation.eulerAngles.y;
        searchTimer = maxSearchTimer;
        turnRight = initialTurnRight;
    }

    void TurnCharacter()
    {
        if (stopTurning)
        {
            return;
        }

        float angle = Vector3.Angle(initialForward, transform.forward);
        Vector3 cross = Vector3.Cross(initialForward, transform.forward);
        if (cross.y < 0)
        {
            angle = -angle;
        }

        if (angle > maxTurnAngle && turnRight)
        {
            stopTurning = true;
        }
        else if (angle < -maxTurnAngle && !turnRight)
        {
            stopTurning = true;
        }

        if (turnRight)
        {
            enemy.thirdPersonCharacter.TurnCharacter(turnSpeed, turnSpeed);
        }
        else
        {
            enemy.thirdPersonCharacter.TurnCharacter(-turnSpeed, turnSpeed);
        }
    }

    /// <summary>
    /// Stunned
    /// </summary>
    
    public void StartStun()
    {
        isStunned = true;
        stunTimer = maxStunTimer;

        if (!enemy.nav.isStopped || enemy.nav.hasPath)
        {
            enemy.thirdPersonCharacter.Move(Vector3.zero, false, false);
            enemy.nav.SetDestination(transform.position);
            enemy.nav.isStopped = true;
        }

        enemy.anim.SetBool("IsStunned", true);

        isAttacking = false;
        shouldRest = false;
        hasRested = false;
    }

    public void Stunned()
    {

    }

    /// <summary>
    /// Juice
    /// </summary>

    private void PlayJuice(Vector3 position)
    {
        GameObject spawnedParticle = ObjectPooler.GetPooler(explosionParticleKey).GetPooledObject();
        spawnedParticle.transform.position = position;
        spawnedParticle.transform.rotation = transform.rotation;
        spawnedParticle.SetActive(true);

        DisableAfterTime(spawnedParticle, 1);
    }

    void DisableAfterTime(GameObject objectToDisable, float time = 0)
    {
        StartCoroutine(DisableEnum(time, objectToDisable));
    }

    IEnumerator DisableEnum(float disableTime, GameObject objectToDisable)
    {
        yield return new WaitForSeconds(disableTime);

        objectToDisable.SetActive(false);
    }

    /// <summary>
    /// Update
    /// </summary>

    private void Update()
    {
        if (!Player.Instance.IsAlive())
        {
            return;
        }

        if (!enemy.IsAlive())
        {
            if (enemy.nav.enabled)
            {
                if (!enemy.nav.isStopped || enemy.nav.hasPath)
                {
                    enemy.thirdPersonCharacter.Move(Vector3.zero, false, false);
                    enemy.nav.isStopped = true;
                }
                return;
            }

            return;
        }

        //Update nav speed.
        if (enemy.nav.speed != currentSpeed)
        {
            enemy.nav.speed = currentSpeed;
        }

        if (stopTurning)
        {

            searchTimer -= Time.deltaTime;
            if (searchTimer <= 0)
            {
                turnRight = !turnRight;
                searchTimer = maxSearchTimer;
                stopTurning = false;
            }
        }

        //Checks if enemy is resting.
        if (shouldRest)
        {
            TurnCharacter();
            if (stopTurning)
            {
                enemy.thirdPersonCharacter.Move(Vector3.zero, false, false);
            }
            
            restTimer -= Time.deltaTime;
            if (restTimer <= 0)
            {
                shouldRest = false;
                hasRested = true;
            }
        }

        //Charges up an attack and releases it once the timer hits zero.
        if (isAttacking)
        {
            enemy.thirdPersonCharacter.Move(Vector3.zero, false, false);

            attackTimer -= Time.deltaTime;
            if (attackTimer <= 0)
            {
                Debug.Log("Attack!");

                Vector3 playerPos = Player.Instance.transform.position;
                Vector3 dirToPlayer = (playerPos - transform.position).normalized;
                float currentDistance = Vector3.Distance(playerPos, transform.position);
                LayerMask invertedEnemyMask = ~enemy.enemyMask;
                if (currentDistance < outerAttackRadius && enemy.enemyFOV.IsTargetInFOV(dirToPlayer))
                {
                    RaycastHit hitInfo;
                    if (Physics.Raycast(transform.position, dirToPlayer, out hitInfo, currentDistance, invertedEnemyMask))
                    {
                        enemy.anim.SetTrigger("Hit");

                        if (hitInfo.collider == Player.Instance.primaryCollider)
                        {
                            Player.Instance.Explode(1000, transform.position, outerAttackRadius);
                            shouldAct = false;
                        }

                        PlayJuice(hitInfo.point);
                    }
                }

                isAttacking = false;
                attackTimer = maxAttackTimer;
                enemy.anim.SetBool("IsAttacking", false);
            }
        }

        if (isStunned)
        {
            stunTimer -= Time.deltaTime;
            if (stunTimer <= 0)
            {
                isStunned = false;
                stunTimer = maxStunTimer;

                enemy.anim.SetBool("IsStunned", false);
                enemy.enemyStateMachine.switchState(EnemyStateMachine.StateType.LostPlayer);

                enemy.nav.isStopped = false;
            }
        }
    }
}
