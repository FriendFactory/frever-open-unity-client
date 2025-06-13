using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace Modules.WatermarkManagement
{
    internal class WatermarkRenderPass : ScriptableRenderPass
    {
        private static readonly int MAIN_TEX = Shader.PropertyToID("_MainTex");
        private static readonly int OPACITY = Shader.PropertyToID("_Opacity");
        private static readonly int OFFSET_X = Shader.PropertyToID("_OffsetX");
        private static readonly int OFFSET_Y = Shader.PropertyToID("_OffsetY");
        private static readonly int SCALE = Shader.PropertyToID("_Scale");
        private static readonly int TEXTURE_ASPECT = Shader.PropertyToID("_TextureAspect");
        private static readonly int SCREEN_ASPECT = Shader.PropertyToID("_ScreenAspect");
        
        private readonly Material _material;
        private RTHandle _temporaryTexture;

        public WatermarkRenderPass(Material material)
        {
            _material = material;
        }

        public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
        {
            #if UNITY_EDITOR
            if (!Application.isPlaying) return;
            #endif
            
            if (!WatermarkSettings.IsOn) return;
            
            if (renderingData.cameraData.camera.cameraType != CameraType.Game ||
                renderingData.cameraData.camera != WatermarkSettings.TargetCamera) return;

            var cmd = CommandBufferPool.Get("CustomWatermarkRenderFeature");

            var posSettings = WatermarkSettings.GetPositionSettings();
            _material.SetTexture(MAIN_TEX, WatermarkSettings.WatermarkTexture);
            _material.SetFloat(OPACITY, WatermarkSettings.Opacity);
            _material.SetFloat(OFFSET_X, posSettings.OffsetX);
            _material.SetFloat(OFFSET_Y, posSettings.OffsetY);
            _material.SetFloat(SCALE, posSettings.Scale);
            
            var watermarkAspect = WatermarkSettings.WatermarkTexture.width / (float)WatermarkSettings.WatermarkTexture.height;
            _material.SetFloat(TEXTURE_ASPECT, watermarkAspect);
            var cameraAspect = renderingData.cameraData.camera.aspect;
            _material.SetFloat(SCREEN_ASPECT, cameraAspect);
            
            cmd.Blit(WatermarkSettings.WatermarkTexture, renderingData.cameraData.renderer.cameraColorTargetHandle, _material);
            if (renderingData.cameraData.camera.targetTexture != null)
            {
                cmd.Blit(WatermarkSettings.WatermarkTexture, renderingData.cameraData.camera.targetTexture, _material);
            }
            
            context.ExecuteCommandBuffer(cmd);
            CommandBufferPool.Release(cmd);
        }
    }
}