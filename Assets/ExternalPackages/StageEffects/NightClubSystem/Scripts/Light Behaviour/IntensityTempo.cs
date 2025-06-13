using UnityEngine;
using UnityEngine.UI;

public class IntensityTempo : MonoBehaviour
{
    private const float RefValue = 0.1f;
    private const float MinDB = -10.0f;
    private const float MaxDB = 18.0f;

    private float sum;
    private float[] firstFrames;
    private float[] secondFrames;
    private bool isReaction;

    [Header("Main")]
    [SerializeField] private SourceLightManager sourceLightManager;

    [Header("Button")]
    [SerializeField] private Button button;
    [SerializeField] private Color disabledColor;
    [SerializeField] private Color enabledColor;

    [Header("Audio")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private float minVolumeToReact;
    [SerializeField] private float maxIntensity;
    [SerializeField] private bool startState;

    [Header("Object")]
    [SerializeField] private Light[] lightSources;
    [SerializeField] private MeshRenderer[] meshes;

    private void Start()
    {
        if (button != null)
        {
            isReaction = startState;
            button.onClick.AddListener(ChangeState);
            ChangeButtonColor();
        }

        firstFrames = new float[1024];
        secondFrames = new float[1024];
    }

    private void Update()
    {
        if (!isReaction) return;

        audioSource.GetOutputData(firstFrames, 0);
        audioSource.GetOutputData(secondFrames, 1);

        for (int i = 0; i < firstFrames.Length; i++)
        {
            firstFrames[i] += secondFrames[i];
        }

        var volume = GetVolume(firstFrames);

        SetIntensity(volume);
        ChangeMeshRenderer(volume);
    }

    private void SetIntensity(float volume)
    {
        for (int i = 0; i < lightSources.Length; i++)
        {
            var light = lightSources[i];
            bool isReact = volume > minVolumeToReact;

            light.enabled = isReact;

            if (isReact)
            {
                light.intensity = volume;
                light.intensity = Mathf.Clamp(light.intensity, 0, maxIntensity);
            }
        }
    }

    private void ChangeMeshRenderer(float volume)
    {
        ChangeMeshes(volume > minVolumeToReact);
    }
    
    private float GetVolume(float[] data) 
    {
        sum = 0;

        for (int i = 0; i < data.Length; i++)
        {
            sum += (data[i] * data[i]) / 2f;
        }

        float rmsValue = Mathf.Sqrt(sum/data.Length);
        float volume = 20.0f * Mathf.Log10(rmsValue/RefValue);
        
        if (volume < MinDB)
        {
            volume = MinDB;
        }

        if (volume > MaxDB)
        {
            volume = MaxDB;
        }

        volume -= MinDB;

        return (maxIntensity * volume) / (MaxDB - MinDB) - 1;
    }

    private void ChangeState()
    {
        isReaction = !isReaction;
        ChangeButtonColor();

        if (sourceLightManager == null)
        {
            ChangeLights(true);
            ChangeMeshes(true);
        }
        else
        {
            if (!isReaction)
            {
                if (sourceLightManager.State)
                {
                    ChangeLights(true);
                    ChangeMeshes(true);
                }
            }
        }
    }

    private void ChangeLights(bool state)
    {
        for (int i = 0; i < lightSources.Length; i++)
        {
            lightSources[i].enabled = state;
        }
    }

    private void ChangeMeshes(bool state)
    {
        for (int i = 0; i < meshes.Length; i++)
        {
            meshes[i].enabled = state;
        }
    }

    private void ChangeButtonColor()
    {
        ColorBlock colorBlock = button.colors;
        colorBlock.normalColor = isReaction ? enabledColor : disabledColor;
        colorBlock.highlightedColor = colorBlock.normalColor;
        button.colors = colorBlock;
    }
}