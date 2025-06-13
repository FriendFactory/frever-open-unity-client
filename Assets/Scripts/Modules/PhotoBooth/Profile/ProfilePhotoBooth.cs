using System.Threading.Tasks;
using Common;
using JetBrains.Annotations;
using Modules.CameraCapturing;
using UnityEngine;

namespace Modules.PhotoBooth.Profile
{
    [UsedImplicitly]
    public sealed class ProfilePhotoBooth
    {
        public async Task<Texture2D> GetPhotoAsync(Camera camera, Vector2Int resolution)
        {
            if (!camera)
            {
                Debug.LogError($"[{GetType().Name}] Camera is not set");
                return null;
            }
            
            var photo = await CaptureIntoTextureAsync(camera, resolution);

            return photo;
        }

        private async Task<Texture2D> CaptureIntoTextureAsync(Camera camera, Vector2Int resolution)
        {
            var aspectRatio = (float)Screen.height / Screen.width;
            var sourceResolution = new Vector2Int(resolution.x, (int)(resolution.x * aspectRatio));
            var renderTexture = CameraCapture.CaptureIntoRenderTexture(camera, sourceResolution, true, Constants.ThumbnailSettings.PROFILE_PHOTO_BOOTH_RENDER_SCALE);
            
            var originalRenderTexture = RenderTexture.active;
            RenderTexture.active = renderTexture;
            
            var snapshot = new Texture2D(resolution.x, resolution.y);
            var (xRect, yRect) = (0f, Mathf.Floor(0.5f * (sourceResolution.y - resolution.y)));
            var rect = new Rect(xRect, yRect, resolution.x, resolution.y);
            
            snapshot.ReadPixels(rect, 0, 0, false);
            snapshot.Apply();

            RenderTexture.active = originalRenderTexture;

            RenderTexture.ReleaseTemporary(renderTexture);
            
            return await Task.FromResult(snapshot);
        }
    }
}