using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Framework;

public class RetryLevel : MonoBehaviour
{
    public void Retry()
    {
        ScenesMgr.I().ReloadCurrentSceneAsyn(() => { }, (x) => { });
    }
}
