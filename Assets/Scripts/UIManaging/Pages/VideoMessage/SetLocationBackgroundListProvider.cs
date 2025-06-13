using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Bridge;
using Bridge.Models.ClientServer.Assets;
using Bridge.Results;
using Common.Exceptions;
using Extensions;
using JetBrains.Annotations;

namespace UIManaging.Pages.VideoMessage
{
    [UsedImplicitly]
    internal sealed class SetLocationBackgroundListProvider
    {
        private readonly IBridge _bridge;
        private readonly List<IBackgroundOption> _cache = new List<IBackgroundOption>();
        private LoadingJob _currentLoadingProcess;

        public readonly int PageSize;
        public IReadOnlyCollection<IBackgroundOption> CachedBackgrounds => _cache;
        public bool HasCached => !_cache.IsNullOrEmpty();

        public SetLocationBackgroundListProvider(IBridge bridge, int pageSize)
        {
            _bridge = bridge;
            PageSize = pageSize;
        }

        public async Task<IBackgroundOption[]> GetSetLocationBackgrounds(int pageIndex, CancellationToken token = default)
        {
            if (_currentLoadingProcess != null)
            {
                while (!_currentLoadingProcess.IsDone && !token.IsCancellationRequested)
                {
                    await Task.Delay(50, token);
                }
            }
            
            if (_cache.Count > pageIndex * PageSize)
            {
                return _cache.Skip(pageIndex * PageSize).ToArray();
            }
            
            _currentLoadingProcess = new LoadingJob(_bridge, pageIndex, PageSize);
            await _currentLoadingProcess.GetSetLocationBackground(token);

            if (_currentLoadingProcess.IsCancelled)
            {
                _currentLoadingProcess = null;
                return Array.Empty<IBackgroundOption>();
            }
            if (_currentLoadingProcess.IsSuccess)
            {
                var models = _currentLoadingProcess.SetLocationBackgrounds;
                _cache.AddRange(models);
                _currentLoadingProcess = null;
                return models;
            }

            _currentLoadingProcess = null;
            DebugHelper.LogSilentError("Failed to load set location backgrounds");
            return Array.Empty<IBackgroundOption>();
        }
        
        private sealed class LoadingJob
        {
            private readonly IBridge _bridge;
            private readonly int _pageSize;
            private readonly int _pageIndex;
            private Result<BackgroundOptions> _result;

            public LoadingJob(IBridge bridge, int pageIndex, int pageSize)
            {
                _bridge = bridge;
                _pageSize = pageSize;
                _pageIndex = pageIndex;
            }

            public bool IsSuccess => _result.IsSuccess;
            public bool IsDone { get; private set; }
            public bool IsCancelled => _result.IsRequestCanceled;
            public IBackgroundOption[] SetLocationBackgrounds => _result.Model.Options.ToArray();

            public async Task GetSetLocationBackground(CancellationToken token)
            {
                _result = await _bridge.GetSetLocationBackgroundOptionsAsync(_pageSize, _pageIndex * _pageSize, token);
                IsDone = true;
            }
        }
    }
}