using System;
using System.Threading.Tasks;
using Unity.Collections;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace Modules.CameraCapturing
{
    public static class CameraCapture
    {
        private const float DEFAULT_RENDER_SCALE = 1f;

        private static float _preservedRenderScale = -1f;
        private static UniversalRenderPipelineAsset _preservedUrpAsset;
        
        public static async Task<Texture2D> CaptureIntoTextureAsync(Camera camera, Vector2Int resolution, bool changeRenderScale = false, float renderScale = DEFAULT_RENDER_SCALE)
        {
            if (camera == null)
            {
                throw new ArgumentNullException($"Frame capturing failed. Camera can not be null");
            }
            
            var renderTexture = GetTemporaryRenderTexture(resolution);
            camera.targetTexture = renderTexture;

            if (changeRenderScale)
            {
                SetUrpRenderScale(renderScale);
            }

            var originalRenderTexture = RenderTexture.active;
            RenderTexture.active = camera.targetTexture;

            camera.Render();

            var textureReadback = GetTextureReadback(resolution);
            var snapshot = await textureReadback.ReadIntoTextureAsync(renderTexture);

            RenderTexture.active = originalRenderTexture;
            camera.targetTexture = null;

            RenderTexture.ReleaseTemporary(renderTexture);
            if (changeRenderScale)
            {
                RestoreUrpRenderScale();
            }

            return snapshot;
        }

        public static RenderTexture CaptureIntoRenderTexture(Camera camera, Vector2Int resolution, bool changeRenderScale = false, float renderScale = DEFAULT_RENDER_SCALE)
        {
            if (camera == null)
            {
                throw new ArgumentNullException($"Frame capturing failed. Camera can not be null");
            }

            var cameraTexture = camera.targetTexture;
            var renderTexture = GetTemporaryRenderTexture(resolution);
            camera.targetTexture = renderTexture;

            var originalRenderTexture = RenderTexture.active;
            RenderTexture.active = camera.targetTexture;

            if (changeRenderScale)
            {
                SetUrpRenderScale(renderScale);
            }

            camera.Render();

            RenderTexture.active = originalRenderTexture;
            camera.targetTexture = cameraTexture;
            if (changeRenderScale)
            {
                RestoreUrpRenderScale();
            }

            return renderTexture;
        }

        public static void CaptureFromCamera(Camera camera, Vector2Int resolution,
                                             Action<NativeArray<uint>> onPixelDataRetrieved,
                                             bool changeRenderScale = false, float renderScale = DEFAULT_RENDER_SCALE)
        {
            if (camera == null)
            {
                throw new ArgumentNullException($"Frame capturing failed. Camera can not be null");
            }

            var textureReadback = GetTextureReadback(resolution);
            var cameraTexture = camera.targetTexture;
            var renderTexture = GetTemporaryRenderTexture(resolution);
            camera.targetTexture = renderTexture;

            var originalRenderTexture = RenderTexture.active;
            RenderTexture.active = camera.targetTexture;

            if (changeRenderScale)
            {
                SetUrpRenderScale(renderScale);
            }

            camera.Render();

            textureReadback.Request(renderTexture, OnPixelDataRetrieved);

            camera.targetTexture = cameraTexture;
            RenderTexture.active = originalRenderTexture;

            void OnPixelDataRetrieved(NativeArray<uint> pixelData)
            {
                onPixelDataRetrieved?.Invoke(pixelData);

                textureReadback.Dispose();
                RenderTexture.ReleaseTemporary(renderTexture);
                
                if (changeRenderScale)
                {
                    RestoreUrpRenderScale();
                }
            }
        }

        private static ITextureReadback GetTextureReadback(Vector2Int resolution)
        {
            return SystemInfo.supportsAsyncGPUReadback
                ? new AsyncTextureReadback(resolution) as ITextureReadback
                : new TextureReadback(resolution);
        }

        private static RenderTexture GetTemporaryRenderTexture(Vector2Int resolution)
        {
            return RenderTexture.GetTemporary(resolution.x, resolution.y, 0, RenderTextureFormat.ARGB32);
        }

        private static void SetUrpRenderScale(float renderScale)
        {
            _preservedUrpAsset = GraphicsSettings.currentRenderPipeline as UniversalRenderPipelineAsset;
            var previousRenderScale = 0f;
            if (_preservedUrpAsset is null)
            {
                Debug.LogError("No URP Asset found. Please configure URP in Project Settings.");
            } 
            else
            {
                _preservedRenderScale = _preservedUrpAsset.renderScale;
                _preservedUrpAsset.renderScale = renderScale;
            }
        }

        private static void RestoreUrpRenderScale()
        {
            if (_preservedUrpAsset is not null)
            {
                _preservedUrpAsset.renderScale = _preservedRenderScale;
            }
        }
    }
}