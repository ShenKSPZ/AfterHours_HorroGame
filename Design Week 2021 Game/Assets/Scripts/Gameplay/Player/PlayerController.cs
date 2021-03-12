using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FunctionExtend;
using UnityEngine.UI;
using FMODUnity;
using Framework;


public enum PlayerState
{
    Free,
    Climbing,
    Grabbing,
}

public class PlayerController : MonoBehaviour
{
    #region Settings
    public PlayerState State = PlayerState.Free;

    public float DetectingRayLength = 1f;
    [Header("Movement")]
    public float MovingSpeed = 9;
    public float HidingMovingSpeed = 3;
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
    public Vector2 ClimbingBoxOffset;
    public Vector2 ClimbingBoxSize;

    [Header("GroundDetect")]
    public Vector2 GroundSize = Vector2.zero;
    public Vector2 GroundOffset = Vector2.zero;
    public float MaxSlope = 50;
    public LayerMask GroundLayer;
    #endregion

    #region Reference
    AnimatorManager Anim;
    Rigidbody2D Rig;
    BoxCollider2D Box;
    #endregion

    #region Runtime
    bool FlipX = false;
    FMOD.Studio.EventInstance GrabingInstances;

    //Velocity
    Vector2 MovingDirection = Vector2.zero;
    float DampVelocity1 = 0;
    float HighMovingSpeed = 0;

    //About Ground
    public bool OnGround = false;
    public bool ActualOnGround = false;
    float LeftGrounded = 0f;
    public float OnGroundRadius = 0;

    //Jumping
    bool IsJumped = false;
    bool LeaveGround = false;

    //Grabbing
    GameObject ObjectGrabed = null;

    //Climbing
    bool WaitForClimb = false;
    bool CanClimb = false;

    //Hide
    public bool Hide = false;
    #endregion

    // Start is called before the first frame update
    void Awake()
    {
        //FMODUnity.RuntimeManager.PlayOneShot("event:/01D", transform.position);
        Rig = GetComponent<Rigidbody2D>();
        Box = GetComponent<BoxCollider2D>();
        Anim = GetComponent<AnimatorManager>();
        HighMovingSpeed = MovingSpeed;
    }

    private void Start()
    {
        GrabingInstances = RuntimeManager.CreateInstance("event:/Teddy/Grab_Loop");
    }

