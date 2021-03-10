using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class JanitorChaseState : JanitorBaseState
{
    public override void Init(GameObject _owner, FSM _fsm)
    {
        base.Init(_owner, _fsm);

        // some animation action listener can be added here
    }

    public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        controller.spriteRenderer.color = Color.red;
    }

    public override void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        int direction = (controller.player.transform.position.x - controller.transform.position.x) > 0 ? 1 : -1;
        // within the catch range
        if (Vector3.Distance(controller.transform.position, controller.player.transform.position) < controller.catchDistance)
        {
            Debug.Log("I got you!");
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
