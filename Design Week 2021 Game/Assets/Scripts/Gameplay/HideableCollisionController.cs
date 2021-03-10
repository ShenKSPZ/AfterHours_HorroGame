using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HideableCollisionController : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D collision)
    {
        GameObject go = collision.gameObject;
        if (go.tag == "Player")
        {
            GameplayController.I().gameState = GameplayController.States.PlayerHiding;
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        GameObject go = collision.gameObject;
        if (go.tag == "Player")
        {
            GameplayController.I().gameState = GameplayController.States.Normal;
        }
    }
    
}
