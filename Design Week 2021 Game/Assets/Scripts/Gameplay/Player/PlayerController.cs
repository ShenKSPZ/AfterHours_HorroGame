using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FunctionExtend;

public enum PlayerState
{
    Free,
    Climbing,
    Grabbing,
    StartJump,
    Jump,
    Landing,
}

public class PlayerController : MonoBehaviour
{
    #region Settings
    public PlayerState State = PlayerState.Free;

    public float DetectingRayLength = 1f;
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

    [Header("Climbing")]
    public Vector2 AfterClimbingOffset;

    [Header("GroundDetect")]
    public Vector2 GroundSize = Vector2.zero;
    public Vector2 GroundOffset = Vector2.zero;
    public float MaxSlope = 50;
    public LayerMask GroundLayer;
    #endregion

    #region Reference
    AnimatorManager Anim;
    SpriteRenderer SR;
    Rigidbody2D Rig;
    BoxCollider2D Box;
    #endregion

    #region Runtime
    //Velocity
    Vector2 MovingDirection = Vector2.zero;
    float DampVelocity1 = 0;

    //About Ground
    bool OnGround = false;
    bool ActualOnGround = false;
    float LeftGrounded = 0f;
    float OnGroundRadius = 0;

    //Jumping
    bool IsJumped = false;
    bool LeaveGround = false;

    //Grabbing
    GameObject ObjectGrabed = null;

    //Climbing
    public bool WaitForClimb = false;
    public bool CanClimb = false;
    #endregion

    // Start is called before the first frame update
    void Awake()
    {
        Rig = GetComponent<Rigidbody2D>();
        SR = GetComponent<SpriteRenderer>();
        Box = GetComponent<BoxCollider2D>();
        Anim = GetComponent<AnimatorManager>();
    }

    private void Update()
    {
        StateCheck();
        ObjectGrabed = null;

        switch (State)
        {
            case PlayerState.Free:
                Anim.Idle(false);
                if (CanClimb)
                {
                    if (Input.GetAxis("Jump") != 0 || (Input.GetAxisRaw("Horizontal") > 0 && !SR.flipX) || (Input.GetAxisRaw("Horizontal") < 0 && SR.flipX))
                    {
                        GoState(PlayerState.Climbing);
                    }
                }
                #region Grabbing
                if (Input.GetButton("Grab"))
                {
                    Collider2D coll = Physics2D.OverlapBox((Vector2)transform.position + new Vector2(Box.size.x / 2 * (SR.flipX ? -1 : 1) + GrabOffset.x * (SR.flipX ? -1 : 1), GrabOffset.y) + Box.offset, GrabSize * Box.size, 0f, InteractableLayer);
                    ObjectGrabed = coll != null ? coll.gameObject : null;
                    if (ObjectGrabed != null)
                        GoState(PlayerState.Grabbing);
                }
                #endregion

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
                        Rig.velocity = new Vector2(Mathf.SmoothDamp(Rig.velocity.x, MovingSpeed * 50 * Time.fixedDeltaTime, ref DampVelocity1, AccelerateSpeed), Rig.velocity.y);
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
                        Rig.velocity = new Vector2(Mathf.SmoothDamp(Rig.velocity.x, MovingSpeed * -50 * Time.fixedDeltaTime, ref DampVelocity1, AccelerateSpeed), Rig.velocity.y);
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
                        Rig.velocity = new Vector2(Mathf.SmoothDamp(Rig.velocity.x, 0, ref DampVelocity1, DecelerateSpeed), Rig.velocity.y);
                    }
                }
                #endregion

