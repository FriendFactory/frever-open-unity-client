using UnityEngine;
using System.Collections;
using Modules.LevelManaging.FullPreview.Assets;

public class LightController : MonoBehaviour
{
    private enum LightState
    {
        UntilFlicker, Flicker
    }

    private LightState lightState = LightState.UntilFlicker;
    private Renderer[] renderersMat;
    private float[] startMatAlpha;
    private bool[] startStatsOfLight;
    private float untilFlickerTimer;
    private float frequencyTimer;
    private float flickeringTimer;
    private float cookieTimer;
    private int colorNumber;

    [Header("Cookie Settings")]
    [Tooltip("Array of cookies (random cookie is selected) / Массив куки(Выбирается рандомная cookie)")][SerializeField] private Texture[] cookies;
    [Tooltip("Minimum time to change cookies (takes a random time between the min. And max. Time) / Минимальное время до смены cookie (берется рандомное время между мин. и макс. временем)")][SerializeField] private float minChangeCookie;
    [Tooltip("Maximum time to change cookie (take random time between min. And max. Time) / Максимальное время до смены cookie (берется рандомное время между мин. и макс. временем)")][SerializeField] private float maxChangeCookie;

    [Header("Main Settings")]
    [Tooltip("Array of objects for on/off / Массив объектов для вкл/выкл")][SerializeField] private GameObject[] objs;
    [Tooltip("Array of light sources that change cookies / Массив источников света, которым менять cookie")][SerializeField] private Light[] cookieLight;

    [Header("Time To Start Flick")]
    [Tooltip("Minimum time to blink (random time between min. And max. Time is taken) / Минимальное время до мигания (берется рандомное время между мин. и макс. временем)")][SerializeField] private float minUntilFlicker;
    [Tooltip("Maximum time to blink (random time between min. And max. Time is taken) / Максимальное время до мигания (берется рандомное время между мин. и макс. временем)")][SerializeField] private float maxUntilFlicker;

    [Header("Frequency of flicker")]
    [Tooltip("The minimum time of the flicker frequency (the random time between the min. And max. Time is taken) / Минимальное время частоты мерцания (берется рандомное время между мин. и макс. временем)")][SerializeField] private float minFlickerFrequency;
    [Tooltip("The maximum time of the flicker frequency (a random time between the min. And max. Time is taken) / Максимальное время частоты мерцания (берется рандомное время между мин. и макс. временем)")][SerializeField] private float maxFlickerFrequency;

    [Header("Total Time Flickering")]
    [Tooltip("The minimum time of the blink duration (a random time between the min. And max. Time is taken) / Минимальное время продолжительности мерцания (берется рандомное время между мин. и макс. временем)")][SerializeField] private float minFlickering;
    [Tooltip("Maximum blink time (random time between min. And max. Time taken) / Максимальное время продолжительности мерцания (берется рандомное время между мин. и макс. временем)")][SerializeField] private float maxFlickering;

    [Header("Color Settings")]
    [Tooltip("Light sources for color changing / Источники света для смены цвета")][SerializeField] private Light[] colorLights;
    [Tooltip("Color set / Набор цветов")][SerializeField] private Color[] colors;
    [Tooltip("Objects that have materials on which to change color / Объекты, на которых находятся материалы у которых менять цвет")][SerializeField] private GameObject[] GOMaterials;
    [Tooltip("Color change time / Время смены цвета")][SerializeField] private float timeChangeColor;

    private void Awake()
    {
        startStatsOfLight = new bool[objs.Length];

        for (int i = 0; i < objs.Length; i++)
        {
            startStatsOfLight[i] = objs[i].activeSelf;
        }

        renderersMat = new Renderer[GOMaterials.Length];
        startMatAlpha = new float[GOMaterials.Length];

        for(int i = 0; i < renderersMat.Length; i++)
        {
            renderersMat[i] = GOMaterials[i].GetComponent<Renderer>();
            renderersMat[i].material.SetColor("_BaseColor", renderersMat[i].material.GetColor("_BaseColor"));
            startMatAlpha[i] = renderersMat[i].material.GetColor("_BaseColor").a;
        }
    }

