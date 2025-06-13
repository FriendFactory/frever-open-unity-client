using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UMA;
using Extensions;

public class GetColors : MonoBehaviour
{
    public SharedColorTable SharedColor;

    void Start()
    {

    }

    [ContextMenu("get colors in text")]
    public void GetColor()
    {
        string colorString = $"(\'{SharedColor.sharedColorName}\',"+"\'{";
        foreach (var color in SharedColor.colors)
        {
            colorString += $"{((Color32)color.color).ConvertToInt()},";
        }
        colorString = colorString.Remove(colorString.Length - 1);
        colorString += "}\')";
        Debug.LogError(colorString);
    }
}
