using UnityEngine;

namespace Modules.CameraSystem.PlayerCamera
{
    /// <summary>
    ///     During switching of anchors(character spawn points) we need to adapt free look camera to new relative angle,
    ///     especially in cases when LookAt is Off.
    ///     We store delta rotation between previous and next anchors, and then apply it to CinemachineFreeLook transform,
    ///     what allows us to keep the same relative rotation for the next anchor
    /// </summary>
    internal sealed class CinemachineRelativeRotationKeeper
    {
        private Transform _cinemachineTransform;
        private Vector3? _anchorsDeltaRotation;

        public void SetCinemachineTransform(Transform cinemachineTransform)
        {
            _cinemachineTransform = cinemachineTransform;
        }

        public void TrackSwitchedAnchors(Transform previousAnchor, Transform nextAnchor)
        {
            if (previousAnchor == null || nextAnchor == null)
            {
                _anchorsDeltaRotation = null;
                return;
            }

            _anchorsDeltaRotation = nextAnchor.eulerAngles - previousAnchor.eulerAngles;
        }

        public void AdaptRotationToNewAnchor()
        {
            if (!_anchorsDeltaRotation.HasValue) return;

            _cinemachineTransform.eulerAngles += _anchorsDeltaRotation.Value;
            _anchorsDeltaRotation = null;
        }
    }
}