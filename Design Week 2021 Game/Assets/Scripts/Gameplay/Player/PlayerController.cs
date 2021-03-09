using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FunctionExtend;

public enum PlayerState
{
    Idle,
    Moving,
    Grabbing,
}

public class PlayerController : MonoBehaviour
{
    #region Settings
    public PlayerState State = PlayerState.Idle;

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
    public Vector2 GroundSize = Vector2.zero;
    public Vector2 GroundOffset = Vector2.zero;
    public float MaxSlope = 50;
    public float RayLength = 1f;
    public LayerMask GroundLayer;
    #endregion

    #region Reference
    SpriteRenderer SR;
    Rigidbody2D Rig;
    BoxCollider2D Box;
    #endregion

    #region Runtime
    //Velocity
    Vector2 MovingDirection = Vector2.zero;
    float DampVelocity1 = 0;

    //About Ground
    public bool OnGround = false;
    public bool ActualOnGround = false;
    float LeftGrounded = 0f;
    float OnGroundRadius = 0;

    //Jumping
    bool IsJumped = false;
    bool LeaveGround = false;

    //Grabbing
    GameObject ObjectGrabed = null;
    #endregion

    // Start is called before the first frame update
    void Awake()
    {
        Rig = GetComponent<Rigidbody2D>();
        SR = GetComponent<SpriteRenderer>();
        Box = GetComponent<BoxCollider2D>();
    }

    private void Update()
    {
        StateCheck();

        #region Moving
        if (Input.GetAxisRaw("Horizontal") > 0)
        {
            if (ObjectGrabed != null)
            {
                Rigidbody2D OtherRig = ObjectGrabed.GetComponent<Rigidbody2D>();
                OtherRig.velocity = new Vector2(Mathf.SmoothDamp(OtherRig.velocity.x, MovingSpeed * (50 - OtherRig.mass <= 1 ? 1 : 50 - OtherRig.mass) * Time.fixedDeltaTime, ref DampVelocity1, AccelerateSpeed), OtherRig.velocity.y);
                if (SR.flipX)
                    MovingDirection = new Vector2(OtherRig.velocity.x * 0.5f, MovingDirection.y);
                else
                    MovingDirection = new Vector2(OtherRig.velocity.x, MovingDirection.y);
            }
            else
            {
                MovingDirection = new Vector2(Mathf.SmoothDamp(MovingDirection.x, MovingSpeed * 50 * Time.fixedDeltaTime, ref DampVelocity1, AccelerateSpeed), MovingDirection.y);
                SR.flipX = false;
            }

            if (ActualOnGround && !IsJumped)
            {
                Rig.gravityScale = 0;
                Rig.velocity = ClampSlop(MovingDirection);
            }
            else
            {
                Rig.velocity = new Vector2(MovingDirection.x, Rig.velocity.y);
            }
        }
        else if (Input.GetAxisRaw("Horizontal") < 0)
        {
            if (ObjectGrabed != null)
            {
                Rigidbody2D OtherRig = ObjectGrabed.GetComponent<Rigidbody2D>();
                OtherRig.velocity = new Vector2(Mathf.SmoothDamp(OtherRig.velocity.x, MovingSpeed * (-50 + OtherRig.mass >= -1 ? -1 : -50 + OtherRig.mass) * Time.fixedDeltaTime, ref DampVelocity1, AccelerateSpeed), OtherRig.velocity.y);
                if (!SR.flipX)
                    MovingDirection = new Vector2(OtherRig.velocity.x * 0.5f, MovingDirection.y);
                else
                    MovingDirection = new Vector2(OtherRig.velocity.x, MovingDirection.y);
            }
            else
            {
                MovingDirection = new Vector2(Mathf.SmoothDamp(MovingDirection.x, MovingSpeed * -50 * Time.fixedDeltaTime, ref DampVelocity1, AccelerateSpeed), MovingDirection.y);
                SR.flipX = true;
            }

            if (ActualOnGround && !IsJumped)
            {
                Rig.gravityScale = 0;
                Rig.velocity = ClampSlop(MovingDirection);
            }
            else
            {
                Rig.velocity = new Vector2(MovingDirection.x, Rig.velocity.y);
            }
        }
        else
        {
            if (ObjectGrabed != null)
            {
                Rigidbody2D OtherRig = ObjectGrabed.GetComponent<Rigidbody2D>();
                OtherRig.velocity = new Vector2(Mathf.SmoothDamp(OtherRig.velocity.x, 0, ref DampVelocity1, DecelerateSpeed), OtherRig.velocity.y);
                MovingDirection = new Vector2(OtherRig.velocity.x, MovingDirection.y);
            }
            else
            {
                MovingDirection = new Vector2(Mathf.SmoothDamp(MovingDirection.x, 0, ref DampVelocity1, DecelerateSpeed), MovingDirection.y);
            }

            if (ActualOnGround && !IsJumped)
            {
                Rig.gravityScale = 0;
                Rig.velocity = ClampSlop(MovingDirection);
            }
            else
            {
                Rig.velocity = new Vector2(MovingDirection.x, Rig.velocity.y);
            }
        }
        #endregion

        #region Jumping
        if (Input.GetButtonDown("Jump") && OnGround && ObjectGrabed == null)
        {
            IsJumped = true;
            LeaveGround = false;
            Rig.velocity = new Vector2(Rig.velocity.x, JumpingSpeed);
        }
        #endregion

        #region Grabbing
        if (Input.GetButton("Grab"))
        {
            Collider2D coll = Physics2D.OverlapBox((Vector2)transform.position + new Vector2(GrabOffset.x * (SR.flipX ? -1 : 1), GrabOffset.y), GrabSize, 0f, InteractableLayer);
            ObjectGrabed = coll != null ? coll.gameObject : null;
        }
        else if (Input.GetButtonUp("Grab"))
        {
            ObjectGrabed = null;
        }
        #endregion

        Debug.Log(MovingDirection);
    }

