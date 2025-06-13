using System;
using UnityEngine.XR.ARFoundation;

namespace Modules.FaceAndVoice.Face.Facade
{
    internal sealed class ARFoundationSessionManager: IArSessionManager
    {
        public bool IsActive => _arSession.enabled && _arCameraManager.enabled && _arFaceManager.enabled;
        public event Action<bool> StateSwitched;
        
        private readonly ARSession _arSession;
        private readonly ARCameraManager _arCameraManager;
        private readonly ARFaceManager _arFaceManager;

        public ARFoundationSessionManager(ARSession arSession, ARCameraManager cameraManager, ARFaceManager faceManager)
        {
            _arSession = arSession;
            _arCameraManager = cameraManager;
            _arFaceManager = faceManager;
            _arSession.attemptUpdate = true;

            if (arSession.enabled != cameraManager.enabled)
                throw new InvalidOperationException(
                    $"{nameof(arSession)} and {nameof(cameraManager)} must have the same Component.enabled state on startup");
        }

        public void SetARActive(bool isActive)
        {
            if(IsActive == isActive) return;
            SetState(isActive);
            StateSwitched?.Invoke(isActive);
        }

        private void SetState(bool isActive)
        {
            _arSession.enabled = isActive;
            _arCameraManager.enabled = isActive;
            _arFaceManager.enabled = isActive;
            _arFaceManager.SetTrackablesActive(isActive);
        }
    }
}