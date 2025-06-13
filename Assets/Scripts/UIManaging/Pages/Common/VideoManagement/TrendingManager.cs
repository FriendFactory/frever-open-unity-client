using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Bridge.Models.ClientServer.Template;
using Bridge.Models.VideoServer;
using Bridge;
using Bridge.ExternalPackages.AsynAwaitUtility;
using JetBrains.Annotations;
using UnityEngine;
using Zenject;

namespace UIManaging.Pages.Common.VideoManagement
{
    [UsedImplicitly]
    public sealed class TrendingManager
    {
        public const int TEMPLATES_PER_PAGE = 8;
        private const int VIDEOS_PER_TEMPLATE = 8;

        [Inject] private readonly IBridge _bridge;
    
        private readonly List<TemplateChallenge> _cache = new List<TemplateChallenge>();
        private readonly List<int> _loadingPages = new List<int>();

        public async void GetTrendingChallenges(int pageIndex, Action<TemplateChallenge[]> onSuccess, Action<string> onFail = null, bool fetchNextPage = false, CancellationToken cancellationToken = default)
        {
            if (_loadingPages.Contains(pageIndex))
            {
                await new WaitUntil(() => !_loadingPages.Contains(pageIndex));
            }
            
            var skip = pageIndex > 0 ? pageIndex * TEMPLATES_PER_PAGE : 0;
            if (_cache.Count > skip)
            {
                var remainingPageSize = _cache.Count - skip;
                onSuccess?.Invoke(_cache.Skip(skip).Take(remainingPageSize).ToArray());
                return;
            }

            _loadingPages.Add(pageIndex);
            var result = await _bridge.GetTrendingTemplateChallenges(TEMPLATES_PER_PAGE, skip, VIDEOS_PER_TEMPLATE, cancellationToken);
            _loadingPages.Remove(pageIndex);
           
            if (!result.IsSuccess)
            {
                onFail?.Invoke(result.ErrorMessage);
                return;
            }
        
            _cache.AddRange(result.Models);
            if (fetchNextPage)
            {
                FetchPage(pageIndex + 1);
            }
            onSuccess?.Invoke(result.Models);
        }

        private void FetchPage(int pageIndex)
        {
            GetTrendingChallenges(pageIndex, null);
        }

        public async void GetTemplateData(long id, Action<TemplateInfo> onSuccess, CancellationToken cancellationToken = default)
        {
            var result = await _bridge.GetEventTemplate(id, cancellationToken);
            if (result == null || !result.IsSuccess)
            {
                UnityEngine.Debug.LogWarning($"Failed to download template data: {id}");
                return;
            }
            onSuccess?.Invoke(result.Model);
        }
    }
}