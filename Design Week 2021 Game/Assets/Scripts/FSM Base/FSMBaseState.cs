using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class FSMBaseState<T> : InternalFSMBaseState where T : FSM
{
    protected T fsm { get; private set; }
    protected GameObject owner { get; private set; }

    public override void Init(GameObject _owner, FSM _fsm)
    {
        owner = _owner;
        fsm = (T)_fsm;
    }
}
