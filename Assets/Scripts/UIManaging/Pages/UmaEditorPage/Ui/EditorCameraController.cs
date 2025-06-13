using System;
using UnityEngine;

namespace UIManaging.Pages.UmaEditorPage.Ui
{
    public sealed class EditorCameraController : MonoBehaviour
    {
        private const float MIN_RATIO = 1.78f;
        private const float MAX_RATIO = 2.16f;
        
        [SerializeField]
        private float _zoomSpeed = 2f;
        [SerializeField]
        private float _destZoom;
        [SerializeField]
        private Vector2 _rotationClamp;
        [SerializeField]
        private float _minZoom = 60f;
        [SerializeField]
        private float _maxZoom = 10f;

        public Action<float> ZoomChanged;

        public bool UseDest = false;

        private Transform Transform
        {
            get
            {
                if (!_transform)
                {
                    _transform = transform;
                }

                return _transform;
            }
        }
        
        private float RatioInterpolationCoefficient
        {
            get
            {
                if (_ratioInterpolationCoefficient <= 0f)
                {
                    float currentRatio = Screen.height / (float)Screen.width;
                    _ratioInterpolationCoefficient = Mathf.Clamp01((currentRatio - MIN_RATIO) / (MAX_RATIO - MIN_RATIO));
                }

                return _ratioInterpolationCoefficient;
            }
        }

        private float _currentZoom = 60;
        private Camera _camera;
        private RectTransform _rawImageRect;
        private float _ratioInterpolationCoefficient = -1f;

        private Transform _transform;
        private Vector3? _worldPoint = null;
        private Quaternion _targetRotation;
        private bool _pinchDisabled;
        private Vector3 _defaultCameraPosition;
        private Vector3 _targetCameraPosition;
        private bool _isMoving;

        //---------------------------------------------------------------------
        // Messages
        //---------------------------------------------------------------------

        private void Awake()
        {
            _camera = GetComponent<Camera>();
            _currentZoom = _camera.fieldOfView;
            _destZoom = _currentZoom;
            _defaultCameraPosition = Transform.localPosition;
        }

        private void Update()
        {
            if (UseDest && Math.Abs(_destZoom - _currentZoom) > 0.0001f)
            {
                _currentZoom += (_destZoom - _currentZoom) * Time.deltaTime * _zoomSpeed;
                if (Mathf.Abs(_currentZoom - _destZoom) < 0.05f) _currentZoom = _destZoom;
                SetNewFoV();
            }
#if UNITY_EDITOR
            if (Input.mouseScrollDelta.y != 0)
            {
                UseDest = false;
                var touchesMidPoint = Input.mousePosition;
                RectTransformUtility.ScreenPointToLocalPointInRectangle(_rawImageRect, touchesMidPoint, null, out var _point);
                var xWithCorrection = _point.x / _rawImageRect.rect.width + _rawImageRect.pivot.x;
                var yWithCorrection = _point.y / _rawImageRect.rect.height + _rawImageRect.pivot.y;
                _currentZoom += Input.mouseScrollDelta.y * Time.deltaTime * _zoomSpeed;
                _currentZoom = Mathf.Clamp(_currentZoom, _maxZoom, _minZoom);

                var viewportPoint = new Vector3(xWithCorrection, yWithCorrection, 10);
                _worldPoint = _camera.ViewportToWorldPoint(viewportPoint);
                var eulerAngles = Quaternion.LookRotation(_worldPoint.Value - Transform.position).eulerAngles;
                var xRotation = eulerAngles.x > 180 ? (eulerAngles.x - 360) : eulerAngles.x;
                var yRotation = eulerAngles.y > 180 ? (eulerAngles.y - 360) : eulerAngles.y;

                xRotation = Mathf.Clamp(xRotation, -_rotationClamp.x, _rotationClamp.x);
                yRotation = Mathf.Clamp(yRotation, -_rotationClamp.y, _rotationClamp.y);
                SetTargetRotation(Quaternion.Euler(xRotation, yRotation, 0));
                SetNewFoV();
            }
#endif
            if (Input.touchCount >= 2)
            {
                PinchZooming();
                UseDest = false;
            }
            else if (_worldPoint != null)
            {
                _worldPoint = null;
            }
            SetCameraPosition();
            SetRotation();
        }

        //---------------------------------------------------------------------
        // Public
        //---------------------------------------------------------------------
        public void SetDestination(EditorZoomingPreset preset)
        {
            if (preset == null || !Transform.gameObject.activeSelf) return;
    
            UseDest = true;
            _destZoom = preset.GetInterpolatedZoomValue(RatioInterpolationCoefficient);
            _targetCameraPosition = _defaultCameraPosition + preset.GetInterpolatedCameraOffset(RatioInterpolationCoefficient);
            _isMoving = true;
    
            // Calculate the desired world position using parent's transformation
            Vector3 desiredWorldPosition = Transform.parent != null 
                ? Transform.parent.TransformPoint(_targetCameraPosition) 
                : _targetCameraPosition;

            if (preset.ZoomingTarget)
            {
                SetTargetRotation(Quaternion.LookRotation(preset.TargetPosition - desiredWorldPosition));
            }
        }

