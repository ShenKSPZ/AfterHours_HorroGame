using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class JanitorBaseState : FSMBaseState<JanitorFSM>
{
    protected JanitorController controller;

    public override void Init(GameObject _owner, FSM _fsm)
    {
        base.Init(_owner, _fsm);
        controller = owner.GetComponent<JanitorController>();
        Debug.Assert(controller != null, $"{owner.name} requires a JanitorController Component");
    }
}
