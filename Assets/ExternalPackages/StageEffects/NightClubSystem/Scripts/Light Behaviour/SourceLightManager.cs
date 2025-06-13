using UnityEngine;
using UnityEngine.UI;

public class SourceLightManager : MonoBehaviour
{
    protected bool state;

    [SerializeField] private bool startState;
    [SerializeField] private Button button;
    [SerializeField] private Color disabledColor;
    [SerializeField] private Color enabledColor;
    [SerializeField] private GameObject[] controllers;
    [SerializeField] private Light[] lights;
    [SerializeField] private MeshRenderer[] meshes;

    public bool State => state;

    private void Awake()
    {
        state = startState;
        Change(startState);
        if (button != null)
        {
            button.onClick.AddListener(ChangeLightState);
        }
    }

    private void ChangeLightState()
    {
        state = !state;

        Change(state);
    }

    private void Change(bool curState)
    {
        if (button != null)
        {
            ColorBlock colorBlock = button.colors;
            colorBlock.normalColor = state ? enabledColor : disabledColor;
            colorBlock.highlightedColor = colorBlock.normalColor;
            button.colors = colorBlock;
        }

        if (controllers != null)
        {
            for (int i = 0; i < controllers.Length; i++)
            {
                controllers[i].SetActive(curState);
            }
        }

        for (int i = 0; i < lights.Length; i++)
        {
            lights[i].enabled = curState;
        }

        for (int i = 0; i < meshes.Length; i++)
        {
            meshes[i].enabled = curState;
        }
    }
}