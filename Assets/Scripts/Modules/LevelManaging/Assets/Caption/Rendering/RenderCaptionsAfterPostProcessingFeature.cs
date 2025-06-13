using UnityEngine.Rendering.Universal;

namespace Modules.LevelManaging.Assets.Caption.Rendering
{
    public sealed class RenderCaptionsAfterPostProcessingFeature : ScriptableRendererFeature
    {
        private RenderCaptionsAfterPostProcessingPass _renderPass;

        public override void Create()
        {
            _renderPass = new RenderCaptionsAfterPostProcessingPass
            {
                renderPassEvent = RenderPassEvent.AfterRenderingPostProcessing
            };
        }

        public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
        {
            renderer.EnqueuePass(_renderPass);
        }
    }
}