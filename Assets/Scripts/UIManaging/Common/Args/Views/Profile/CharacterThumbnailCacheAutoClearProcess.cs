using JetBrains.Annotations;
using Navigation.Core;

namespace UIManaging.Common.Args.Views.Profile
{
    [UsedImplicitly]
    public sealed class CharacterThumbnailCacheAutoClearProcess
    {
        private readonly CharacterThumbnailProvider _characterThumbnailProvider;
        private readonly PageManager _pageManager;

        private bool _isRunning;

        internal CharacterThumbnailCacheAutoClearProcess(CharacterThumbnailProvider characterThumbnailProvider, PageManager pageManager)
        {
            _characterThumbnailProvider = characterThumbnailProvider;
            _pageManager = pageManager;
        }

        public void Run()
        {
            if (_isRunning) return;
            _pageManager.PageSwitchingBegan += (id, data) => _characterThumbnailProvider.ClearCache();
        }
    }
}