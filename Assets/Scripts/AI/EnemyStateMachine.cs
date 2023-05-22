using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyStateMachine : MonoBehaviour
{
    private Enemy enemy;

    public enum StateType
    {
        Patrol,
        Attack,
        Chase,
        LostPlayer,
        Stunned
    }

    public StateType state = StateType.Patrol;

    private void Awake()
    {
        enemy = GetComponent<Enemy>();
    }

    void Start()
    {
        OnStateEnter(state);
    }

    void Update()
    {
        if (Player.Instance == null || !Player.Instance.IsAlive())
        {
            return;
        }

        if (!enemy.IsAlive())
        {
            return;
        }

        if (!enemy.enemyBehavior.shouldAct)
        {
            return;
        }

        switch (state)
        {
            case StateType.Patrol:
                enemy.enemyBehavior.Patrol();
                    
                enemy.enemyFOV.FindPlayer();
                break;
            case StateType.Attack:
                enemy.enemyBehavior.Attack();

                break;
            case StateType.Chase:
                enemy.enemyBehavior.Chase();

                break;
            case StateType.LostPlayer:
                enemy.enemyBehavior.LostPlayer();

                enemy.enemyFOV.FindPlayer();
                break;
            case StateType.Stunned:
                //enemy.enemyBehavior.Stunned();

                break;
        }
    }

    public StateType GetCurrentState()
    {
        return state;
    }

    public void switchState(StateType newState)
    {
        if (state == newState)
        {
            return;
        }

        if (!Player.Instance.IsAlive() || !enemy.IsAlive())
        {
            return;
        }

        if (!enemy.enemyBehavior.shouldAct)
        {
            return;
        }

        StateType previousState = state;
        state = newState;
        OnStateEnter(previousState);
    }

    private void OnStateEnter(StateType previousState)
    {
        switch (state)
        {
            case StateType.Patrol:
                enemy.enemyBehavior.StartPatrolling();

                break;
            case StateType.Attack:
                enemy.enemyBehavior.StartAttack();

                break;
            case StateType.Chase:
                enemy.enemyBehavior.SetLastKnowPosition(Player.Instance.transform.position);

                if (!enemy.enemyFOV.FindPlayer()) 
                {
                    switchState(StateType.LostPlayer);
                }

                break;
            case StateType.LostPlayer:
                if (previousState != StateType.Stunned)
                {
                    enemy.enemyBehavior.SetLastKnowPosition(Player.Instance.transform.position);
                }

                break;

            case StateType.Stunned:
                enemy.enemyBehavior.StartStun();

                break;
        }
    }
}
