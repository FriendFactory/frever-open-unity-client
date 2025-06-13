using UnityEngine;

namespace Modules.LevelManaging.Assets.Caption
{
    /// <summary>
    /// Sets proper plane distance for different FoV values to fix rendering world canvas problem
    /// when text was disappeared if the plane distance and FoV both are small at the same time
    /// </summary>
    internal sealed class CaptionCanvasPlaneDistanceControl: MonoBehaviour
    {
        [SerializeField] private AnimationCurve _fovToPlaneDistance;
        [SerializeField] private Canvas _canvas;
        private Camera _camera;

        private bool _hasCamera;

        public void SetCamera(Camera cam)
        {
            _camera = cam;
            _hasCamera = cam != null;
        }

        private void Update()
        {
            if (!_hasCamera) return;

            _canvas.planeDistance = _fovToPlaneDistance.Evaluate(_camera.fieldOfView);
        }
    }
}