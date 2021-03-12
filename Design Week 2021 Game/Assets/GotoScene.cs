using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Framework;

public class GotoScene : MonoBehaviour
{
    public void GOTO(string name)
    {
        ScenesMgr.I().LoadSceneAsyn(name, () => { }, (x) => { });
    }
}
