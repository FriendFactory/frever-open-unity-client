using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Common.UI
{
    [RequireComponent(typeof(RectTransform))]
    internal sealed class ScreenAspectRatioBasedPositionCorrector : MonoBehaviour
    {
        [SerializeField]
        private List<PositionCorrectionData> _positionCorrectionData = new List<PositionCorrectionData>();

        private void Start()
        {
            if (_positionCorrectionData == null) return;
            
            var aspectRatio = Screen.width / (float)Screen.height;
            var data = _positionCorrectionData.FirstOrDefault(x => Math.Abs(x.AspectRatio - aspectRatio) < 0.001);
            if (data == null) return;
            var rectTransform = transform as RectTransform;
            rectTransform.anchoredPosition += data.AdjustPosition;
        }

        [Serializable]
        private sealed class PositionCorrectionData
        {
            public int ScreenRelativeWidth;
            public int ScreenRelativeHeight;
            public Vector2 AdjustPosition;

            public float AspectRatio => ScreenRelativeWidth / (float)ScreenRelativeHeight;
        }
    }
}
