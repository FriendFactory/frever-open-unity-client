using System;
using System.Collections.Generic;
using System.Linq;
using Common;
using DG.Tweening;
using DigitalRubyShared;
using Extensions;
using Modules.LevelManaging.Assets;
using Modules.LevelManaging.Editing.LevelManagement;
using SharedAssetBundleScripts.Runtime.SetLocationScripts;
using UnityEngine;
using Zenject;

namespace UIManaging.Pages.VideoMessage.CharacterSizeManaging
{
    internal sealed class CharacterSizeController : MonoBehaviour
    {
        [SerializeField] private RectTransform _border;
        [SerializeField] private CharacterViewPortCorner[] _corners;
        [SerializeField] private CharacterViewProjection _characterViewProjection;
        [SerializeField] private CanvasGroup _canvasGroup;
        [Range(1, 1.4f)]
        [SerializeField] private float _extendBoxX = 1.15f;
        [Range(1, 1.4f)]
        [SerializeField] private float _extendBoxY = 1.15f;
        [SerializeField] private float _fadeDuration = 0.15f;
        [SerializeField] private RectTransform _renderViewPort;
        
        [Inject] private ILevelManager _levelManager;
        
        private ISetLocationAsset _setLocationAsset;
        private Vector2 _initialBorderSize;
        private float _initialPictureInPictureScale;
        private Vector2 _initialCharacterDistanceFromFootToCenter;
        private Vector2 _initialCharacterFootPoint;
        private readonly Vector3[] _cornersWorldPos = new Vector3[4];
        private readonly Vector2[] _cornersRectTransformLocalPosition = new Vector2[4];
        private RectTransform _borderParentRect;
        
        private Tween _currentAnimation;
        public bool IsShown { get; private set; }
        public bool IsChanging { get; private set; }

        private ISetLocationAsset SetLocationAsset => _setLocationAsset ??
                                                      (_setLocationAsset =
                                                          _levelManager.GetCurrentActiveSetLocationAsset());

        public RectTransform BorderRectTransform => _border;
        
        private PictureInPictureController PictureInPictureController => SetLocationAsset.PictureInPictureController;

        private Vector2 BorderMinSize => _initialBorderSize / _initialPictureInPictureScale * Constants.VideoMessage.SCALE_MIN;

        private RectTransform BorderParentRect
        {
            get
            {
                if (_borderParentRect == null)
                {
                    _borderParentRect = _border.parent.GetComponent<RectTransform>();
                }

                return _borderParentRect;
            }
        }
        
        private readonly Dictionary<CornerType, Vector2> _cornerToPivot = new Dictionary<CornerType, Vector2>()
        {
            { CornerType.BottomLeft, new Vector2(1, 1) },
            { CornerType.BottomRight, new Vector2(0, 1) },
            { CornerType.UpperLeft, new Vector2(1, 0) },
            { CornerType.UpperRight, new Vector2(0, 0) }
        };

        private readonly Dictionary<CornerType, CornerType> _oppositePairs = new Dictionary<CornerType, CornerType>()
        {
            { CornerType.BottomLeft, CornerType.UpperRight },
            { CornerType.UpperRight, CornerType.BottomLeft },
            { CornerType.UpperLeft, CornerType.BottomRight },
            { CornerType.BottomRight, CornerType.UpperLeft }
        };

        private void Awake()
        {
            foreach (var corner in _corners)
            {
                corner.Dragging += OnDragging;
            }
        }

        public void Show()
        {
            if (IsShown) return;
            Refresh();
            SubscribeToPictureController();

            IsShown = true;
            gameObject.SetActive(true);
            _canvasGroup.alpha = 0;
            _currentAnimation?.Kill();
            _currentAnimation = _canvasGroup.DOFade(1, _fadeDuration).OnComplete(() => _currentAnimation = null);
        }

        public void Hide()
        {
            if (!IsShown) return;

            UnsubscribeFromPictureController();
            _currentAnimation?.Kill();
            _currentAnimation = _canvasGroup.DOFade(0, _fadeDuration).OnComplete(()=>
            {
                _currentAnimation = null;
                gameObject.SetActive(false);
            });
            IsShown = false;
        }

