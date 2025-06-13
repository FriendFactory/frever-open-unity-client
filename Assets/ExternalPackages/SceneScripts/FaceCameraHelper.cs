using UnityEngine;

internal sealed class FaceCameraHelper : MonoBehaviour
{
    private Camera _camera;

    void Update()
    {
        transform.LookAt(GetCamera().transform);
    }

    private Camera GetCamera()
    {
        if (_camera == null)
        {
            _camera = Camera.main;
        }

        return _camera;
    }
}
