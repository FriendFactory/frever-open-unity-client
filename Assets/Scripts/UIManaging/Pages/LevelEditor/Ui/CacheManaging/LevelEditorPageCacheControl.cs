using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Bridge.Models.AsseManager.Extensions.FilesContainable;
using Bridge.Models.Common;
using Extensions;
using JetBrains.Annotations;
using Models;
using Modules.AssetsStoraging.Core;
using Modules.FreverUMA;
using Modules.LevelManaging.Editing.LevelManagement;
using Modules.MemoryManaging;

namespace UIManaging.Pages.LevelEditor.Ui.CacheManaging
{
    [UsedImplicitly]
    internal sealed class LevelEditorPageCacheControl: EditorPageCacheControlBase
    {
        private Event RecordingEvent => LevelManager.TargetEvent;
        
        public LevelEditorPageCacheControl(AvatarHelper avatarHelper, ILevelManager levelManager, ICacheManager cacheManager, IDataFetcher dataFetcher) 
            : base(avatarHelper, levelManager, cacheManager, dataFetcher)
        {
        }

        public async void TryToClearCache()
        {
            var filesUsedByCurrentLevel = await GetFilesUsedByCurrentLevel();
            var filesUsedByRecordingEvent = await GetFilesUsedByRecordingEvent();
            var allFiles = filesUsedByCurrentLevel.AppendWith(filesUsedByRecordingEvent).ToArray();
            CacheManager.ClearCacheIfNeeded(allFiles);
        }

        private async Task<IEnumerable<IFilesAttachedEntity>> GetFilesUsedByRecordingEvent()
        {
            var output = RecordingEvent.ExtractAllModelWithFiles();
            var umaBundles = await SelectUmaBundlesUsedByCharacters(RecordingEvent.CharacterController);
            output.AddRange(umaBundles);
            return output;
        }
    }
}