using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InteractableCollisionController : MonoBehaviour
{
    public JanitorController controller;
    private void OnCollisionEnter2D(Collision2D collision)
    {
        
        GameObject go = collision.gameObject;
        if (go.tag == "Player")
        {
            controller.alertRaiseEvent.Invoke();
        }
    }
}
