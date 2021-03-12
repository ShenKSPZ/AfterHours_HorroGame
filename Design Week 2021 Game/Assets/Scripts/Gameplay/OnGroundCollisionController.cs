using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OnGroundCollisionController : MonoBehaviour
{
    public JanitorController controller;
    private void OnCollisionEnter2D(Collision2D collision)
    {
        GameObject go = collision.gameObject;
        if (go.CompareTag("Ground"))
        {
            FMODUnity.RuntimeManager.PlayOneShot("event:/HitGround", transform.position);
            controller.alertRaiseEvent.Invoke();
        }
    }
}
