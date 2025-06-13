using System.Threading.Tasks;
using Bridge;
using Bridge.Models.VideoServer;
using Bridge.Results;
using Modules.Amplitude;
using JetBrains.Annotations;
using UIManaging.Pages.Common.UsersManagement.BlockedUsersManaging;
using Zenject;

namespace UIManaging.Pages.Common.VideoManagement
{
    [UsedImplicitly]
    internal sealed class ForMeFeedVideoLoader :  SingleListCacheVideoLoader<VideoLoadArgs>
    {
        [Inject] private readonly AmplitudeManager _amplitudeManager;
        
        public override VideoListType VideoType => VideoListType.ForMe;

        public ForMeFeedVideoLoader(IBlockedAccountsManager blockedAccountsManager, IVideoBridge bridge) : base(blockedAccountsManager, bridge)
        {
        }

        protected override Task<EntitiesResult<Video>> DownloadFromServerAsync(VideoLoadArgs args)
        {
            return Bridge.GetForYouFeedMLVideoListAsync(args.TargetVideoKey, args.TakeNext, _amplitudeManager.MlExperimentVariantsHeader, args.CancellationToken);
        }
    }
}