using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Framework;

public class HandEmitter : MonoBehaviour
{
    public void Anim_TurnBlack()
    {
        EventCenter.I().Triggered("BlackScreen");
    }
}
