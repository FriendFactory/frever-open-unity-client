using System.Linq;
using JetBrains.Annotations;
using Modules.AssetsStoraging.Core;
using Modules.FreverUMA;
using Modules.LevelManaging.Editing.LevelManagement;
using Modules.MemoryManaging;

namespace UIManaging.Pages.LevelEditor.Ui.CacheManaging
{
    [UsedImplicitly]
    internal sealed class PostRecordEditorPageCacheControl : EditorPageCacheControlBase
    {
        public PostRecordEditorPageCacheControl(AvatarHelper avatarHelper, ILevelManager levelManager, ICacheManager cacheManager, IDataFetcher dataFetcher) 
            : base(avatarHelper, levelManager, cacheManager, dataFetcher)
        {
        }

        public async void TryToClearCache()
        {
            var filesUsedByCurrentLevel = await GetFilesUsedByCurrentLevel();
            CacheManager.ClearCacheIfNeeded(filesUsedByCurrentLevel.ToArray());
        }
    }
}