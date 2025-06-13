using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Bridge;
using Bridge.Models.ClientServer.Recommendations;
using Modules.Amplitude;
using JetBrains.Annotations;
using UIManaging.Pages.Common.FollowersManagement;

namespace Modules.FollowRecommendations
{
    public enum FollowRecommendationsType
    {
        Friends = 0,
        FollowBack = 1,
    }

    [UsedImplicitly]
    public sealed class FollowRecommendationsListModelProvider
    {
        private static readonly TimeSpan INVALIDATE_CACHE_TIME_SPAN = TimeSpan.FromMinutes(4);
        
        private readonly FollowRecommendationsListModel _followRecommendationsListModel;
        private readonly FollowBackRecommendationsListModel _followBackRecommendationsListModel;
        private readonly FollowersManager _followersManager;

        private bool _prefetched;
        private DateTime _cacheValidUntil;
        
        public FollowRecommendationsListModelProvider(IRecommendationsBridge bridge, FollowersManager followersManager, AmplitudeManager amplitudeManager)
        {
            _followersManager = followersManager;
            _followRecommendationsListModel = new FollowRecommendationsListModel(bridge, amplitudeManager);
            _followBackRecommendationsListModel = new FollowBackRecommendationsListModel(bridge, amplitudeManager);
        }

        public async Task PrefetchData(CancellationToken token = default)
        {
            if (_prefetched && _cacheValidUntil > DateTime.UtcNow) return;

            await ReloadListsFromBackend(token);

            if (token.IsCancellationRequested) return;
            
            _prefetched = true;
            _cacheValidUntil = DateTime.UtcNow + INVALIDATE_CACHE_TIME_SPAN;
        }

        public IFollowRecommendationsListModel GetListModel(FollowRecommendationsType recommendationsType)
        {
            switch (recommendationsType)
            {
                case FollowRecommendationsType.Friends:
                    return new FollowRecommendationsListModelWrapper(_followRecommendationsListModel, _followersManager);
                case FollowRecommendationsType.FollowBack:
                    return new FollowRecommendationsListModelWrapper(_followBackRecommendationsListModel, _followersManager);
                default:
                    throw new ArgumentOutOfRangeException(nameof(recommendationsType), recommendationsType, null);
            }
        }
        
        private async Task ReloadListsFromBackend(CancellationToken token)
        {
            var tasks = new[]
            {
                _followRecommendationsListModel.InitializeAsync(token),
                _followBackRecommendationsListModel.InitializeAsync(token)
            };

            await Task.WhenAll(tasks);
        }
        
        /// <summary>
        /// Filters out the profiles which were started followed during this app session
        /// </summary>
        private sealed class FollowRecommendationsListModelWrapper: IFollowRecommendationsListModel
        {
            private readonly IFollowRecommendationsListModel _followRecommendationsListModel;
            private readonly FollowersManager _followersManager;

            public FollowRecommendationsListModelWrapper(IFollowRecommendationsListModel followRecommendationsListModel, FollowersManager followersManager)
            {
                _followRecommendationsListModel = followRecommendationsListModel;
                _followersManager = followersManager;
            }

            public IReadOnlyList<FollowRecommendation> Models => _followRecommendationsListModel.Models
               .Where(x => !_followersManager.LocalUserFollowed.ExistInCachedList(x.Group.Id)).ToArray();
        }
    }
}