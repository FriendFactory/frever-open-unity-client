using Common;
using Modules.RenderingPipelineManagement.iOS;
using UnityEngine;

namespace Modules.RenderingPipelineManagement.Android
{
    [CreateAssetMenu(menuName = "Friend Factory/Rendering Pipeline Profile/Android", fileName = Constants.FileDefaultNames.ANDROID_RENDERING_PROFILE)]
    internal sealed class AndroidRenderingPipelineProfile: ScriptableObject
    {
        [SerializeField] private PipelinesData _pipelines;

        private PipelinesData _editablePipelines;

        public PipelinesData GetPipelines()
        {
            _editablePipelines.OptimizedPreviewPipeline = Instantiate(_pipelines.OptimizedPreviewPipeline);
            _editablePipelines.HighQualityVideoRenderPipeline = Instantiate(_pipelines.HighQualityVideoRenderPipeline);

            return _editablePipelines;
        }
    }
}