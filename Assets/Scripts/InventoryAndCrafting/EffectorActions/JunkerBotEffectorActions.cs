using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class JunkerBotEffectorActions : EffectorActions
{
    public JunkerBot junker;

    public override void PowerEffectorAction()
    {
        if (junker.stateMachine.GetCurrentState() != JunkerStateMachine.StateType.Disabled)
        {
            junker.stateMachine.switchState(JunkerStateMachine.StateType.Disabled);
        }
        else
        {
            junker.ResetDisabledTimer();
        }
    }

    public override void ExplosiveAction(float explosionForce, Vector3 explosionPosition, float explosionRadius)
    {
        if (junker.stateMachine.GetCurrentState() != JunkerStateMachine.StateType.Disabled)
        {
            junker.stateMachine.switchState(JunkerStateMachine.StateType.Disabled);
        }
        else
        {
            junker.ResetDisabledTimer();
        }

        junker.primaryRigidbody.AddExplosionForce(explosionForce, explosionPosition, explosionRadius);
    }
}
