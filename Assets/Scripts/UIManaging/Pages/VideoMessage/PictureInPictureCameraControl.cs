using System.Linq;
using Common;
using DigitalRubyShared;
using Extensions;
using JetBrains.Annotations;
using Models;
using Modules.LevelManaging.Assets;
using Modules.LevelManaging.Editing.LevelManagement;
using SharedAssetBundleScripts.Runtime.SetLocationScripts;
using UIManaging.Pages.VideoMessage.CharacterSizeManaging;
using UnityEngine;

namespace UIManaging.Pages.VideoMessage
{
    [UsedImplicitly]
    internal sealed class PictureInPictureCameraControl
    {
        private const float SCALE_SENSITIVITY = 2;

        private readonly ILevelManager _levelManager;
        private readonly FingersScript _fingersScript;
        private readonly IVideoMessagePageGesturesControl _videoMessagePageGesturesControl;

        private SetLocationController SetLocationController => _levelManager.TargetEvent.GetSetLocationController();

        private PictureInPictureController PictureInPictureController => _setLocationAsset.PictureInPictureController;
        private ISetLocationAsset _setLocationAsset;
        private CharacterSizeController _characterSizeController;
        private RectTransform _border;
        private RectTransform _viewPort;
        private bool _enabled;
        private bool _panGestureStartedOverPictureInPicture;

        public PictureInPictureCameraControl(ILevelManager levelManager, FingersScript fingersScript, IVideoMessagePageGesturesControl videoMessagePageGesturesControl)
        {
            _levelManager = levelManager;
            _fingersScript = fingersScript;
            _videoMessagePageGesturesControl = videoMessagePageGesturesControl;
        }
        
        public void Init(ISetLocationAsset setLocationAsset, CharacterSizeController characterSizeController, RectTransform border, RectTransform viewPort)
        {
            _setLocationAsset = setLocationAsset;
            _characterSizeController = characterSizeController;
            _border = border;
            _viewPort = viewPort;

            var pictureGameObject = PictureInPictureController.PictureRectTransform.gameObject;
            _fingersScript.PassThroughObjects.Add(pictureGameObject);
            pictureGameObject.AddListenerToDestroyEvent(()=> _fingersScript.PassThroughObjects.Remove(pictureGameObject));
        }

        public void Enable()
        {
            if (_enabled) return;

            _enabled = true;

            _videoMessagePageGesturesControl.PanGestureStateChanged += OnPanGestureStateUpdated;
            _videoMessagePageGesturesControl.RotationExecuted += OnRotationGestureExecuted;
            _videoMessagePageGesturesControl.ZoomGestureStateChanged += OnZoomStateUpdated;
        }

        public void Disable()
        {
            if (!_enabled) return;
            
            _videoMessagePageGesturesControl.PanGestureStateChanged -= OnPanGestureStateUpdated;
            _videoMessagePageGesturesControl.RotationExecuted -= OnRotationGestureExecuted;
            _videoMessagePageGesturesControl.ZoomGestureStateChanged -= OnZoomStateUpdated;
            
            _enabled = false;
        }

        public void Cleanup()
        {
            Disable();
            _setLocationAsset = null;
        }

        private void OnPanGestureStateUpdated(GestureRecognizer gesture)
        {
            if (_characterSizeController.IsChanging) return;
            if (gesture.State != GestureRecognizerState.Executing && gesture.State != GestureRecognizerState.Began) return;
         
            if (gesture.CurrentTrackedTouches.Count > 1) return;
            var touch = gesture.CurrentTrackedTouches.First();
            
            if (gesture.State == GestureRecognizerState.Began)
            {
                _panGestureStartedOverPictureInPicture = IsOverCharacter(touch);
                return;
            }
            
            if (!_panGestureStartedOverPictureInPicture) return;
            var currentPosition = new Vector2(touch.X, touch.Y);
            var previousPosition = new Vector2(touch.PreviousX, touch.PreviousY);
            
            RectTransformUtility.ScreenPointToLocalPointInRectangle(_viewPort, previousPosition, null, out var previousCanvasTouchPosition);
            RectTransformUtility.ScreenPointToLocalPointInRectangle(_viewPort, currentPosition, null, out var currentCanvasTouchPosition);

            var normalizedDelta = Rect.PointToNormalized(_viewPort.rect, currentCanvasTouchPosition) -
                                  Rect.PointToNormalized(_viewPort.rect, previousCanvasTouchPosition);

            PictureInPictureController.PictureNormalizedPosition += normalizedDelta;
            SetLocationController.PictureInPictureSettings.Position = PictureInPictureController.PictureNormalizedPosition.ToVector2Dto();
        }
        
        private void OnZoomStateUpdated(GestureRecognizer gesture)
        {
            if (gesture.State != GestureRecognizerState.Executing) return;

            var scaleRecognizer = gesture as ScaleGestureRecognizer;
            var previousSize = PictureInPictureController.PictureSize;
            var nextSize = previousSize + scaleRecognizer.ScaleDistanceDelta * SCALE_SENSITIVITY;
            PictureInPictureController.PictureSize = Mathf.Clamp(nextSize, Constants.VideoMessage.SCALE_MIN, Constants.VideoMessage.SCALE_MAX);
            SetLocationController.PictureInPictureSettings.Scale = PictureInPictureController.PictureSize;
        }

        private void OnRotationGestureExecuted(float deltaDegrees)
        {
            PictureInPictureController.Rotation += deltaDegrees;
            var setLocationController = _levelManager.TargetEvent.GetSetLocationController();
            setLocationController.PictureInPictureSettings.Rotation = PictureInPictureController.Rotation;
        }

        private bool IsOverCharacter(GestureTouch touch)
        {
            var screenPos = new Vector2(touch.X, touch.Y);
            return RectTransformUtility.RectangleContainsScreenPoint(_border, screenPos);
        }
    }
}