using Framework;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class JanitorChaseState : JanitorBaseState
{
    private FMOD.Studio.EventInstance HeartBeatFastInstance;

    public override void Init(GameObject _owner, FSM _fsm)
    {
        base.Init(_owner, _fsm);
    }

    public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        controller.state = JanitorController.States.Chase;
        controller.legAnimator.SetBool("Chase", true);

        FMODUnity.RuntimeManager.PlayOneShot("event:/Janitor/Chasing");
        HeartBeatFastInstance = FMODUnity.RuntimeManager.CreateInstance("event:/Janitor/HeartBeatsFast");
        HeartBeatFastInstance.start();
    }

    public override void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        int direction = (controller.player.transform.position.x - controller.transform.position.x) > 0 ? 1 : -1;
        // within the catch range
        if ( Mathf.Abs(controller.player.transform.position.x - controller.transform.position.x) < controller.catchDistance)
        {
            if (!controller.player.GetComponent<PlayerController>().Hide)
            {
                // trigger caught
                EventCenter.I().Triggered("GetCaught");
                HeartBeatFastInstance.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
                controller.legAnimator.SetBool("Stand", true);
            }
            else
            {
                fsm.ChangeState(fsm.PatrolState);
                controller.legAnimator.SetBool("Chase", false);
                FMODUnity.RuntimeManager.PlayOneShot("event:/Janitor/GiveUp");
                controller.legAnimator.SetBool("Stand", false);
            }
            
        }
        else
        {
            if (direction != controller.direction)
            {
                controller.direction = direction;
                controller.FlipCharacter();
            }
            controller.transform.position += new Vector3(direction * Time.deltaTime * controller.chaseSpeed, 0, 0);

            HeartBeatFastInstance.getPlaybackState(out FMOD.Studio.PLAYBACK_STATE playbackstate);
            if (playbackstate == FMOD.Studio.PLAYBACK_STATE.STOPPED || playbackstate == FMOD.Studio.PLAYBACK_STATE.STOPPING)
                HeartBeatFastInstance.start();
        }
    }

    public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        HeartBeatFastInstance.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
    }
    private void OnDestroy()
    {
        HeartBeatFastInstance.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
    }
}
