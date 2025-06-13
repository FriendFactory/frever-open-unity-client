using UnityEngine.Rendering.Universal;

namespace Modules.LevelManaging.Assets.Caption.Rendering
{
    public sealed class PreventRenderingCaptionsBeforePostProcessingFeature : ScriptableRendererFeature
    {
        private PreventRenderingCaptionsBeforePostProcessingPass _renderPass;

        public override void Create()
        {
            _renderPass = new PreventRenderingCaptionsBeforePostProcessingPass
            {
                renderPassEvent = RenderPassEvent.AfterRendering
            };
        }

        public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
        {
            renderer.EnqueuePass(_renderPass);
        }
    }
}
