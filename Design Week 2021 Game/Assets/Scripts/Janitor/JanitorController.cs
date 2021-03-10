using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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
    [HideInInspector]
    public SpriteRenderer spriteRenderer;

    // Start is called before the first frame update
    void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();

    }
	
}
