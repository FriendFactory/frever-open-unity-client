using UnityEngine;

// Behavior used to initialise abd release the PVRTC compute manager
public class PVRTCComputeBehaviour : MonoBehaviour
{
    public ComputeShader shader;
    
    void Start()
    {
        PVRTCComputeManager.Initialise(shader);
    }

    void OnDestroy()
    {
        PVRTCComputeManager.Dispose();
    }
}
