using System.Threading.Tasks;
using Bridge;
using Bridge.Models.ClientServer.Assets;
using Bridge.Models.VideoServer;
using Bridge.Results;
using JetBrains.Annotations;
using UIManaging.Pages.Common.UsersManagement.BlockedUsersManaging;
using UnityEngine;

namespace UIManaging.Pages.Common.VideoManagement
{
    [UsedImplicitly]
    internal class SoundVideoLoader: MultipleListCacheVideoLoader<SoundVideoLoaderArgs, long>
    {
        public override VideoListType VideoType => VideoListType.Sound;

        public SoundVideoLoader(IVideoBridge bridge, IBlockedAccountsManager blockedAccountsManager) : base(bridge, blockedAccountsManager)
        {
        }
        
        protected override async Task<EntitiesResult<Video>> DownloadFromServerAsync(SoundVideoLoaderArgs args)
        {
            var result = await Bridge.GetVideoListBySoundAsync(args.SoundType, args.SoundId, args.TargetVideoKey, args.TakeNext, args.CancellationToken);

            if (result.IsError)
            {
                Debug.LogError($"[{GetType().Name}] Failed to get videos # {result.ErrorMessage}");
                return null;
            }
            
            return result;
        }

        protected override long GetCacheKey(SoundVideoLoaderArgs args) => args.SoundId;
    }

    internal class SoundVideoLoaderArgs : VideoLoadArgs
    {
        public SoundType SoundType { get; }
        public long SoundId { get; }
        
        public override VideoListType VideoType => VideoListType.Sound;

        public SoundVideoLoaderArgs(SoundType soundType, long soundId)
        {
            SoundType = soundType;
            SoundId = soundId;
        }
    }
}