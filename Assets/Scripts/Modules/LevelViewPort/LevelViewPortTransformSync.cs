using Extensions;
using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace Modules.LevelViewPort
{
    [RequireComponent(typeof(RectTransform))]
    internal sealed class LevelViewPortTransformSync : MonoBehaviour
    {
        private const float BOTTOM_UI_OFFSET = 250f;

        private LevelViewPort _levelViewPort;
        private RectTransform _rectTransform;
        private AspectRatioFitter _aspectRatioFitter;

        //---------------------------------------------------------------------
        // Properties
        //---------------------------------------------------------------------

        private RectTransform RectTransform
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

        private AspectRatioFitter AspectRatioFitter
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


        //---------------------------------------------------------------------
        // Ctors
        //---------------------------------------------------------------------
        
        [Inject, UsedImplicitly]
        public void Construct(LevelViewPort levelViewPort)
        {
            _levelViewPort = levelViewPort;
            _levelViewPort.Initialized += SyncTransform;
        }

        //---------------------------------------------------------------------
        // Messages
        //---------------------------------------------------------------------

        private void OnDestroy()
        {
            _levelViewPort.Initialized -= SyncTransform;
        }

        //---------------------------------------------------------------------
        // Helpers
        //---------------------------------------------------------------------

        private void SyncTransform()
        {
            _levelViewPort.RectTransform.CopyProperties(RectTransform);
            AspectRatioFitter.aspectRatio = _levelViewPort.AspectRatioFitter.aspectRatio;

            if (!_levelViewPort.IsFullScreen) return;

            var height = RectTransform.GetHeight() - BOTTOM_UI_OFFSET;
            RectTransform.SetLocalPositionY((BOTTOM_UI_OFFSET - height) / 2f);
            AspectRatioFitter.aspectRatio =  RectTransform.GetWidth() / height;
        }
    }
}