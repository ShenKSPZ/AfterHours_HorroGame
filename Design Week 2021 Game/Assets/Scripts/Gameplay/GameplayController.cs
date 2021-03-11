using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class GameplayController : Framework.SingletonBase<GameplayController>
{
    public enum States
    {
        Normal = 0,
        PlayerHiding
    }

    public States gameState = States.Normal;

    public UnityEvent alertRaiseEvent = new UnityEvent();

}
