#if UNITY_IOS && !UNITY_EDITOR
using System;
using UnityEngine.XR.ARKit;

namespace ARKit
{
    public class FreverARKitSessionDelegate : DefaultARKitSessionDelegate
    {
        private readonly Action _errorCallback;
        
        public FreverARKitSessionDelegate(Action errorCallback)
        {
            _errorCallback = errorCallback;
        }

        protected override void OnSessionDidFailWithError(ARKitSessionSubsystem sessionSubsystem, NSError error)
        {
            _errorCallback?.Invoke();
        }
    }
}
#endif