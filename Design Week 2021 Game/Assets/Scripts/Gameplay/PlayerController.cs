using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum PlayerState
{
    Free,
    Jumping
}

public class PlayerController : MonoBehaviour
{
    #region Settings
    [Header("Movement")]
    public float MovingSpeed = 9;
    public float AccelerateSpeed = 0.09f;
    public float DecelerateSpeed = 0.05f;

    [Header("Jump")]
    public float JumpingSpeed = 5f;
    public float FallMultiplier = 2.5f;
    public float LowJumpMultiplier = 2f;
    public float LeftJumpTime = 0.2f;

    [Header("Grab")]
    public Vector2 GrabSize;
    public Vector2 GrabOffset;
    public LayerMask InteractableLayer;
    public float GrabForce = 500f;

    [Header("GroundDetect")]
    public bool OnGround;
    public Vector2 Size;
    public Vector2 Offset;
    public LayerMask GroundLayer;
    #endregion

    #region Reference
    SpriteRenderer SR;
    Rigidbody2D Rig;
    #endregion

    #region Runtime
    float DampVelocity1 = 0;

    float LeftGrounded = 0f;

    public GameObject ObjectGrabed = null;
    Vector2 GrabedDistance = Vector2.zero;
    #endregion

    // Start is called before the first frame update
    void Awake()
    {
        Rig = GetComponent<Rigidbody2D>();
        SR = GetComponent<SpriteRenderer>();
    }

    private void Update()
    {
        StateCheck();

        #region InputCheck
        if (Input.GetButtonDown("Jump") && OnGround && ObjectGrabed == null)
        {
            Rig.velocity = new Vector2(Rig.velocity.x, JumpingSpeed);
        }

        if (Input.GetButton("Grab"))
        {
            Collider2D coll = Physics2D.OverlapBox((Vector2)transform.position + GrabOffset * (SR.flipX ? -1 : 1), GrabSize, 0f, InteractableLayer);
            ObjectGrabed = coll != null ? coll.gameObject : null;
        }
        else if (Input.GetButtonUp("Grab"))
        {
            ObjectGrabed = null;
        }
        #endregion
    }

    void StateCheck()
    {
        if (IsOnGround())
        {
            if (LeftGrounded > LeftJumpTime)
                OnGround = true;
            else
                LeftGrounded += Time.deltaTime;
        }

        if (Rig.velocity.y < 0)
        {
            Rig.velocity += Vector2.up * Physics2D.gravity.y * (FallMultiplier - 1) * Time.fixedDeltaTime;
        }
        else if (Rig.velocity.y > 0 && Input.GetAxis("Jump") != 1)
        {
            Rig.velocity += Vector2.up * Physics2D.gravity.y * (LowJumpMultiplier - 1) * Time.fixedDeltaTime;
        }
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        if (Input.GetAxisRaw("Horizontal") > 0)
        {
            if(ObjectGrabed != null)
            {
                Rigidbody2D OtherRig = ObjectGrabed.GetComponent<Rigidbody2D>();
                OtherRig.velocity = new Vector2(Mathf.SmoothDamp(Rig.velocity.x, MovingSpeed * (50 - OtherRig.mass <= 1 ? 1 : 50 - OtherRig.mass) * Time.fixedDeltaTime, ref DampVelocity1, AccelerateSpeed), Rig.velocity.y);
                if (SR.flipX)
                    Rig.velocity = OtherRig.velocity * 0.5f;
                else
                    Rig.velocity = OtherRig.velocity;
            }
            else
            {
                Rig.velocity = new Vector2(Mathf.SmoothDamp(Rig.velocity.x, MovingSpeed * 50 * Time.fixedDeltaTime, ref DampVelocity1, AccelerateSpeed), Rig.velocity.y);
                SR.flipX = false;
            }
        }
        else if (Input.GetAxisRaw("Horizontal") < 0)
        {
            if (ObjectGrabed != null)
            {
                Rigidbody2D OtherRig = ObjectGrabed.GetComponent<Rigidbody2D>();
                OtherRig.velocity = new Vector2(Mathf.SmoothDamp(Rig.velocity.x, MovingSpeed * (-50 + OtherRig.mass >= -1 ? -1 : -50 + OtherRig.mass) * Time.fixedDeltaTime, ref DampVelocity1, AccelerateSpeed), Rig.velocity.y);
                if (!SR.flipX)
                    Rig.velocity = OtherRig.velocity * 0.5f;
                else
                    Rig.velocity = OtherRig.velocity;
            }
            else
            {
                Rig.velocity = new Vector2(Mathf.SmoothDamp(Rig.velocity.x, MovingSpeed * -50 * Time.fixedDeltaTime, ref DampVelocity1, AccelerateSpeed), Rig.velocity.y);
                SR.flipX = true;
            }
        }
        else
        {
            if (ObjectGrabed != null)
            {
                Rigidbody2D OtherRig = ObjectGrabed.GetComponent<Rigidbody2D>();
                OtherRig.velocity = new Vector2(Mathf.SmoothDamp(Rig.velocity.x, 0, ref DampVelocity1, DecelerateSpeed), Rig.velocity.y);
                Rig.velocity = OtherRig.velocity;
            }
            else
            {
                Rig.velocity = new Vector2(Mathf.SmoothDamp(Rig.velocity.x, 0, ref DampVelocity1, DecelerateSpeed), Rig.velocity.y);
            }
        }
    }

    bool IsOnGround()
    {
        return Physics2D.OverlapBox((Vector2)transform.position + Offset, Size, 0f, GroundLayer);
    }

    void OnDrawGizmos()
    {
        Gizmos.color = Color.red;

        Gizmos.DrawWireCube((Vector2)transform.position + Offset, Size);

        if(SR != null)
            Gizmos.DrawWireCube((Vector2)transform.position + GrabOffset * (SR.flipX ? -1 : 1), GrabSize);
    }
}
