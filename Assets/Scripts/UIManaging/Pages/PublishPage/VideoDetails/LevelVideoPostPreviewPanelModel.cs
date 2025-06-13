using System;
using Models;

namespace UIManaging.Pages.PublishPage.VideoDetails
{
    internal sealed class LevelVideoPostPreviewPanelModel: BasePostVideoPreviewPanelModel
    {
        public Level Level { get; }
        
        public LevelVideoPostPreviewPanelModel(Action onPreviewRequested, Level level) : base(onPreviewRequested)
        {
            Level = level;
        }
    }
}