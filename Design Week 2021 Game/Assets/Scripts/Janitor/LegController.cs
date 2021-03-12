using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LegController : MonoBehaviour
{
    public void OnWalk()
    {
        FMODUnity.RuntimeManager.PlayOneShot("event:/Janitor/Walk", transform.position);
    }

    public void OnRun()
    {
        FMODUnity.RuntimeManager.PlayOneShot("event:/Janitor/Run", transform.position);
    }
}
