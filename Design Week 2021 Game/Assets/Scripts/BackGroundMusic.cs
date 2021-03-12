using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FMOD;
using FMODUnity;
using Framework;

public class BackGroundMusic : MonoBehaviour
{
    public List<string> Audio = new List<string>();
    public float Volume;
    List<FMOD.Studio.EventInstance> instances = new List<FMOD.Studio.EventInstance>();

    private void Start()
    {
        for (int i = 0; i < Audio.Count; i++)
        {
            instances.Add(RuntimeManager.CreateInstance(Audio[i]));
            instances[i].setVolume(Volume);
        }

        EventCenter.I().AddListener("StopBGM", Stop);

        Invoke("Play", 0.5f);
    }

    void Play()
    {
        for (int i = 0; i < instances.Count; i++)
        {
            instances[i].start();
        }
    }

    private void OnDisable()
    {
        for (int i = 0; i < instances.Count; i++)
        {
            instances[i].stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
        }

        EventCenter.I().RemoveEventListener("StopBGM", Stop);
    }

    private void Stop()
    {
        for (int i = 0; i < instances.Count; i++)
        {
            instances[i].stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
        }
    }
}
