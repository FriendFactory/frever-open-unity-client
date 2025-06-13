using UnityEngine;

namespace Extensions
{
    public static class UnityCameraExtension
    {
        public static void ApplyAspectRatioFromRenderTextureImmediate(this Camera camera)
        {
            camera.Render();
        }
    }
}