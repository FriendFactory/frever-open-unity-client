using UnityEngine;

namespace Modules.CameraSystem.PlayerCamera.Raycasting
{
    public class HorizontalCameraRaycaster : CameraRaycaster
    {
        protected override RaycastDirectionType DirectionType => RaycastDirectionType.Horizontal;

        protected override float DistanceOnHit(Vector3 hitPos, Vector3 startPos)
        {
            var heading = hitPos - startPos;
            var distance = heading.magnitude;
            var direction = heading / distance;
            direction = DirectionVector(direction, DirectionType);
            Debug.DrawRay(startPos, direction * distance, RayHitColor);
            return distance;
        }
    }
}
