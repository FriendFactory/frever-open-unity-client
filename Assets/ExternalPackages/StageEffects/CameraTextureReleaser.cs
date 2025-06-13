using UnityEngine;

namespace Modules.LevelManaging.FullPreview.Assets
{
    public class CameraTextureReleaser : MonoBehaviour
    {
        private Camera _camera;

        private void Start()
        {
            _camera = GetComponent<Camera>();
        }

        private void OnDisable()
        {
            if (_camera == null || _camera.targetTexture == null) return;
            
                _camera.targetTexture = null;
        }
    }
}