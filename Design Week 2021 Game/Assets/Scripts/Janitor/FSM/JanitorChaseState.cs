using Framework;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class JanitorChaseState : JanitorBaseState
{
    public override void Init(GameObject _owner, FSM _fsm)
    {
        base.Init(_owner, _fsm);
    }

    public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        controller.legAnimator.SetBool("Chase", true);
    }

    public override void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        int direction = (controller.player.transform.position.x - controller.transform.position.x) > 0 ? 1 : -1;
        // within the catch range
        if (Vector3.Distance(controller.transform.position, controller.player.transform.position) < controller.catchDistance)
        {
            if (!controller.player.GetComponent<PlayerController>().Hide)
            {
                EventCenter.I().Triggered("GetCaught");
            }
            else
            {
                fsm.ChangeState(fsm.PatrolState);
                controller.legAnimator.SetBool("Chase", false);
            }
            
        }
        else
        {
            controller.transform.position += new Vector3(direction * Time.deltaTime * controller.chaseSpeed, 0, 0);
        }
    }

    public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
    }
}
