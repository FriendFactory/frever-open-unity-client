using System.Linq;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace Modules.LevelManaging.Assets.Caption.Rendering
{
    internal sealed class PreventRenderingCaptionsBeforePostProcessingPass : ScriptableRenderPass
    {
        public override void Configure(CommandBuffer cmd, RenderTextureDescriptor cameraTextureDescriptor)
        {
        }
        
        public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
        {
            var captions = CaptionAssetsRegister.CaptionAssets;
            if (captions.Count == 0) return;

            for (var i = 0; i < captions.Count; i++)
            {
                var captionAsset = captions.ElementAt(i);
                if (!captionAsset.IsViewActive) continue;
                var captionText = captionAsset.CaptionView.TextComponent;
                captionText.enabled = false;
            }
        }
    }
}
