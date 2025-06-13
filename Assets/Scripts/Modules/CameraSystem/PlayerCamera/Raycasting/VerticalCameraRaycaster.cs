
using UnityEngine;

namespace Modules.CameraSystem.PlayerCamera.Raycasting
{
    public class VerticalCameraRaycaster : CameraRaycaster
    {
        public Vector3 RootPosition { get; set; }
        protected override RaycastDirectionType DirectionType => RaycastDirectionType.Vertical;
        protected override float DistanceOnHit(Vector3 hitPos, Vector3 startPos)
        {
            hitPos = new Vector3(RootPosition.x, hitPos.y, RootPosition.z);
            var heading = hitPos - RootPosition;
            var distance = heading.magnitude;
            var direction = heading / distance;
            direction = DirectionVector(direction, DirectionType);
            Debug.DrawRay(startPos, direction * distance, RayHitColor);
            distance = Mathf.Clamp(distance, 0.1f, float.MaxValue);
            return distance;
        }
    }
}