    private void Start()
    {
        CookieTimer();
        TimeUntilFlicker();
    }

    private void Update()
    {
        Cookie();

        if (lightState == LightState.UntilFlicker)
        {
            UntilFlicer();
        }
        else if(lightState == LightState.Flicker)
        {
            Flickering();
        }
    }

    private void Cookie()
    {
        cookieTimer -= Time.deltaTime;

        if (cookieTimer <= 0)
        {
            for (int i = 0; i < cookieLight.Length; i++)
            {
                cookieLight[i].cookie = cookies[Random.Range(0, cookies.Length)];
            }

            CookieTimer();
        }
    }

    private void UntilFlicer()
    {
        untilFlickerTimer -= Time.deltaTime;

        if (untilFlickerTimer <= 0)
        {
            FlickerFrequency();
            FlickeringTimer();

            if(colorLights.Length > 0 || colors.Length > 0)
            {
                StartCoroutine(ChangeBothColors());
            }

            lightState = LightState.Flicker;
        }
    }

    private void Flickering()
    {
        flickeringTimer -= Time.deltaTime;
        frequencyTimer -= Time.deltaTime;

        if (flickeringTimer <= 0)
        {
            for (int i = 0; i < objs.Length; i++)
            {
                var obj = objs[i];
                obj.SetActive(startStatsOfLight[i]);
            }

            TimeUntilFlicker();

            lightState = LightState.UntilFlicker;

            return;
        }

        if (frequencyTimer <= 0)
        {
            for (int i = 0; i < objs.Length; i++)
            {
                var obj = objs[i];
                obj.SetActive(!obj.activeSelf);
            }

            FlickerFrequency();
        }
    }

    private void TimeUntilFlicker()
    {
        untilFlickerTimer = Random.Range(minUntilFlicker, maxUntilFlicker);
    }

    private void FlickerFrequency()
    {
        frequencyTimer = Random.Range(minFlickerFrequency, maxFlickerFrequency);
    }

    private void FlickeringTimer()
    {
        flickeringTimer = Random.Range(minFlickering, maxFlickering);
    }

    private void CookieTimer()
    {
        cookieTimer = Random.Range(minChangeCookie, maxChangeCookie);
    }

    private IEnumerator ChangeBothColors()
    {
        float t = 0;

        Color targetColor = colors[colorNumber];
        Color[] cacheLights = new Color[colorLights.Length];
        Color[] cacheMats = new Color[renderersMat.Length];

        for(int i = 0; i < colorLights.Length; i++)
        {
            cacheLights[i] = colorLights[i].color;
        }

        for(int i = 0; i < renderersMat.Length; i++)
        {
            cacheMats[i] = renderersMat[i].material.GetColor("_BaseColor");
        }

        while (t < timeChangeColor)
        {
            t += Time.deltaTime;

            for (int i = 0; i < colorLights.Length; i++)
            {
                colorLights[i].color = Color.Lerp(cacheLights[i], targetColor, t / timeChangeColor);
            }

            for (int i = 0; i < renderersMat.Length; i++)
            {
                Color c = Color.Lerp(cacheMats[i], targetColor, t / timeChangeColor);

                renderersMat[i].material.SetColor("_BaseColor", new Color(c.r, c.g, c.b, startMatAlpha[i]));
            }

            yield return null;
        }

        for (int i = 0; i < colorLights.Length; i++)
        {
            colorLights[i].color = targetColor;
        }

        for (int i = 0; i < renderersMat.Length; i++)
        {
            renderersMat[i].material.SetColor("_BaseColor", new Color(targetColor.r, targetColor.g, targetColor.b, startMatAlpha[i]));
        }

        colorNumber++;

        if(colorNumber > colors.Length - 1) colorNumber = 0;
    }
    
}