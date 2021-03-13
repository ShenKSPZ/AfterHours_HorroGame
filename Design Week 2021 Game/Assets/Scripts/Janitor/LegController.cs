using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class LegController : MonoBehaviour
{
    public void OnWalk()
    {
        FMODUnity.RuntimeManager.PlayOneShot("event:/Janitor/Walk", transform.position);
        Camera.main.DOShakePosition(0.1f, 0.01f, 1, 10, true);
    }

    public void OnRun()
    {
        FMODUnity.RuntimeManager.PlayOneShot("event:/Janitor/Run", transform.position);
        Camera.main.DOShakePosition(0.1f, 0.10f, 20, 50, true);
    }
}
