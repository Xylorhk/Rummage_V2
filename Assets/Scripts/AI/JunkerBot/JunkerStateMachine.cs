using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class JunkerStateMachine : MonoBehaviour
{
    private JunkerBot junker;

    public enum StateType
    {
        Patrol,
        Act,
        Chase,
        Disabled
    }

    public StateType state = StateType.Patrol;

    private void Awake()
    {
        junker = GetComponent<JunkerBot>();
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

        if (!junker.IsAlive())
        {
            return;
        }

        if (!junker.behavior.shouldAct)
        {
            return;
        }

        switch (state)
        {
            case StateType.Patrol:
                junker.behavior.Patrol();

                if (junker.shouldScoop)
                {
                    junker.junkerFOV.FindPlayer();
                }
                break;
            case StateType.Act:

                break;
            case StateType.Chase:
                junker.behavior.Chase();

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

        if ((!Player.Instance.IsAlive() && newState != StateType.Patrol) || !junker.IsAlive())
        {
            return;
        }

        if (!junker.behavior.shouldAct)
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
                junker.nav.speed = junker.behavior.patrolSpeed;
                junker.behavior.StartPatrolling();

                junker.ChangeEmissionColor(junker.neutralEyeColor);

                break;
            case StateType.Act:
                junker.behavior.PerformAction();

                break;
            case StateType.Chase:
                junker.anim.SetBool("IsWalking", true);
                junker.nav.speed = junker.behavior.chaseSpeed;
                junker.behavior.SetLastKnowPosition(Player.Instance.transform.position);

                junker.ChangeEmissionColor(junker.aggresiveEyeColor);

                if (!junker.junkerFOV.FindPlayer())
                {
                    switchState(StateType.Patrol);
                }
                break;
            case StateType.Disabled:
                junker.behavior.Disable();

                junker.ChangeEmissionColor(junker.disabledEyeColor);
                break;
        }
    }
}
