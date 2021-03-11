using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InteractableCollisionController : MonoBehaviour
{
    private void OnCollisionEnter2D(Collision2D collision)
    {
        GameObject go = collision.gameObject;
        if (gameObject.name == "Broom" && go.tag == "Player")
        {
            GameplayController.I().alertRaiseEvent.Invoke();
        }
    }
}