        public void Setup(RectTransform rectTransform, bool disablePinchZoom = false)
        {
            _rawImageRect = rectTransform;
            _pinchDisabled = disablePinchZoom;
            ResetZoom();
        }

        public void ResetZoom()
        {
            if (_camera == null) return;
            UseDest = true;
            Transform.localRotation = Quaternion.identity;
            _currentZoom = _minZoom;
            SetNewFoV();
        }

        public void SetZoom(float value)
        {
            _destZoom = value;
            UseDest = true;
            if (value == _minZoom)
            {
                SetTargetRotation(Quaternion.identity);
            }
        }

        public void SetPositionSilent(EditorZoomingPreset preset)
        {
            if (preset == null) return;
            if (_camera == null) _camera = GetComponent<Camera>();

            _currentZoom = preset.GetInterpolatedZoomValue(RatioInterpolationCoefficient);
            SetNewFoV();
        
            Vector3 newPosition = _defaultCameraPosition + preset.GetInterpolatedCameraOffset(RatioInterpolationCoefficient);
            Transform.localPosition = newPosition;
            _targetCameraPosition = newPosition;
            _isMoving = false;
        
            // Calculate rotation from the final position
            Vector3 worldPosition = Transform.position;
            SetTargetRotation(Quaternion.LookRotation(preset.TargetPosition - worldPosition));
            SetRotation();
        }

        //---------------------------------------------------------------------
        // Private
        //---------------------------------------------------------------------
        private void PinchZooming()
        {
            if (_pinchDisabled) return;

            var touchZero = Input.GetTouch(0);
            var touchOne = Input.GetTouch(1);

            var zeroPrevPos = touchZero.position - touchZero.deltaPosition;
            var onePrevPos = touchOne.position - touchOne.deltaPosition;

            var prevDistance = Vector2.Distance(zeroPrevPos, onePrevPos);
            var currentDistance = Vector2.Distance(touchZero.position, touchOne.position);

            var touchesMidPoint = (touchZero.position + touchOne.position) / 2;
            RectTransformUtility.ScreenPointToLocalPointInRectangle(_rawImageRect, touchesMidPoint, null, out var _point);
            if (!CheckPointOnRectTransform(_point)) return;

            _currentZoom += (prevDistance - currentDistance) * Time.deltaTime * _zoomSpeed;
            _currentZoom = Mathf.Clamp(_currentZoom, _maxZoom, _minZoom);

            if (_worldPoint == null)
            {
                var xWithCorrection = _point.x / _rawImageRect.rect.width + _rawImageRect.pivot.x;
                var yWithCorrection = _point.y / _rawImageRect.rect.height + _rawImageRect.pivot.y;
                var viewportPoint = new Vector3(xWithCorrection, yWithCorrection, 10);
                _worldPoint = _camera.ViewportToWorldPoint(viewportPoint);

                var eulerAngles = Quaternion.LookRotation(_worldPoint.Value - Transform.position).eulerAngles;
                var xRotation = eulerAngles.x > 180 ? (eulerAngles.x - 360) : eulerAngles.x;
                var yRotation = eulerAngles.y > 180 ? (eulerAngles.y - 360) : eulerAngles.y;

                xRotation = Mathf.Clamp(xRotation, -_rotationClamp.x, _rotationClamp.x);
                yRotation = Mathf.Clamp(yRotation, -_rotationClamp.y, _rotationClamp.y);
                SetTargetRotation(Quaternion.Euler(xRotation, yRotation, 0));

            }
            SetNewFoV();
        }

        private void SetNewFoV()
        {
            _camera.fieldOfView = _currentZoom;
            ZoomChanged?.Invoke(_currentZoom);
        }

        private void SetRotation()
        {
            // Adjust rotation interpolation to account for camera position
            var t = 1 - (_currentZoom - _maxZoom) / (_minZoom - _maxZoom);
        
            // Calculate the base rotation considering current position
            var currentPos = Transform.position;
            var targetDir = _worldPoint.HasValue ? 
                (_worldPoint.Value - currentPos).normalized : 
                Transform.forward;
            
            var desiredRotation = Quaternion.LookRotation(targetDir);
        
            // Interpolate between identity and desired rotation
            var minRotation = Quaternion.Lerp(Quaternion.identity, desiredRotation, t);
            var finalRotation = Quaternion.Lerp(minRotation, _targetRotation, t);
        
            Transform.localRotation = finalRotation;
        }

        private void SetCameraPosition()
        {
            if (_isMoving)
            {
                var currentPosition = Transform.localPosition;
                Transform.localPosition = Vector3.Lerp(currentPosition, _targetCameraPosition, Time.deltaTime * _zoomSpeed);
            
                if (Vector3.Distance(Transform.localPosition, _targetCameraPosition) < 0.001f)
                {
                    Transform.localPosition = _targetCameraPosition;
                    _isMoving = false;
                }
            }
        }

        private bool CheckPointOnRectTransform(Vector2 point)
        {
            var rect = _rawImageRect.rect;
            return point.x <= rect.xMax && point.x >= rect.x && point.y <= rect.yMax && point.y >= rect.y;
        }

        private void SetTargetRotation(Quaternion rotation)
        {
            _targetRotation = rotation;
        }
    }
}