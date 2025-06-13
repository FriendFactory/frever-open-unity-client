using Bridge;
using Navigation.Core;
using UIManaging.Common.Args.Views.LastLevelsPanelArgs;
using UIManaging.Pages.Common.VideoManagement;

namespace UIManaging.Common
{
    public abstract class LocalUserVideoListLoaderBase : BaseVideoListLoader
    {
        protected LocalUserVideoListLoaderBase(VideoManager videoManager, PageManager pageManager, IBridge bridge,
            long userGroupId) : base(videoManager, pageManager, bridge, userGroupId)
        {
        }

        public override void AdjustToRowSize(int rowSize)
        {
            var removeCount = LevelPreviewArgs.Count;
            if (removeCount <= DefaultPageSize) return;
            removeCount %= rowSize;
            if (removeCount == 0) return;
            Models.RemoveRange(Models.Count - removeCount, removeCount);
            LevelPreviewArgs.RemoveRange(LevelPreviewArgs.Count - removeCount, removeCount);
        }
    }
}