                #region Jumping
                if (Input.GetButtonDown("Jump") && OnGround)
                {
                    IsJumped = true;
                    LeaveGround = false;
                    Rig.velocity = new Vector2(Rig.velocity.x, JumpingSpeed);
                }
                #endregion
                break;
            case PlayerState.Climbing:
                MovingDirection = Vector2.zero;
                Rig.velocity = Vector2.zero;
                Rig.isKinematic = true;
                Anim.Climb();
                break;
            case PlayerState.Grabbing:
                #region Grabbing
                if (Input.GetButton("Grab"))
                {
                    Collider2D coll = Physics2D.OverlapBox((Vector2)transform.position + new Vector2(Box.size.x / 2 * (SR.flipX ? -1 : 1) + GrabOffset.x * (SR.flipX ? -1 : 1), GrabOffset.y) + Box.offset, GrabSize * Box.size, 0f, InteractableLayer);
                    ObjectGrabed = coll != null ? coll.gameObject : null;
                    if (ObjectGrabed == null)
                        GoState(PlayerState.Free);
                }
                else
                {
                    GoState(PlayerState.Free);
                }
                #endregion

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
                        Rig.velocity = new Vector2(Mathf.SmoothDamp(Rig.velocity.x, MovingSpeed * 50 * Time.fixedDeltaTime, ref DampVelocity1, AccelerateSpeed), Rig.velocity.y);
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
                        Rig.velocity = new Vector2(Mathf.SmoothDamp(Rig.velocity.x, MovingSpeed * -50 * Time.fixedDeltaTime, ref DampVelocity1, AccelerateSpeed), Rig.velocity.y);
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
                        Rig.velocity = new Vector2(Mathf.SmoothDamp(Rig.velocity.x, 0, ref DampVelocity1, DecelerateSpeed), Rig.velocity.y);
                    }
                }
                #endregion
                break;
            case PlayerState.StartJump:
                break;
            case PlayerState.Jump:
                break;
            case PlayerState.Landing:
                break;
            default:
                break;
        }
    }

    void StateCheck()
    {
        #region GroundCheck
        if (IsOnGround())
        {
            LeftGrounded = 0;
            OnGround = true;
            ActualOnGround = true;
            if (LeaveGround)
                IsJumped = false;
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
        }
        #endregion

        #region CanClimbCheck
        Collider2D BodyHit = Physics2D.OverlapBox((Vector2)transform.position + new Vector2(Box.size.x / 2 * (SR.flipX ? -1 : 1) + GrabOffset.x * (SR.flipX ? -1 : 1), GrabOffset.y) + Box.offset, GrabSize * Box.size, 0f, GroundLayer);
        RaycastHit2D HeadHit = Physics2D.Raycast(transform.position + new Vector3(Box.size.x / 2 * (SR.flipX ? -1 : 1), Box.size.y / 2, 0) + (Vector3)Box.offset, SR.flipX ? Vector2.left : Vector2.right, DetectingRayLength, GroundLayer);
        if (BodyHit != null && HeadHit.collider == null && State == PlayerState.Free)
        {
            WaitForClimb = true;
        }
        else if (HeadHit.collider != null && WaitForClimb && State == PlayerState.Free)
        {
            CanClimb = true;
            WaitForClimb = false;
        }
        else
        {
            CanClimb = false;
            WaitForClimb = false;
        }
        #endregion
    }

    private void FixedUpdate()
    {
        if (!ActualOnGround)
        {
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
        Vector2 LeftPos = new Vector2(transform.position.x - (Box.size.x / 2), transform.position.y - (Box.size.y / 2)) + Box.offset;
        Vector2 RightPos = new Vector2(transform.position.x + (Box.size.x / 2), transform.position.y - (Box.size.y / 2)) + Box.offset;
        RaycastHit2D[] hits = new RaycastHit2D[] {
            Physics2D.Raycast(LeftPos, Vector2.down, DetectingRayLength, GroundLayer),
            Physics2D.Raycast(LeftPos, Vector2.left, DetectingRayLength, GroundLayer),
            Physics2D.Raycast(RightPos, Vector2.down, DetectingRayLength, GroundLayer),
            Physics2D.Raycast(RightPos, Vector2.right, DetectingRayLength, GroundLayer),
            Physics2D.Raycast(transform.position - new Vector3(0, Box.size.y / 2,  0) + (Vector3)Box.offset, Vector2.right, DetectingRayLength, GroundLayer),
        };

        RaycastHit2D[] InnerHits = new RaycastHit2D[] {
            Physics2D.Raycast(LeftPos, Vector2.right, DetectingRayLength, GroundLayer),
            Physics2D.Raycast(RightPos, Vector2.left, DetectingRayLength, GroundLayer),
        };

        bool Detected = false;
        bool CollideSomething = false;
        for (int i = 0; i < hits.Length; i++)
        {
            if (hits[i].collider != null)
            {
                CollideSomething = true;
                if (Mathf.Abs(hits[i].normal.SignedAngle()) <= 50)
                {
                    OnGroundRadius = hits[i].normal.SignedAngle() * Mathf.Deg2Rad;
                    Detected = true;
                    break;
                }
            }

            if (i == hits.Length - 1 && !CollideSomething)
            {
                Collider2D coll = Physics2D.OverlapBox((Vector2)transform.position - new Vector2(0, Box.size.y / 2) + GroundOffset + Box.offset, GroundSize * Box.size, 0, GroundLayer);
                Detected = coll != null ? true : false;
            }
        }

        for (int i = 0; i < InnerHits.Length; i++)
        {
            if (InnerHits[i].collider != null)
            {
                Detected = false;
                break;
            }
        }
        return Detected;
    }

    void GoState(PlayerState next)
    {
        if (State == PlayerState.Climbing)
        {
            StartCoroutine(WaitForClimbingFinish(next));
        }
        else
        {
            State = next;
        }
    }

    IEnumerator WaitForClimbingFinish(PlayerState next)
    {
        yield return new WaitForEndOfFrame();
        transform.position += new Vector3(AfterClimbingOffset.x * (SR.flipX ? -1 : 1), AfterClimbingOffset.y);
        Rig.isKinematic = false;
        State = next;
    }

#if UNITY_EDITOR
    void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        if (Box != null)
        {
            Vector2 LeftPos = new Vector2(transform.position.x - (Box.size.x / 2), transform.position.y - (Box.size.y / 2)) + Box.offset;
            Vector2 RightPos = new Vector2(transform.position.x + (Box.size.x / 2), transform.position.y - (Box.size.y / 2)) + Box.offset;

            Gizmos.DrawLine(LeftPos, LeftPos - new Vector2(DetectingRayLength, 0));
            Gizmos.DrawLine(LeftPos, LeftPos - new Vector2(0, DetectingRayLength));
            Gizmos.DrawLine(RightPos, RightPos + new Vector2(DetectingRayLength, 0));
            Gizmos.DrawLine(RightPos, RightPos - new Vector2(0, DetectingRayLength));
            Gizmos.DrawLine(transform.position - new Vector3(0, Box.size.y / 2, 0) + (Vector3)Box.offset, transform.position - new Vector3(0, Box.size.y / 2, 0) + (Vector3)Box.offset - new Vector3(0, DetectingRayLength));

            Gizmos.DrawWireCube((Vector2)transform.position - new Vector2(0, Box.size.y / 2) + GroundOffset + Box.offset, GroundSize * Box.size);
        }

        if (SR != null)
        {
            Gizmos.DrawWireCube((Vector2)transform.position + new Vector2(Box.size.x / 2 * (SR.flipX ? -1 : 1) + GrabOffset.x * (SR.flipX ? -1 : 1), GrabOffset.y) + Box.offset, GrabSize * Box.size);

            if (Box != null)
            {
                Gizmos.DrawLine(transform.position + new Vector3(Box.size.x / 2 * (SR.flipX ? -1 : 1), 0, 0) + (Vector3)Box.offset, (Vector3)Box.offset + transform.position + new Vector3(Box.size.x / 2 * (SR.flipX ? -1 : 1), 0, 0) + new Vector3(DetectingRayLength * (SR.flipX ? -1 : 1), 0));
                Gizmos.DrawLine(transform.position + new Vector3(Box.size.x / 2 * (SR.flipX ? -1 : 1), Box.size.y / 2, 0) + (Vector3)Box.offset, (Vector3)Box.offset + transform.position + new Vector3(Box.size.x / 2 * (SR.flipX ? -1 : 1), Box.size.y / 2, 0) + new Vector3(DetectingRayLength * (SR.flipX ? -1 : 1), 0));
            }
        }
    }
#endif
}
