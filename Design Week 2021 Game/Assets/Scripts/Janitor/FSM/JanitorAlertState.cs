using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FMODUnity;

public class JanitorAlertState : JanitorBaseState
{
    private float currentWait = 0.0f;
    public override void Init(GameObject _owner, FSM _fsm)
    {
        base.Init(_owner, _fsm);
    }

    public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        controller.state = JanitorController.States.Alert;
        currentWait = 0.0f;
        RuntimeManager.PlayOneShot("event:/Janitor/Alert", controller.gameObject.transform.position);
    }

    public override void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        currentWait += Time.deltaTime;
        if ((currentWait > controller.alertDelayTime) && !controller.player.GetComponent<PlayerController>().Hide)
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
