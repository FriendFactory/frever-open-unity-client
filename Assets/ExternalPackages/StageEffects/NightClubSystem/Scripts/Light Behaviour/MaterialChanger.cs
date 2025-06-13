using System.Collections;
using UnityEngine;

public class MaterialChanger : MonoBehaviour
{
    private Renderer[] renderersMat;
    private float[] startMatAlpha;
    private float timer;
    private int colorIndex;
    
    [Tooltip("Objects that have materials on which to change color / Объекты, на которых находятся материалы у которых менять цвет")][SerializeField] private GameObject[] materials;
    [Tooltip("Color set / Набор цветов")][SerializeField] private Color[] colors;
    [Tooltip("Interval between color changes / Интервал между сменой цвета")][SerializeField] private float changeColorInterval;
    [Tooltip("Color change time / Время смены цвета")][SerializeField] private float timeChangeColor;

    private void Awake()
    {
        renderersMat = new Renderer[materials.Length];
        startMatAlpha = new float[materials.Length];

        for(int i = 0; i < renderersMat.Length; i++)
        {
            renderersMat[i] = materials[i].GetComponent<Renderer>();
            renderersMat[i].material.SetColor("_TintColor", renderersMat[i].material.GetColor("_TintColor"));
            startMatAlpha[i] = renderersMat[i].material.GetColor("_TintColor").a;
        }
    }
    
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
        float t = 0;

        Color targetColor = colors[colorIndex];
        Color[] cacheMats = new Color[renderersMat.Length];

        for(int i = 0; i < renderersMat.Length; i++)
        {
            cacheMats[i] = renderersMat[i].material.GetColor("_TintColor");
        }

        while (t < timeChangeColor)
        {
            t += Time.deltaTime;

            for (int i = 0; i < renderersMat.Length; i++)
            {
                Color c = Color.Lerp(cacheMats[i], targetColor, t / timeChangeColor);
                renderersMat[i].material.SetColor("_TintColor", new Color(c.r, c.g, c.b, startMatAlpha[i]));
            }

            yield return null;
        }

        for (int i = 0; i < renderersMat.Length; i++)
        {
            renderersMat[i].material.SetColor("_TintColor", new Color(targetColor.r, targetColor.g, targetColor.b, startMatAlpha[i]));
        }

        ChangeColorIndex();
    }
    
    private void ChangeColorIndex()
    {
        colorIndex++;
        if(colorIndex > colors.Length - 1) colorIndex = 0;      
    }
}