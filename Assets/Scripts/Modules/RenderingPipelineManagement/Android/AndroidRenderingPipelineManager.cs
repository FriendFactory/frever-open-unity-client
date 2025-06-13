using Common;
using JetBrains.Annotations;
using Modules.RenderingPipelineManagement.iOS;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace Modules.RenderingPipelineManagement.Android
{
    [UsedImplicitly]
    internal sealed class AndroidRenderingPipelineManager : BaseRenderingPipelineManager
    {
        private static readonly string PROFILE_PATH = $"ScriptableObjects/{Constants.FileDefaultNames.ANDROID_RENDERING_PROFILE}";

        //---------------------------------------------------------------------
        // IRenderingPipelineManager
        //---------------------------------------------------------------------

        public override void Init()
        {
            var profile = Resources.Load<AndroidRenderingPipelineProfile>(PROFILE_PATH);
            if (!profile)
            {
                Debug.LogWarning($"[{GetType().Name}] Android Rendering Profile has not found - default one will be used");
                
                PipelineAsset = GraphicsSettings.renderPipelineAsset as UniversalRenderPipelineAsset;
                PipeType = PipelineType.Default;
                PipelinesData.OptimizedPreviewPipeline = PipelineAsset;
                PipelinesData.HighQualityVideoRenderPipeline = PipelineAsset;
                
                return;
            }

            PipelinesData = profile.GetPipelines();
        }
    }
}