        public void HideImmediate()
        {
            _currentAnimation?.Kill();
            _currentAnimation = null;
            UnsubscribeFromPictureController();
            gameObject.SetActive(false);
            IsShown = false;
        }
        
        private void Refresh()
        {
            _characterViewProjection.RectTransform.GetWorldCorners(_cornersWorldPos);
            
            for (int i = 0; i < _cornersWorldPos.Length; i++)
            {
                var viewPortPoint = PictureInPictureController.PictureInPictureCamera.WorldToViewportPoint(_cornersWorldPos[i]);
                var worldPos = _renderViewPort.GetWorldPositionFromNormalized(viewPortPoint);
                var screenPoint = RectTransformUtility.WorldToScreenPoint(null, worldPos);
                RectTransformUtility.ScreenPointToLocalPointInRectangle(BorderParentRect, screenPoint, null, out var localPos);
                _cornersRectTransformLocalPosition[i] = localPos;
            }
            
            var width = Vector3.Distance(_cornersRectTransformLocalPosition[2],  _cornersRectTransformLocalPosition[1]);
            var height = Vector3.Distance(_cornersRectTransformLocalPosition[2], _cornersRectTransformLocalPosition[3]);
            var size = new Vector2(width * _extendBoxX, height * _extendBoxY);
            _border.sizeDelta = size;
            
            SetPivot(Vector2.zero);
            
            var anchoredPos = _cornersRectTransformLocalPosition[0];
            var extendBorder = new Vector2((_extendBoxX - 1) * width, (_extendBoxY - 1) * height)/2f;
            var positionCorrection = Quaternion.Euler(extendBorder.x, extendBorder.y, PictureInPictureController.Rotation) * extendBorder;
            anchoredPos -= new Vector2(positionCorrection.x, positionCorrection.y);
            _border.anchoredPosition = anchoredPos;
            
            _border.SetEulerAngleZ(PictureInPictureController.Rotation);
        }

        private Vector2 GetCurrentCharacterFootViewPoint()
        {
            var worldPos = _characterViewProjection.transform.position;
            return _renderViewPort.GetComponent<RectTransform>()
                                             .InverseTransformPoint(worldPos);
        }

        private void OnDragging(ViewPortDragData data)
        {
            if (data.State == GestureRecognizerState.Began)
            {
                StoreInitialValues();
                UnsubscribeFromPictureController();
                IsChanging = true;
            }
            
            var corner = data.Corner;
            ChangeSizeOfBorders(data, corner);
            ScaleThePictureInPictureImage(corner);
            ApplyChangesToModel();
            
            if (data.State == GestureRecognizerState.Ended)
            {
                SubscribeToPictureController();
                IsChanging = false;
            }
        }

        private void StoreInitialValues()
        {
            _initialBorderSize = _border.sizeDelta;
            _initialPictureInPictureScale = PictureInPictureController.PictureSize;
            var initialPictureInPictureLocalPosition = PictureInPictureController.PictureLocalPosition;
            _initialCharacterFootPoint = GetCurrentCharacterFootViewPoint();
            _initialCharacterDistanceFromFootToCenter = initialPictureInPictureLocalPosition - _initialCharacterFootPoint;
        }

        private void ChangeSizeOfBorders(ViewPortDragData data, CharacterViewPortCorner corner)
        {
            SetPivot(_cornerToPivot[corner.CornerType]);
            var initialAspectRatio = _initialBorderSize.x / _initialBorderSize.y;
            
            var oppositeCornerType = _oppositePairs[corner.CornerType];
            var oppositeCornerPos = _corners.First(x => x.CornerType == oppositeCornerType).transform.localPosition;
            RectTransformUtility.ScreenPointToLocalPointInRectangle(_border, data.CurrentPosition, null,
                                                                    out var localTouchPos);
            var size = (localTouchPos - new Vector2(oppositeCornerPos.x, oppositeCornerPos.y)).Abs();
            
            AdjustVectorForAspectRatio(initialAspectRatio, ref size);
            if (size.x < BorderMinSize.x)
            {
                size = BorderMinSize;
            }
            _border.sizeDelta = size;
        }

