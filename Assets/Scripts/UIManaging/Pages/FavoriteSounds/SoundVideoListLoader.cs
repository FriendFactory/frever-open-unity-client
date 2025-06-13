using System.Threading;
using System.Threading.Tasks;
using Bridge;
using Bridge.Models.Common;
using Bridge.Models.VideoServer;
using Extensions;
using Navigation.Args;
using Navigation.Core;
using UIManaging.Common.Args.Views.LastLevelsPanelArgs;
using UIManaging.Pages.Common.VideoManagement;

namespace UIManaging.Pages.FavoriteSounds
{
    public class SoundVideoListLoader: BaseVideoListLoader
    {
        private readonly IPlayableMusic _sound;
        
        public SoundVideoListLoader(IPlayableMusic sound, VideoManager videoManager, PageManager pageManager, IBridge bridge, long userGroupId) : base(videoManager, pageManager, bridge, userGroupId)
        {
            _sound = sound;
        }

        protected override async Task<Video[]> DownloadModelsInternal(object targetId, int takeNext, int takePrevious = 0, CancellationToken token = default)
        {
            var type = _sound.GetFavoriteSoundType();
            var id = _sound.GetSoundId();
            var result = await VideoManager.GetVideosForSound(type, id, (long?)targetId, takeNext, takePrevious, token);
            
            return result.Video;
        }

        protected override void OnVideoPreviewClicked(BaseLevelItemArgs args)
        {
            var modelIndex = Models.IndexOf(args.Video);
            
            PageManager.MoveNext(PageId.Feed, new VideosBasedOnSoundFeedArgs(_sound, VideoManager, args.Video.Id, modelIndex));
        }
    }
}