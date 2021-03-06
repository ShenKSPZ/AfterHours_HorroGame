using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class JanitorPatrolState : JanitorBaseState
{
    public override void Init(GameObject _owner, FSM _fsm)
    {
        base.Init(_owner, _fsm);
    }

    public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        controller.state = JanitorController.States.Patrol;
        controller.alertRaiseEvent.AddListener(OnAlertRaised);
    }

    public override void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        controller.transform.position += new Vector3(Time.deltaTime * controller.patrolSpeed * controller.direction, 0 , 0);
        if (controller.transform.position.x < controller.patrolMinPositionX )
        {
            controller.direction = 1;
            controller.FlipCharacter();
        }
        if (controller.transform.position.x > controller.patrolMaxPositionX)
        {
            controller.direction = -1;
            controller.FlipCharacter();
        }

    }

    public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        controller.alertRaiseEvent.RemoveListener(OnAlertRaised);
    }

    private void OnAlertRaised()
    {
        fsm.ChangeState(fsm.AlertState);
    }
}
