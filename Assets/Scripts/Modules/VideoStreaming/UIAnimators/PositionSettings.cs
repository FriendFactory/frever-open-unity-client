using System;
using UnityEngine;

namespace UI.UIAnimators
{
    [Serializable]
    public struct PositionSettings
    {
        [SerializeField] private Vector2 anchoredPosition;
        [SerializeField] private Vector2 anchorMin;
        [SerializeField] private Vector2 anchorMax;
        [SerializeField] private Vector2 pivot;

        public Vector2 AnchoredPosition => anchoredPosition;
        public Vector2 AnchorMin => anchorMin;
        public Vector2 AnchorMax => anchorMax;
        public Vector2 Pivot => pivot;

        public PositionSettings(Vector2 anchoredPosition, Vector2 anchorMin, Vector2 anchorMax, Vector2 pivot)
        {
            this.anchoredPosition = anchoredPosition;
            this.anchorMin = anchorMin;
            this.anchorMax = anchorMax;
            this.pivot = pivot;
        }

        public override string ToString()
        {
            return $"anchoredPosition: {anchoredPosition}, anchorMin: {anchorMin}, anchorMax: {anchorMax}, pivot: {pivot}";
        }
    }
}
