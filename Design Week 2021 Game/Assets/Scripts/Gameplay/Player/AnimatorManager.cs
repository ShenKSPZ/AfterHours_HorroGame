using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnimatorManager : MonoBehaviour
{

    Animator anim;

    float HidingSpeed = 0;

    // Start is called before the first frame update
    void Awake()
    {
        anim = GetComponent<Animator>();
    }

    public void Climb()
    {
        AnimatorStateInfo info = anim.GetCurrentAnimatorStateInfo(0);
        if (!info.IsName("TeddyClimb"))
        {
            anim.SetTrigger("Climb");
        }
    }

    public void Jump()
    {
        AnimatorStateInfo info = anim.GetCurrentAnimatorStateInfo(0);
        if (!info.IsName("TeddyJump"))
        {
            anim.SetTrigger("Jump");
        }
    }

    public void OnGround(bool OnGround)
    {
        anim.SetBool("OnGround", OnGround);
    }

    public void Locomotion(float MovingSpeed, bool IsHide, bool Grabing = false)
    {
        anim.SetBool("Grabing", Grabing);
        anim.SetFloat("MovingSpeed", MovingSpeed);
        anim.SetFloat("IsHide", Mathf.SmoothDamp(anim.GetFloat("IsHide"), IsHide? 1 : 0, ref HidingSpeed, 0.1f));
    }
}
