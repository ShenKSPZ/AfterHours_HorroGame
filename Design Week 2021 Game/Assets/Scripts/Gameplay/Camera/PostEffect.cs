using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using Framework;

public class PostEffect : MonoBehaviour
{
    public Image blackscreen;
    public float BlackScreenSpeed = 0.1f;

    Volume volume;

    private void Awake()
    {
        EventCenter.I().AddListener("GetCaught", GetCaught);
    }

    void GetVolume()
    {
        volume = GetComponent<Volume>();
    }

    public void GetCaught()
    {
        if(volume == null)
            GetVolume();

        volume.profile.TryGet(out Vignette vignette);

        StartCoroutine(IE_GetCaught(vignette));
        StartCoroutine(IE_GetCaughtScreen(vignette));
    }

    IEnumerator IE_GetCaught(Vignette v)
    {
        while(v.intensity.value != 1)
        {
            v.intensity.value += Time.deltaTime * BlackScreenSpeed;
            yield return null;
        }

        while (blackscreen.color.a != 1)
        {
            blackscreen.color = new Color(blackscreen.color.r, blackscreen.color.g, blackscreen.color.b, blackscreen.color.a + Time.deltaTime * BlackScreenSpeed);
            yield return null;
        }
    }

    IEnumerator IE_GetCaughtScreen(Vignette v)
    {
        yield return new WaitUntil(() => { return v.intensity.value > 0.5f; });

        while (blackscreen.color.a != 1)
        {
            blackscreen.color = new Color(blackscreen.color.r, blackscreen.color.g, blackscreen.color.b, blackscreen.color.a + Time.deltaTime * BlackScreenSpeed);
            yield return null;
        }
    }
}