    private void Update()
    {
        //EventCenter.I().Triggered("GetCaught");

        StateCheck();
        if (FlipX)
            transform.eulerAngles = Vector3.zero;
        else
            transform.eulerAngles = new Vector3(0, 180, 0);

        ObjectGrabed = null;
        Anim.OnGround(ActualOnGround);

        switch (State)
        {
            case PlayerState.Free:
                #region Climbing
                if (CanClimb)
                {
                    if (Input.GetAxis("Jump") != 0 || (Input.GetAxisRaw("Horizontal") > 0 && !FlipX) || (Input.GetAxisRaw("Horizontal") < 0 && FlipX))
                    {
                        GoState(PlayerState.Climbing);
                    }
                }
                #endregion

                #region Grabbing
                if (Input.GetButton("Grab") && ActualOnGround)
                {
                    Collider2D coll = Physics2D.OverlapBox((Vector2)transform.position + new Vector2(Box.size.x / 2 * (FlipX ? -1 : 1) + GrabOffset.x * (FlipX ? -1 : 1), GrabOffset.y) + Box.offset, GrabSize * Box.size, 0f, InteractableLayer);
                    ObjectGrabed = coll != null ? coll.gameObject : null;
                    if (ObjectGrabed != null)
                        GoState(PlayerState.Grabbing);
                }
                #endregion

                #region Moving
                if (Input.GetAxisRaw("Horizontal") > 0)
                {
                    MovingDirection = new Vector2(Mathf.SmoothDamp(MovingDirection.x, MovingSpeed * 50 * Time.fixedDeltaTime, ref DampVelocity1, AccelerateSpeed), MovingDirection.y);
                    FlipX = false;

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
                    MovingDirection = new Vector2(Mathf.SmoothDamp(MovingDirection.x, MovingSpeed * -50 * Time.fixedDeltaTime, ref DampVelocity1, AccelerateSpeed), MovingDirection.y);
                    FlipX = true;

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
                    MovingDirection = new Vector2(Mathf.SmoothDamp(MovingDirection.x, 0, ref DampVelocity1, DecelerateSpeed), MovingDirection.y);

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
                    Anim.Jump();
                }
                #endregion

                GrabingInstances.getPlaybackState(out FMOD.Studio.PLAYBACK_STATE playbackstateMoving);
                if (playbackstateMoving != FMOD.Studio.PLAYBACK_STATE.STOPPED || playbackstateMoving != FMOD.Studio.PLAYBACK_STATE.STOPPING)
                    GrabingInstances.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);

                Anim.Locomotion(Rig.velocity.magnitude / MovingSpeed, Hide);

                break;
            case PlayerState.Climbing:
                MovingDirection = Vector2.zero;
                Rig.velocity = Vector2.zero;
                Rig.isKinematic = true;
                Anim.Climb();
                break;
            case PlayerState.Grabbing:
                
                #region Grabbing
                if (Input.GetButton("Grab") && ActualOnGround)
                {
                    Collider2D coll = Physics2D.OverlapBox((Vector2)transform.position + new Vector2(Box.size.x / 2 * (FlipX ? -1 : 1) + GrabOffset.x * (FlipX ? -1 : 1), GrabOffset.y) + Box.offset, GrabSize * Box.size, 0f, InteractableLayer);
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
                        if (FlipX)
                            MovingDirection = new Vector2(OtherRig.velocity.x * 0.5f, MovingDirection.y);
                        else
                            MovingDirection = new Vector2(OtherRig.velocity.x, MovingDirection.y);
                    }
                    else
                    {
                        MovingDirection = new Vector2(Mathf.SmoothDamp(MovingDirection.x, MovingSpeed * 50 * Time.fixedDeltaTime, ref DampVelocity1, AccelerateSpeed), MovingDirection.y);
                        FlipX = false;
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
                        if (!FlipX)
                            MovingDirection = new Vector2(OtherRig.velocity.x * 0.5f, MovingDirection.y);
                        else
                            MovingDirection = new Vector2(OtherRig.velocity.x, MovingDirection.y);
                    }
                    else
                    {
                        MovingDirection = new Vector2(Mathf.SmoothDamp(MovingDirection.x, MovingSpeed * -50 * Time.fixedDeltaTime, ref DampVelocity1, AccelerateSpeed), MovingDirection.y);
                        FlipX = true;
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

                if (Rig.velocity.magnitude / MovingSpeed > 0.1)
                {
                    GrabingInstances.getPlaybackState(out FMOD.Studio.PLAYBACK_STATE playbackstate);
                    if (playbackstate == FMOD.Studio.PLAYBACK_STATE.STOPPED || playbackstate == FMOD.Studio.PLAYBACK_STATE.STOPPING)
                        GrabingInstances.start();
                }
                else
                {
                    GrabingInstances.getPlaybackState(out FMOD.Studio.PLAYBACK_STATE playbackstate);
                    if (playbackstate != FMOD.Studio.PLAYBACK_STATE.STOPPED || playbackstate != FMOD.Studio.PLAYBACK_STATE.STOPPING)
                        GrabingInstances.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
                }

                Anim.Locomotion(Rig.velocity.magnitude / MovingSpeed, Hide, true);
                break;
            default:
                break;
        }

        //LastOnGround = ActualOnGround;
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
        Collider2D BodyHit = Physics2D.OverlapBox(transform.position + new Vector3(Box.size.x / 2 * (FlipX ? -1 : 1) + ClimbingBoxOffset.x * (FlipX ? -1 : 1), ClimbingBoxOffset.y) + (Vector3)Box.offset, new Vector2(DetectingRayLength, ClimbingBoxSize.y * Box.size.y), 0f, GroundLayer);
        RaycastHit2D HeadHit = Physics2D.Raycast(transform.position + new Vector3(Box.size.x / 2 * (FlipX ? -1 : 1), Box.size.y / 2, 0) + (Vector3)Box.offset, FlipX ? Vector2.left : Vector2.right, DetectingRayLength, GroundLayer);
        if (BodyHit != null && HeadHit.collider == null && State == PlayerState.Free)
        {
            WaitForClimb = true;
        }
        else if (BodyHit != null && HeadHit.collider != null && WaitForClimb && State == PlayerState.Free)
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

        MovingSpeed = Hide ? HidingMovingSpeed : HighMovingSpeed;
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
        velocity = new Vector2(Mathf.Cos(OnGroundRadius) * moveDistance, Mathf.Sin(OnGroundRadius) * moveDistance);
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
            Physics2D.Raycast(LeftPos, Vector2.right + Vector2.up, DetectingRayLength * 2, GroundLayer),
            Physics2D.Raycast(RightPos, Vector2.left + Vector2.up, DetectingRayLength* 2, GroundLayer),
        };

