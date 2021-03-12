using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FMODUnity;
using UnityEngine.Events;

public class JanitorController : MonoBehaviour
{
    public float patrolSpeed = 2.0f;
    public float chaseSpeed = 4.0f;
    public float catchDistance = 4.0f;
    public float patrolMinPositionX = 0.0f;
    public float patrolMaxPositionX = 10.0f;
    public float checkingTime = 3.0f;
    public float alertDelayTime = 2.0f;

    public GameObject player;
    public GameObject leg;
    public GameObject hand;
    [HideInInspector] public int direction = 1;
    [HideInInspector] public Animator legAnimator;
    [HideInInspector] public Animator handAnimator;

    public enum States
    {
        Idle = 0,
        Patrol,
        Alert,
        Chase
    }

    public States state = States.Idle;

    [HideInInspector] public UnityEvent alertRaiseEvent = new UnityEvent();

    private void Awake()
    {
        legAnimator = leg.GetComponent<Animator>();
        Debug.Assert(legAnimator != null, $"{leg.name} requires an Animator Compoent");
        handAnimator = hand.GetComponent<Animator>();
        Debug.Assert(handAnimator != null, $"{hand.name} requires an Animator Compoent");
    }

    void Start()
    {
        gameObject.transform.GetChild(2).GetComponent<Animator>().Play(this.state.ToString());
    }

    private void Update()
    {
        if (state == States.Patrol && (Mathf.Abs(player.transform.position.x - gameObject.transform.position.x) < catchDistance) && !player.GetComponent<PlayerController>().Hide)
        {
            alertRaiseEvent.Invoke();
        }
    }

    public void FlipCharacter()
    {
        Vector3 scale = gameObject.transform.localScale;
        scale.x = direction;
        gameObject.transform.localScale = scale;
    }

}
