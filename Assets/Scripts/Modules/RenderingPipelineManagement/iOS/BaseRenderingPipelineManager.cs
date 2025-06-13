using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace Modules.RenderingPipelineManagement.iOS
{
    public abstract class BaseRenderingPipelineManager : IRenderingPipelineManager
    {
        protected UniversalRenderPipelineAsset PipelineAsset;
        protected PipelineType PipeType;

        private protected PipelinesData PipelinesData;

        //---------------------------------------------------------------------
        // Properties
        //---------------------------------------------------------------------

        public float RenderScale => PipelineAsset.renderScale;

        //---------------------------------------------------------------------
        // IRenderingPipelineManager
        //---------------------------------------------------------------------

        public abstract void Init();

        public void SetDefaultPipeline()
        {
            SetPipeline(PipelineType.Default);
        }

        public void SetHighQualityPipeline()
        {
            SetPipeline(PipelineType.HighQuality);
        }

        public void SetPipeline(PipelineType type)
        {
            GraphicsSettings.renderPipelineAsset = type == PipelineType.Default
                ? PipelinesData.OptimizedPreviewPipeline
                : PipelinesData.HighQualityVideoRenderPipeline;
            PipeType = type;
            PipelineAsset = (UniversalRenderPipelineAsset) GraphicsSettings.renderPipelineAsset;
        }

        public bool IsHighQuality()
        {
            return PipeType == PipelineType.HighQuality;
        }
    }
}