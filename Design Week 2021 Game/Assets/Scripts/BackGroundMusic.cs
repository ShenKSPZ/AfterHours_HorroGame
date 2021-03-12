﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FMOD;
using FMODUnity;

public class BackGroundMusic : MonoBehaviour
{
    public List<string> Audio = new List<string>();
    public float Volume;
    List<FMOD.Studio.EventInstance> instances = new List<FMOD.Studio.EventInstance>();

    private void OnEnable()
    {
        for (int i = 0; i < Audio.Count; i++)
        {
            instances.Add(RuntimeManager.CreateInstance(Audio[i]));
            instances[i].start();
            instances[i].setVolume(Volume);
        }
    }

    private void OnDisable()
    {
        for (int i = 0; i < instances.Count; i++)
        {
            instances[i].stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
        }
    }
}