    void StateCheck()
    {
        if (IsOnGround())
        {
            LeftGrounded = 0;
            OnGround = true;
            ActualOnGround = true;
            if (LeaveGround)
                IsJumped = false;
            //Debug.Log(OnGroundRadius);
        }
        else
        {
            if (LeftGrounded > LeftJumpTime)
                OnGround = false;
            else
                LeftGrounded += Time.fixedDeltaTime;

            ActualOnGround = false;
            LeaveGround = true;
            Rig.gravityScale = 1;
            if (Rig.velocity.y < 0)
            {
                Rig.velocity += Vector2.up * Physics2D.gravity.y * (FallMultiplier - 1) * Time.fixedDeltaTime;
            }
            else if (Rig.velocity.y > 0 && Input.GetAxis("Jump") != 1)
            {
                Rig.velocity += Vector2.up * Physics2D.gravity.y * (LowJumpMultiplier - 1) * Time.fixedDeltaTime;
            }
        }
    }

    Vector2 ClampSlop(Vector2 velocity)
    {
        float moveDistance = velocity.x;
        velocity.y = Mathf.Sin(OnGroundRadius) * moveDistance;
        velocity.x = Mathf.Cos(OnGroundRadius) * moveDistance;
        return velocity;
    }

    bool IsOnGround()
    {
        Vector2 LeftPos = new Vector2(transform.position.x - (Box.size.x / 2), transform.position.y - (Box.size.y / 2));
        Vector2 RightPos = new Vector2(transform.position.x + (Box.size.x / 2), transform.position.y - (Box.size.y / 2));
        RaycastHit2D[] hits = new RaycastHit2D[] {
            Physics2D.Raycast(LeftPos, Vector2.down, RayLength, GroundLayer | InteractableLayer),
            Physics2D.Raycast(LeftPos, Vector2.left, RayLength, GroundLayer | InteractableLayer),
            Physics2D.Raycast(RightPos, Vector2.down, RayLength, GroundLayer | InteractableLayer),
            Physics2D.Raycast(RightPos, Vector2.right, RayLength, GroundLayer | InteractableLayer),
            Physics2D.Raycast(transform.position - new Vector3(0, Box.size.y / 2,  0), Vector2.right, RayLength, GroundLayer | InteractableLayer),
        };

        bool Detected = false;
        for (int i = 0; i < hits.Length; i++)
        {
            if(hits[i].collider != null)
            {
                if (Mathf.Abs(hits[i].normal.SignedAngle()) <= 50)
                {
                    OnGroundRadius = hits[i].normal.SignedAngle() * Mathf.Deg2Rad;
                    Detected = true;
                    break;
                }
            }

            if(i == hits.Length - 1 && !Detected)
            {
                Collider2D coll = Physics2D.OverlapBox((Vector2)transform.position + GroundOffset, GroundSize, 0, GroundLayer | InteractableLayer);
                Detected = coll != null ? true : false;
            }
        }

        return Detected;
    }

    void GoState(PlayerState next)
    {
        State = next;
    }

#if UNITY_EDITOR
    void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        if (Box != null)
        {
            Vector2 LeftPos = new Vector2(transform.position.x - (Box.size.x / 2), transform.position.y - (Box.size.y / 2));
            Vector2 RightPos = new Vector2(transform.position.x + (Box.size.x / 2), transform.position.y - (Box.size.y / 2));

            Gizmos.DrawLine(LeftPos, LeftPos - new Vector2(RayLength, 0));
            Gizmos.DrawLine(LeftPos, LeftPos - new Vector2(0, RayLength));
            Gizmos.DrawLine(RightPos, RightPos + new Vector2(RayLength, 0));
            Gizmos.DrawLine(RightPos, RightPos - new Vector2(0, RayLength));
            Gizmos.DrawLine(transform.position - new Vector3(0, Box.size.y / 2, 0), transform.position - new Vector3(0, Box.size.y / 2, 0) - new Vector3(0, RayLength));
        }

        if(SR != null)
            Gizmos.DrawWireCube((Vector2)transform.position + new Vector2(GrabOffset.x * (SR.flipX ? -1 : 1), GrabOffset.y), GrabSize);

        Gizmos.DrawWireCube((Vector2)transform.position + GroundOffset, GroundSize);
    }
#endif

    //private void OnCollisionEnter2D(Collision2D collision)
    //{
    //    OnCollisionStay2D(collision);
    //}

    //private void OnCollisionStay2D(Collision2D collision)
    //{
    //    if (collision.gameObject.CompareTag("Ground"))
    //    {
    //        for (int i = 0; i < collision.contacts.Length; i++)
    //        {
    //            if(Mathf.Abs(collision.contacts[i].normal.SignedAngle()) < 45)
    //            {
    //                OnGroundNormal = collision.GetContact(i).normal;
    //                Debug.Log(OnGroundNormal);
    //                OnGround = true;
    //                break;
    //            }
    //        }
    //    }
    //}
    //private void OnCollisionExit2D(Collision2D collision)
    //{
    //    if (collision.gameObject.CompareTag("Ground"))
    //    {
    //        for (int i = 0; i < collision.contacts.Length; i++)
    //        {
    //            if (Mathf.Abs(collision.contacts[i].normal.SignedAngle()) < 45)
    //            {
    //                OnGround = false;
    //                break;
    //            }
    //        }
    //    }
    //}
}
