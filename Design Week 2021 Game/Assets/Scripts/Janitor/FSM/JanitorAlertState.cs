using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class JanitorAlertState : JanitorBaseState
{
    private float currentWait = 0.0f;
    private FMOD.Studio.EventInstance HeartBeatInstance;
    public override void Init(GameObject _owner, FSM _fsm)
    {
        base.Init(_owner, _fsm);
    }

    public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        currentWait = 0.0f;
        controller.state = JanitorController.States.Alert;
        FMODUnity.RuntimeManager.PlayOneShot("event:/Janitor/Alert");

        HeartBeatInstance = FMODUnity.RuntimeManager.CreateInstance("event:/Janitor/HeartBeats");
        HeartBeatInstance.start();
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
            FMODUnity.RuntimeManager.PlayOneShot("event:/Janitor/GiveUp");
        }

        HeartBeatInstance.getPlaybackState(out FMOD.Studio.PLAYBACK_STATE playbackstate);
        if (playbackstate == FMOD.Studio.PLAYBACK_STATE.STOPPED || playbackstate == FMOD.Studio.PLAYBACK_STATE.STOPPING)
            HeartBeatInstance.start();

    }

    public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        HeartBeatInstance.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
    }
}
