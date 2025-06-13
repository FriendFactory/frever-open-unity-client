using UnityEngine;

namespace Modules.CameraSystem.PlayerCamera
{
    internal sealed class CameraFocusTargetAdjuster : MonoBehaviour
    {
        private Transform _target;
        private Transform _adjustedTarget;
        
        public Transform Target
        {
            get => _target;
            set
            {
                if (_target == value) return;
                _target = value;
                _adjustedTarget = _adjustedTarget != null? _adjustedTarget : new GameObject("Adjusted Target").transform;
                _adjustedTarget.SetParent(_target);
                _adjustedTarget.localPosition = Vector3.zero;
                _adjustedTarget.localRotation = Quaternion.identity;
                UpdatePosition();
            }
        }

        public Transform CameraTransform { get; set; }

        public AnimationCurve AdjustPositionCurve { get; set; }

        public Transform AdjustedTarget
        {
            get
            {
                UpdatePosition();
                return _adjustedTarget;
            }
        }

        public void UpdatePosition()
        {
            if (Target == null || CameraTransform == null || AdjustPositionCurve == null) return;
            var distanceToCamera = Vector3.Distance(CameraTransform.position, Target.position);
            var adjustedLocalPos = AdjustPositionCurve.Evaluate(distanceToCamera) * Vector3.up;
            _adjustedTarget.localPosition = adjustedLocalPos;
        }
    }
}