using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace Modules.LevelManaging.Assets.Caption.Rendering
{
    internal sealed class RenderCaptionsAfterPostProcessingPass : ScriptableRenderPass
    {
        private const string RENDER_FEATURE_NAME = "Prevent Rendering Captions Before Post Processing Feature";
        
        public override void Configure(CommandBuffer cmd, RenderTextureDescriptor cameraTextureDescriptor)
        {
        }
        
        public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
        {
            var captions = CaptionAssetsRegister.CaptionAssets;
            if (captions.Count == 0) return;
            
            var cmd = CommandBufferPool.Get(RENDER_FEATURE_NAME);
            
            for (var i = 0; i < captions.Count; i++)
            {
                var captionAsset = captions.ElementAt(i);
                if (!captionAsset.IsViewActive) continue;
                
                var tmpText = captionAsset.CaptionView.TextComponent;
                tmpText.enabled = true;
                var worldMatrix = tmpText.transform.localToWorldMatrix;
                
                var textInfo = tmpText.textInfo;
                RenderText(textInfo, cmd, worldMatrix);
            }

            context.ExecuteCommandBuffer(cmd);
            cmd.Clear();
            CommandBufferPool.Release(cmd);
        }

        private static void RenderText(TMP_TextInfo textInfo, CommandBuffer cmd, Matrix4x4 worldMatrix)
        {
            var meshInfos = textInfo.meshInfo;
            for (var i = 0; i < meshInfos.Length; i++)
            {
                var meshInfo = meshInfos[i];
                if (meshInfo.vertexCount == 0) continue;
                var mesh = meshInfo.mesh;
                var material = meshInfo.material;

                if (mesh != null && material != null)
                {
                    cmd.DrawMesh(mesh, worldMatrix, material);
                }
            }
        }
    }
}
