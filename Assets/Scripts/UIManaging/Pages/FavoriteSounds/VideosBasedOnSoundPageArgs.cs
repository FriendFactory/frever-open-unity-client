using Bridge;
using Navigation.Core;
using UIManaging.Common.Args.Views.LastLevelsPanelArgs;
using UIManaging.Pages.Common.FavoriteSounds;
using UIManaging.Pages.Common.VideoManagement;

namespace UIManaging.Pages.FavoriteSounds
{
    public class VideosBasedOnSoundPageArgs : PageArgs
    {
        public override PageId TargetPage => PageId.VideosBasedOnSound;
        
        public UsedSoundItemModel UsedSoundModel { get; }

        public VideosBasedOnSoundPageArgs(UsedSoundItemModel sound)
        {
            UsedSoundModel = sound;
        }
        
        public BaseVideoListLoader GetVideoListLoader(PageManager pageManager, VideoManager videoManager, IBridge bridge)
        {
            return new SoundVideoListLoader(UsedSoundModel.Sound, videoManager, pageManager, bridge, 0);
        }
    }
}