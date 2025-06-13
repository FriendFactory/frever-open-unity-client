using UnityEngine;

#if UNITY_IOS
using System.Collections.Generic;
using UnityEngine.iOS;
#endif

namespace Modules.LevelViewPort
{
    internal sealed class UiSafeAreaFitter : MonoBehaviour
    {
        [SerializeField] private bool _runOnEnable;
        private readonly Vector3[] _worldCorners = new Vector3[4];
        
        private void OnEnable()
        {
            if (!_runOnEnable) return;
            Refresh();
        }
        
        public void Refresh()
        {
            var rectTransform = GetComponent<RectTransform>();

            rectTransform.GetWorldCorners(_worldCorners);
            var rectScreenPosMax = RectTransformUtility.WorldToScreenPoint(null, _worldCorners[2]);
            var rectScreenPosMin = RectTransformUtility.WorldToScreenPoint(null, _worldCorners[0]);

            var isInsideSafeArea = rectScreenPosMin.y > Screen.safeArea.yMin && rectScreenPosMax.y < Screen.safeArea.yMax;
            if (isInsideSafeArea) return;

            rectTransform.position -= new Vector3(0, rectScreenPosMax.y - Screen.safeArea.yMax, 0);
            
            #if UNITY_IOS
            AdjustPositionOnIOS(rectTransform);
            #endif
        }

        #if UNITY_IOS
        private void AdjustPositionOnIOS(Transform rectTransform)
        {
            if (_iphoneSafeAreaAxisYAdjustments.TryGetValue(Device.generation, out var adjustmentY))
            {
                rectTransform.position += new Vector3(0, adjustmentY, 0);
            }
        }

        private readonly Dictionary<DeviceGeneration, int> _iphoneSafeAreaAxisYAdjustments = new()
            {
                { DeviceGeneration.iPhoneX, 40 },
                { DeviceGeneration.iPhoneXR, 40 },
                { DeviceGeneration.iPhoneXS, 40 },
                { DeviceGeneration.iPhoneXSMax, 40 },
                { DeviceGeneration.iPhone11, 40 },
                { DeviceGeneration.iPhone12, 40 },
                { DeviceGeneration.iPhone12Pro, 40 },
                { DeviceGeneration.iPhone12ProMax, 40 },
                { DeviceGeneration.iPhone12Mini, 50 },
            };
        #endif
    }
}