using UnityEngine;
using System.Collections;

public class ColorChanger : MonoBehaviour
{
    private float timer;
    private int colorIndex;

    [Tooltip("Light sources for color changing / Источники света для смены цвета")][SerializeField] private Light[] colorLights;
    [Tooltip("Color set / Набор цветов")][SerializeField] private Color[] colors;

    [Tooltip("Interval between color changes / Интервал между сменой цвета")][SerializeField] private float changeColorInterval;
    [Tooltip("Color change time / Время смены цвета")][SerializeField] private float timeChangeColor;

    private void FixedUpdate()
    {
        if (timer < changeColorInterval)
        {
            timer += Time.fixedDeltaTime;

            if (timer > changeColorInterval)
            {
                StartCoroutine(ChangeColors());
            }
        }
    }

    private IEnumerator ChangeColors()
    {
        Color targetColor = colors[colorIndex];
        Color[] cacheLights = new Color[colorLights.Length];
        float t = 0;

        for(int i = 0; i < colorLights.Length; i++)
        {
            cacheLights[i] = colorLights[i].color;
        }

        while (t < timeChangeColor)
        {
            t += Time.deltaTime;

            for (int i = 0; i < colorLights.Length; i++)
            {
                colorLights[i].color = Color.Lerp(cacheLights[i], targetColor, t / timeChangeColor);
            }

            yield return null;
        }

        for (int i = 0; i < colorLights.Length; i++)
        {
            colorLights[i].color = targetColor;
        }

        ChangeColorIndex();
        timer = 0;
    }

    private void ChangeColorIndex()
    {
        colorIndex++;
        if(colorIndex > colors.Length - 1) colorIndex = 0;      
    }
}