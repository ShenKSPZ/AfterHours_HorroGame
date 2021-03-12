using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Framework;

public class NextLevel : MonoBehaviour
{

    public string NextLevelName;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            ScenesMgr.I().LoadSceneAsyn(NextLevelName, () => { }, (x) => { });
        }
    }
}
