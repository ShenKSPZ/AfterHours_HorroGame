using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class FSM : MonoBehaviour
{
    public RuntimeAnimatorController FSMController;

    public Animator fsmAnimator { get; private set; }

    private readonly int AnyLayer = -1;

    private void Awake()
    {
        // Create a FSM GameObject and set this as a child to the gameObject
        GameObject FSMGO = new GameObject("FSM", typeof(Animator));
        FSMGO.transform.parent = transform;

        // Get the animator from the FSMGO
        fsmAnimator = FSMGO.GetComponent<Animator>();
        fsmAnimator.runtimeAnimatorController = FSMController;

        // set the owner
        InternalFSMBaseState[] behaviors = fsmAnimator.GetBehaviours<InternalFSMBaseState>();
        foreach (var behavior in behaviors)
        {
            behavior.Init(gameObject, this);
        }
    }

    public bool ChangeState(string stateName)
    {
        return ChangeState(Animator.StringToHash(stateName));
    }

    public bool ChangeState(int hashStateName)
    {
        bool hasState = true;

#if UNITY_EDITOR
        hasState = fsmAnimator.HasState(AnyLayer, hashStateName);
#endif
        fsmAnimator.CrossFade(hashStateName, 0.0f, AnyLayer);

        return hasState;
    }
}
