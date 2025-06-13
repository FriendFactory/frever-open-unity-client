using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace Modules.WatermarkManagement
{
    public sealed class CustomWatermarkRenderFeature : ScriptableRendererFeature
    {
        private WatermarkRenderPass _scriptablePass; 
        public Shader Shader;
        
        public override void Create()
        {
            var material = CoreUtils.CreateEngineMaterial(Shader);
            _scriptablePass = new WatermarkRenderPass(material)
            {
                renderPassEvent = RenderPassEvent.AfterRendering
            };
        }

        public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
        {
            renderer.EnqueuePass(_scriptablePass);
        }
    }
}
