using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnimatorManager : MonoBehaviour
{

    Animator anim;

    // Start is called before the first frame update
    void Awake()
    {
        anim = GetComponent<Animator>();
    }

    public void Climb()
    {
        anim.SetTrigger("Climb");
    }

    public void Jump()
    {
        anim.SetTrigger("Jump");
    }

    public void OnGround(bool OnGround)
    {
        anim.SetBool("OnGround", OnGround);
    }

    public void Locomotion(float MovingSpeed, bool IsHide, bool Grabing = false)
    {
        anim.SetBool("Grabing", Grabing);
        anim.SetFloat("MovingSpeed", MovingSpeed);
        anim.SetBool("IsHide", IsHide);
    }
}
