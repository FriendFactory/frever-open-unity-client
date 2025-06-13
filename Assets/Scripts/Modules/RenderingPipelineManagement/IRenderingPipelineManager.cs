namespace Modules.RenderingPipelineManagement
{
    public interface IRenderingPipelineManager
    {
        float RenderScale { get; }
        void Init();
        void SetDefaultPipeline();
        void SetHighQualityPipeline();
        void SetPipeline(PipelineType type);
        bool IsHighQuality();
    }

    public enum PipelineType
    {
        Default,
        HighQuality,
    }
}