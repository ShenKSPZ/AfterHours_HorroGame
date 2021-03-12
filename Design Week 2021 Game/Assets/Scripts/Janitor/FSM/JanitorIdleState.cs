using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class JanitorIdleState : JanitorBaseState
{
    [SerializeField]
    private float statePrepare = 0.5f;

    private float currentWait = 0.0f;

    public override void Init(GameObject _owner, FSM _fsm)
    {
        base.Init(_owner, _fsm);
    }

    public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        controller.state = JanitorController.States.Idle;
        currentWait = 0.0f;
    }

    public override void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        currentWait += Time.deltaTime;
        if (currentWait > statePrepare)
        {
            fsm.ChangeState(fsm.PatrolState);
        }
    }

    public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
    }
}
