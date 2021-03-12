using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using Framework;
using DG.Tweening;

public class PostEffect : MonoBehaviour
{
    public Image blackscreen;
    public Image RetryButton;
    public Text RetryButtonText;
    public Text GameoverText;
    public Transform Hands;
    public float BlackScreenSpeed = 0.1f;

    Volume volume;
    bool AlGetCaught = false;

    private void Awake()
    {
        EventCenter.I().AddListener("GetCaught", GetCaught);
        EventCenter.I().AddListener("BlackScreen", BlackScreen);
    }

    void GetVolume()
    {
        volume = GetComponent<Volume>();
    }

    public void GetCaught()
    {
        if (!AlGetCaught)
        {
            AlGetCaught = true;
            EventCenter.I().Triggered("Lock");
            Hands.gameObject.SetActive(true);
            FMODUnity.RuntimeManager.PlayOneShot("event:/Janitor/GetCaught");
            StartCoroutine(CameraShaking());
        }
    }

    IEnumerator CameraShaking()
    {
        Camera.main.transform.DOShakePosition(5f, 0.1f, 100, 90, false, false);

        if(volume == null)
            GetVolume();
        volume.profile.TryGet(out FilmGrain grain);
        volume.profile.TryGet(out Vignette vig);

        while(grain.intensity.value < 1 || vig.intensity.value < 0.5f)
        {
            if (vig.intensity.value < 0.5f)
                vig.intensity.value += Time.deltaTime * 0.1f;

            if (grain.intensity.value < 1)
                grain.intensity.value += Time.deltaTime * 0.1f;
            yield return null;
        }
    }

    public void BlackScreen()
    {
        if (volume == null)
            GetVolume();
        volume.profile.TryGet(out Vignette vignette);

        EventCenter.I().Triggered("StopBGM");

        StartCoroutine(IE_GetCaught(vignette));
        StartCoroutine(IE_GetCaughtScreen(vignette));
    }

    IEnumerator IE_GetCaught(Vignette v)
    {
        while(v.intensity.value < 1)
        {
            v.intensity.value += Time.deltaTime * BlackScreenSpeed;
            yield return null;
        }
    }

    IEnumerator IE_GetCaughtScreen(Vignette v)
    {
        yield return new WaitUntil(() => { return v.intensity.value > 0.5f; });

        while (blackscreen.color.a < 1)
        {
            blackscreen.color = new Color(blackscreen.color.r, blackscreen.color.g, blackscreen.color.b, blackscreen.color.a + Time.deltaTime * BlackScreenSpeed);
            yield return null;
        }

        yield return new WaitForSeconds(1.2f);

        RetryButton.gameObject.SetActive(true);
        RetryButtonText.gameObject.SetActive(true);
        GameoverText.gameObject.SetActive(true);

        RetryButton.DOColor(new Color(RetryButton.color.r, RetryButton.color.g, RetryButton.color.b, 1), 3f);
        RetryButtonText.DOColor(new Color(RetryButtonText.color.r, RetryButtonText.color.g, RetryButtonText.color.b, 1), 3f);
        GameoverText.DOColor(new Color(GameoverText.color.r, GameoverText.color.g, GameoverText.color.b, 1), 3f);
    }
}
