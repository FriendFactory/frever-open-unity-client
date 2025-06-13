using System;
using SwiftPlugin.Source;
using UnityEngine.XR.ARFoundation;

namespace Modules.FaceAndVoice.Face.Facade
{
    /// <summary>
    /// Used for devices which does not support true depth camera
    /// </summary>
    internal sealed class NoTrueDepthARSessionManager: IArSessionManager
    {
        public bool IsActive { get; private set; }
        public event Action<bool> StateSwitched;

        private bool _arCoreInitialized;
        private readonly ARCameraManager _arCameraManager;

        public NoTrueDepthARSessionManager(ARCameraManager arCameraManager)
        {
            _arCameraManager = arCameraManager;
        }
        
        public void SetARActive(bool isActive)
        {
            if(IsActive == isActive) return;
            IsActive = isActive;
            if (isActive && !_arCoreInitialized)
            {
                SwiftForUnity.viewDidLoad();
                _arCoreInitialized = true;
            }
            StateSwitched?.Invoke(true);
        }
    }
}