using UnityEngine;
using System.Collections;

public class SmoothlyLight : MonoBehaviour
{
    public enum StartState
    {
        Decrease, Increase
    }

    private enum ChangeColorMoment
    {
        On, Off
    }

    private Renderer[] renderersMat;
    private float[] startMatAlpha;
    private float offTimer;
    private float onTimer;
    private int colorNumber;

    [Header("Main Settings")]
    [Tooltip("Starting state of the light / Стартовое состояние света")][SerializeField] private StartState startState;
    [Tooltip("Light source / Источник света")][SerializeField] private new Light light;
    [Tooltip("Tracking purpose / Цель слежения")][SerializeField] private Transform target;
    [Tooltip("The time of change of light intensity / Время изменения интенсивности света")][SerializeField] private float timeChangeIntensity;
    [Tooltip("Minimum light intensity / Минимальное значение интенсивности света")][SerializeField] private float minIntensity;
    [Tooltip("Maximum light intensity / Максимальное значение интенсивности света")][SerializeField] private float maxIntensity;

    [Header("Off Time")]
    [Tooltip("Minimum time off state (takes a random time between the minimum and maximum time) / Минимальное время выключенного состояния (берется рандомное время между минимальным и максимальным временем)")][SerializeField] private float minTimeOff;
    [Tooltip("Minimum time off state (takes a random time between the minimum and maximum time) / Минимальное время выключенного состояния (берется рандомное время между минимальным и максимальным временем)")][SerializeField] private float maxTimeOff;

    [Header("On Time")]
    [Tooltip("Minimum time off state (takes a random time between the minimum and maximum time) / Минимальное время выключенного состояния (берется рандомное время между минимальным и максимальным временем)")][SerializeField] private float minTimeOn;
    [Tooltip("Minimum time off state (takes a random time between the minimum and maximum time) / Минимальное время выключенного состояния (берется рандомное время между минимальным и максимальным временем)")][SerializeField] private float maxTimeOn;

    [Header("Color Settings")]
    [Tooltip("Moment of color change / Момент смены цвета")] [SerializeField] private ChangeColorMoment changeColorMoment;
    [Tooltip("Light sources for changing colors / Источники света для смены цвета")] [SerializeField] private Light[] colorLights;
    [Tooltip("Set of flowers / Набор цветов")] [SerializeField] private Color[] colors;
    [Tooltip("Objects that have materials on which to change color / Объекты, на которых находятся материалы, у которых менять цвет")] [SerializeField] private GameObject[] GOMaterials;
    [Tooltip("Time to change color / Время смены цвета")] [SerializeField] private float timeChangeColor;

    private void Awake()
    {
        renderersMat = new Renderer[GOMaterials.Length];
        startMatAlpha = new float[GOMaterials.Length];

        for(int i = 0; i < renderersMat.Length; i++)
        {
            renderersMat[i] = GOMaterials[i].GetComponent<Renderer>();
            renderersMat[i].material.SetColor("_TintColor", renderersMat[i].material.GetColor("_TintColor"));
            startMatAlpha[i] = renderersMat[i].material.GetColor("_TintColor").a;
        }
    }

    private void Start()
    {
        SetStartState();
    }

    private void Update()
    {
        if(target != null)
        {
            transform.LookAt(target);
        }
    }

    private void SetStartState()
    {
        switch (startState)
        {
            case StartState.Decrease:
                StartCoroutine(DecreaseLight());
                break;

            case StartState.Increase:
                StartCoroutine(IncreaseLight());
                break;
        }
    }

    private void SetOffTime()
    {
        offTimer = Random.Range(minTimeOff, maxTimeOff);
    }

    private void SetOnTime()
    {
        onTimer = Random.Range(minTimeOn, maxTimeOn);
    }

    private IEnumerator DecreaseLight()
    {
        float t = 0;
        float startIntensity = light.intensity;

        while(t < timeChangeIntensity)
        {
            t += Time.deltaTime;

            light.intensity = Mathf.Lerp(startIntensity, minIntensity, t / timeChangeIntensity);

            for(int i = 0; i < renderersMat.Length; i++)
            {
                float alpha = Mathf.Lerp(startMatAlpha[i], 0, t / timeChangeIntensity);
                Color curColor = renderersMat[i].material.GetColor("_TintColor");
                renderersMat[i].material.SetColor("_TintColor", new Color(curColor.r, curColor.g, curColor.b, alpha));
            }

            yield return null;
        }

        light.intensity = 0;

        for (int i = 0; i < renderersMat.Length; i++)
        {
            Color curColor = renderersMat[i].material.GetColor("_TintColor");
            renderersMat[i].material.SetColor("_TintColor", new Color(curColor.r, curColor.g, curColor.b, 0));
        }

        if(light.intensity <= minIntensity)
        {
            SetOffTime();

            ColorMoment(ChangeColorMoment.Off);

            StartCoroutine(OffLight());
        }
    }

    private IEnumerator IncreaseLight()
    {
        float t = 0;
        float startIntensity = light.intensity;

        while(t < timeChangeIntensity)
        {
            t += Time.deltaTime;

            light.intensity = Mathf.Lerp(startIntensity, maxIntensity, t / timeChangeIntensity);

            for(int i = 0; i < renderersMat.Length; i++)
            {
                float alpha = Mathf.Lerp(0, startMatAlpha[i], t / timeChangeIntensity);
                Color curColor = renderersMat[i].material.GetColor("_TintColor");
                renderersMat[i].material.SetColor("_TintColor", new Color(curColor.r, curColor.g, curColor.b, alpha));
            }

            yield return null;
        }

        light.intensity = maxIntensity;

        for (int i = 0; i < renderersMat.Length; i++)
        {
            Color curColor = renderersMat[i].material.GetColor("_TintColor");
            renderersMat[i].material.SetColor("_TintColor", new Color(curColor.r, curColor.g, curColor.b, startMatAlpha[i]));
        }

        if (light.intensity >= maxIntensity)
        {
            SetOnTime();

            ColorMoment(ChangeColorMoment.On);

            StartCoroutine(OnLight());
        }
    }

    private IEnumerator OffLight()
    {
        yield return new WaitForSeconds(offTimer);

        StartCoroutine(IncreaseLight());
    }

    private IEnumerator OnLight()
    {
        yield return new WaitForSeconds(onTimer);

        StartCoroutine(DecreaseLight());
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
            cacheMats[i] = renderersMat[i].material.GetColor("_TintColor");
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

                renderersMat[i].material.SetColor("_TintColor", new Color(c.r, c.g, c.b, startMatAlpha[i]));
            }

            yield return null;
        }

        for (int i = 0; i < colorLights.Length; i++)
        {
            colorLights[i].color = targetColor;
        }

        for (int i = 0; i < renderersMat.Length; i++)
        {
            renderersMat[i].material.SetColor("_TintColor", new Color(targetColor.r, targetColor.g, targetColor.b, startMatAlpha[i]));
        }

        colorNumber++;

        if(colorNumber > colors.Length - 1) colorNumber = 0;
    }

    private void ColorMoment(ChangeColorMoment moment)
    {
        if (colorLights.Length > 0 || colors.Length > 0)
        {
            if (changeColorMoment == moment)
            {
                StartCoroutine(ChangeBothColors());
            }
        }
    }
}