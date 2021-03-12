using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FMODUnity;

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

    private void Awake()
    {
        legAnimator = leg.GetComponent<Animator>();
        Debug.Assert(legAnimator != null, $"{leg.name} requires an Animator Compoent");
        handAnimator = hand.GetComponent<Animator>();
        Debug.Assert(handAnimator != null, $"{hand.name} requires an Animator Compoent");
    }

    private void Update()
    {
        Vector3 scale = gameObject.transform.localScale;
        scale.x = direction;
        gameObject.transform.localScale = scale;
    }

    // Start is called before the first frame update
    void Start()
    {
        

    }
	
}
