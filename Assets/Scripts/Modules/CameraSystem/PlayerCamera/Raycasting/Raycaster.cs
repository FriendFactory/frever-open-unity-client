using UnityEngine;

namespace Modules.CameraSystem.PlayerCamera.Raycasting
{
    public abstract class CameraRaycaster
    {
        private const float RAYCAST_DISTANCE = 1000f;
        private const int ENVIRONMENT_LAYER = 8;

        //---------------------------------------------------------------------
        // Properties
        //---------------------------------------------------------------------

        public Color RayColor { get; set; } = Color.red;
        public Color RayHitColor { get; set; } = Color.green;

        public bool ColliderIsHit { get; set; }
        public Vector3 HitPos { get; set; }
        public float Distance { get; set; }

        protected abstract RaycastDirectionType DirectionType { get; }

        //---------------------------------------------------------------------
        // Public
        //---------------------------------------------------------------------

        public void Raycast(Vector3 startPos, Vector3 dirPos)
        {
            var heading = dirPos - startPos;
            var distance = heading.magnitude;
            var direction = heading / distance;
            direction = DirectionVector(direction, DirectionType);

            if (Physics.Raycast(startPos, direction, out var hit, RAYCAST_DISTANCE))
            {
                Debug.DrawRay(startPos, direction * RAYCAST_DISTANCE, RayColor);

                if (hit.collider.gameObject.layer.Equals(ENVIRONMENT_LAYER))
                {
                    HitPos = hit.point;
                    ColliderIsHit = true;
                    Distance = DistanceOnHit(hit.point, startPos);
                    return;
                }
            }
        
            ColliderIsHit = false;
        }

        //---------------------------------------------------------------------
        // Protected
        //---------------------------------------------------------------------

        protected abstract float DistanceOnHit(Vector3 hitPos, Vector3 startPos);
    
        protected Vector3 DirectionVector(Vector3 direction, RaycastDirectionType raycastDirection)
        {
            switch (raycastDirection)
            {
                case RaycastDirectionType.Vertical:
                    return new Vector3(0, direction.y, 0);
                case RaycastDirectionType.Horizontal:
                    return new Vector3(direction.x, 0, direction.z);
                default:
                    return direction;
            }
        }
    }
}
