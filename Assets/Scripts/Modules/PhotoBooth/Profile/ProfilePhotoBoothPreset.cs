using UnityEngine;

namespace Modules.PhotoBooth.Profile
{
    [CreateAssetMenu(order = 10, fileName = "Profile Preset", menuName = "ScriptableObjects/PhotoBooth/Profile Preset")]
    public class ProfilePhotoBoothPreset: ScriptableObject
    {
        public Vector2Int resolution = new Vector2Int(256, 256);
        public Vector3 followOffset = 2f * Vector3.back;
        public Vector3 targetOffset = 0.25f * Vector3.up;
        public float verticalFOV = 25f;
    }
}