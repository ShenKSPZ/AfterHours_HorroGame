using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class JanitorAlertState : JanitorBaseState
{
    private float currentWait = 0.0f;
    private float currentCheckingTime = 0.0f;
    public override void Init(GameObject _owner, FSM _fsm)
    {
        base.Init(_owner, _fsm);

        // some animation action listener can be added here
    }

    public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        currentWait = 0.0f;
        currentCheckingTime = 0.0f;
        controller.spriteRenderer.color = Color.yellow;
    }

    public override void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        currentWait += Time.deltaTime;
        if ((currentWait > controller.alertDelayTime) && GameplayController.I().gameState == GameplayController.States.Normal)
        {
            fsm.ChangeState(fsm.ChaseState);
        }
        
        if (currentWait - controller.alertDelayTime > controller.checkingTime)
        {
            fsm.ChangeState(fsm.PatrolState);
        }
    }

    public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
    }
}
