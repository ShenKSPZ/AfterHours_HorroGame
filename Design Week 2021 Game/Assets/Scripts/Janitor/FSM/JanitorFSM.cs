using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(JanitorController))]
public class JanitorFSM : FSM
{
    public readonly int IdleState = Animator.StringToHash("Idle");
    public readonly int PatrolState = Animator.StringToHash("Patrol");
    public readonly int AlertState = Animator.StringToHash("Alert");
    public readonly int ChaseState = Animator.StringToHash("Chase");
}