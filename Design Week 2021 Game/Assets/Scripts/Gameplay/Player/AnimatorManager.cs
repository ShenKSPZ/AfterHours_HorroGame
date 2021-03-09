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
    readonly int m_Climb = Animator.StringToHash("Climb");
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

    }

    public void Run(bool Grabbing)
    {

    }

    public void Idle(bool Grabbing)
    {
        anim.Play(m_Idle, 0);
    }
}
