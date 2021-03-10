using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnimatorManager : MonoBehaviour
{

    Animator anim;
    AnimatorStateInfo Info;

    #region NameHash
    readonly int m_Idle = Animator.StringToHash("Idle");
    readonly int m_Run = Animator.StringToHash("Run");
    readonly int m_Jump = Animator.StringToHash("Jump");
    readonly int m_Fall = Animator.StringToHash("Fall");
    readonly int m_Land = Animator.StringToHash("Land");
    readonly int m_Climb = Animator.StringToHash("Climb");
    readonly int m_Grab = Animator.StringToHash("Grab");
    readonly int m_GrabWalkBack = Animator.StringToHash("GrabWalkBack");
    readonly int m_GrabWalkForward = Animator.StringToHash("GrabWalkForward");
    #endregion

    // Start is called before the first frame update
    void Awake()
    {
        anim = GetComponent<Animator>();
    }

    public void Climb()
    {
        anim.Play(m_Climb, 0);
    }

    public void Jump()
    {
        anim.Play(m_Jump, 0);
    }

    public void Fall()
    {
        AnimatorStateInfo info = anim.GetCurrentAnimatorStateInfo(0);
        if (!info.IsName("Jump"))
            anim.Play(m_Fall, 0);
    }

    public void Run(bool Grabbing, bool flipX = false)
    {
        if (!Grabbing)
        {
            anim.Play(m_Run, 0);
        }
        else
        {
            if(Input.GetAxisRaw("Horizontal") > 0)
            {
                anim.Play(!flipX ? m_GrabWalkForward : m_GrabWalkBack, 0);
            }
            else if (Input.GetAxisRaw("Horizontal") < 0)
            {
                anim.Play(flipX ? m_GrabWalkForward : m_GrabWalkBack, 0);
            }
        }
    }

    public void Idle(bool Grabbing, bool isLanding)
    {
        if (isLanding)
        {
            anim.Play(m_Land, 0);
        }
        else
        {
            AnimatorStateInfo info = anim.GetCurrentAnimatorStateInfo(0);
            if (!info.IsName("Land"))
            {
                anim.Play(Grabbing ? m_Grab : m_Idle, 0);
            }
        }
    }
}