        bool Detected = false;
        bool CollideSomething = false;

        bool InnerDetected = false;

        for (int i = 0; i < InnerHits.Length; i++)
        {
            if (InnerHits[i].collider != null)
            {
                InnerDetected = true;
                break;
            }
        }

        if (!InnerDetected)
        {
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
                    Collider2D coll = Physics2D.OverlapBox((Vector2)transform.position - new Vector2(0, Box.size.y / 2 + 0.01f) + GroundOffset + Box.offset, GroundSize * Box.size, 0, GroundLayer);
                    Detected = coll != null ? true : false;
                }
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
        transform.position += new Vector3(AfterClimbingOffset.x * (FlipX ? -1 : 1), AfterClimbingOffset.y);
        Rig.isKinematic = false;
        ActualOnGround = true;
        OnGround = true;
        State = next;
        yield return new WaitForEndOfFrame();
    }

    public void Anim_Jump()
    {
        RuntimeManager.PlayOneShot("event:/Teddy/Jump");
        IsJumped = true;
        LeaveGround = false;
        Rig.velocity = new Vector2(Rig.velocity.x, JumpingSpeed);
    }

    public void Anim_Walk()
    {
        RuntimeManager.PlayOneShot("event:/Teddy/Walk");
    }

    public void Anim_Land()
    {
        RuntimeManager.PlayOneShot("event:/Teddy/Land");
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
            
            Gizmos.DrawWireCube((Vector2)transform.position + new Vector2(Box.size.x / 2 * (FlipX ? -1 : 1) + GrabOffset.x * (FlipX ? -1 : 1), GrabOffset.y) + Box.offset, GrabSize * Box.size);

            Gizmos.color = Color.black;
            Gizmos.DrawWireCube(transform.position + new Vector3(Box.size.x / 2 * (FlipX ? -1 : 1) + ClimbingBoxOffset.x * (FlipX ? -1 : 1), ClimbingBoxOffset.y) + (Vector3)Box.offset, new Vector2(DetectingRayLength, ClimbingBoxSize.y * Box.size.y));

            Gizmos.color = Color.red;
            Gizmos.DrawLine(transform.position + new Vector3(Box.size.x / 2 * (FlipX ? -1 : 1), Box.size.y / 2, 0) + (Vector3)Box.offset, (Vector3)Box.offset + transform.position + new Vector3(Box.size.x / 2 * (FlipX ? -1 : 1), Box.size.y / 2, 0) + new Vector3(DetectingRayLength * (FlipX ? -1 : 1), 0));

            Gizmos.DrawLine(LeftPos, LeftPos + (Vector2.right + Vector2.up) * DetectingRayLength * 2);
            Gizmos.DrawLine(RightPos, RightPos + (Vector2.left + Vector2.up) * DetectingRayLength * 2);
        }
    }
#endif

    private void OnTriggerEnter2D(Collider2D collision)
    {
        OnTriggerStay2D(collision);
    }

    private void OnTriggerStay2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag("Hideable"))
        {
            Hide = true;
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag("Hideable"))
        {
            Hide = false;
        }
    }
}
