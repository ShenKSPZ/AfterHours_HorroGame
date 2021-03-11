using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class JanitorPatrolState : JanitorBaseState
{
    [SerializeField]
    private int direction = 1;

    public override void Init(GameObject _owner, FSM _fsm)
    {
        base.Init(_owner, _fsm);

        // some animation action listener can be added here
    }

    public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        controller.spriteRenderer.color = Color.green;

        GameplayController.I().alertRaiseEvent.AddListener(OnAlertRaised);
    }

    public override void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        controller.transform.position += new Vector3(Time.deltaTime * controller.patrolSpeed * direction, 0 , 0);
        if (controller.transform.position.x < controller.patrolMinPositionX )
        {
            direction = 1;
        }
        if (controller.transform.position.x > controller.patrolMaxPositionX)
        {
            direction = -1;
        }

    }

    public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        GameplayController.I().alertRaiseEvent.RemoveListener(OnAlertRaised);
    }

    private void OnAlertRaised()
    {
        fsm.ChangeState(fsm.AlertState);
    }
}
