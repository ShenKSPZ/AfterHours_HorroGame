using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Walking : MonoBehaviour
{
    public void Anim_Walk()
    {
        Debug.Log("Walk");
        FMODUnity.RuntimeManager.PlayOneShot("event:/Janitor/Walk", GetComponent<Transform>().position);
    }
}