        private void ScaleThePictureInPictureImage(CharacterViewPortCorner corner)
        {
            var nextSize = SetNewScale();
            AdjustPosition(corner, nextSize);
        }

        private void AdjustPosition(CharacterViewPortCorner corner, float nextSize)
        {
            var cornerForScalingType = _oppositePairs[corner.CornerType];

            var cornerForScaling = _corners.First(x => x.CornerType == cornerForScalingType);
            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                _renderViewPort,
                cornerForScaling.transform.position,
                null,
                out var pivot);
            var normalizedLocalPos = Rect.PointToNormalized(_renderViewPort.rect, pivot);
            var pivotWorldPos = PictureInPictureController.PictureCanvas.GetComponent<RectTransform>().GetWorldPositionFromNormalized(normalizedLocalPos);

            var pivotLocalPosition = PictureInPictureController.PictureCanvas.GetComponent<RectTransform>()
                                                               .InverseTransformPoint(pivotWorldPos);
            Vector2 nextLocalPosition = pivotLocalPosition;
            nextLocalPosition.x +=
                (_initialCharacterFootPoint.x - pivotLocalPosition.x) * nextSize / _initialPictureInPictureScale;
            ;
            nextLocalPosition.y +=
                (_initialCharacterFootPoint.y - pivotLocalPosition.y) * nextSize / _initialPictureInPictureScale;

            nextLocalPosition += _initialCharacterDistanceFromFootToCenter * nextSize / _initialPictureInPictureScale;
            PictureInPictureController.PictureLocalPosition = nextLocalPosition;
        }

        private float SetNewScale()
        {
            var nextSize = _initialPictureInPictureScale / _initialBorderSize.y * _border.rect.height;
            nextSize = Mathf.Clamp(nextSize, Constants.VideoMessage.SCALE_MIN, Constants.VideoMessage.SCALE_MAX);
            PictureInPictureController.PictureSize = nextSize;
            return nextSize;
        }

        private void ApplyChangesToModel()
        {
            var setLocationController = _levelManager.TargetEvent.GetSetLocationController();
            setLocationController.PictureInPictureSettings.Scale = PictureInPictureController.PictureSize;
            setLocationController.PictureInPictureSettings.Position =
                PictureInPictureController.PictureNormalizedPosition.ToVector2Dto();
        }

        private void SetPivot(Vector2 pivot)
        {
            _border.ChangePivotWithKeepingPosition(pivot);
        }

        private void AdjustVectorForAspectRatio(float targetAspectRatio, ref Vector2 originalVector)
        {
            var currentAspectRatio = originalVector.x / originalVector.y;

            if (Math.Abs(currentAspectRatio - targetAspectRatio) < 0.00001f) return;

            if (currentAspectRatio < targetAspectRatio)
                originalVector.x *= targetAspectRatio / currentAspectRatio;
            else
                originalVector.y *= currentAspectRatio / targetAspectRatio;
        }
        
        private void OnPicturePositionChangedOutside(Vector2 localPositionBefore, Vector2 newLocalPosition)
        {
            Refresh();
        }
        
        private void OnPicturePositionRotationChangedOutside(float angle)
        {
            Refresh();
        }

        private void OnPictureSizeChangedOutside(float deltaSize)
        {
            Refresh();
        }
        
        private void UnsubscribeFromPictureController()
        {
            PictureInPictureController.PositionChanged -= OnPicturePositionChangedOutside;
            PictureInPictureController.RotationChanged -= OnPicturePositionRotationChangedOutside;
            PictureInPictureController.SizeChanged -= OnPictureSizeChangedOutside;
        }
        
        private void SubscribeToPictureController()
        {
            PictureInPictureController.PositionChanged += OnPicturePositionChangedOutside;
            PictureInPictureController.RotationChanged += OnPicturePositionRotationChangedOutside;
            PictureInPictureController.SizeChanged += OnPictureSizeChangedOutside;
        }

        private void OnDestroy()
        {
            UnsubscribeFromPictureController();
        }
    }
}