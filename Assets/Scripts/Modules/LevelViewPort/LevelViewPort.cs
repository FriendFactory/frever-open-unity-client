using System;
using Common;
using Extensions;
using Modules.RenderingPipelineManagement;
using UnityEngine;
using UnityEngine.Experimental.Rendering;
using UnityEngine.UI;
using Zenject;

namespace Modules.LevelViewPort
{
    public sealed class LevelViewPort: MonoBehaviour
    {
        private const float MAX_DYNAMIC_RATIO = 0.4864f;

        [SerializeField] private RawImage _rawImage;
        [SerializeField] private AspectRatioFitter _aspectRatioFitter;
        [SerializeField] private UiSafeAreaFitter _safeAreaFitter;

        [Inject] private IRenderingPipelineManager _renderingPipelineManager;

        private RenderTexture _renderTexture;
        private RectTransform _rectTransform;
        private RectTransform _parentRectTransform;
        private float _forcedAspectRatio;

        public event Action Initialized;

        //---------------------------------------------------------------------
        // Properties
        //---------------------------------------------------------------------

        public bool IsFullScreen { get; private set; }
        
        public RenderTexture RenderTexture
        {
            get
            {
                if (_renderTexture == null)
                {
                    var resolution = new Vector2(Screen.width, Screen.width / GetTargetAspectRatio());
                    _renderTexture = new RenderTexture((int) resolution.x, (int) resolution.y, 24, GraphicsFormat.R16G16B16A16_SFloat);
                }

                return _renderTexture;
            }
        }

        public RectTransform RectTransform
        {
            get
            {
                if (_rectTransform == null)
                {
                    _rectTransform = GetComponent<RectTransform>();
                }
                return _rectTransform;
            }
        }

        public AspectRatioFitter AspectRatioFitter
        {
            get
            {
                if (_aspectRatioFitter == null)
                {
                    _aspectRatioFitter = GetComponent<AspectRatioFitter>();
                }
                return _aspectRatioFitter;
            }
        }

        private RectTransform ParentRectTransform
        {
            get
            {
                if (_parentRectTransform == null)
                {
                    _parentRectTransform = RectTransform.parent.GetComponent<RectTransform>();
                }
                return _parentRectTransform;
            }
        }

        private UiSafeAreaFitter SafeAreaFitter
        {
            get
            {
                if (_safeAreaFitter == null)
                {
                    _safeAreaFitter = GetComponent<UiSafeAreaFitter>();
                }

                return _safeAreaFitter;
            }
        }

        //---------------------------------------------------------------------
        // Public
        //---------------------------------------------------------------------
        
        public void Init()
        {
            _rawImage.texture = RenderTexture;
            AspectRatioFitter.aspectRatio = GetTargetAspectRatio();

            SafeAreaFitter?.Refresh();
            Initialized?.Invoke();
        }

        public void Init(Vector2Int resolution)
        {
            _renderTexture = new RenderTexture(resolution.x, resolution.y, 24, GraphicsFormat.R16G16B16A16_SFloat);
            _forcedAspectRatio = resolution.x / (float) resolution.y;

            Init();
        }

        //---------------------------------------------------------------------
        // Events
        //---------------------------------------------------------------------
        
        private void OnDestroy()
        {
            if (_renderTexture == null) return;

            _renderTexture.Release();
            Destroy(_renderTexture);
        }

        //---------------------------------------------------------------------
        // Helpers
        //---------------------------------------------------------------------

        private float GetTargetAspectRatio()
        {
            // Forced aspect ratio used for video rendering
            if (_forcedAspectRatio > 0) return _forcedAspectRatio;

            // Dynamic aspect ratio used for UI when screen ratio is close to 9:16
            var parentAspectRatio = ParentRectTransform.GetAspectRatio();
            if (parentAspectRatio < MAX_DYNAMIC_RATIO)
            {
                IsFullScreen = false;
                return Constants.VideoRenderingResolution.PORTRAIT_ASPECT;
            }

            IsFullScreen = true;
            return parentAspectRatio;
        }
    }
}