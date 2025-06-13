using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Bridge.Models.AsseManager.Extensions.FilesContainable;
using Bridge.Models.ClientServer.Assets;
using Bridge.Models.Common;
using Extensions;
using Models;
using Modules.AssetsStoraging.Core;
using Modules.FreverUMA;
using Modules.LevelManaging.Editing.LevelManagement;
using Modules.MemoryManaging;

namespace UIManaging.Pages.LevelEditor.Ui.CacheManaging
{
    /// <summary>
    /// Trigger cache clearing with preventing deletion of currently used assets
    /// </summary>
    internal abstract class EditorPageCacheControlBase
    {
        private readonly IDataFetcher _dataFetcher;
        protected readonly AvatarHelper AvatarHelper;
        protected readonly ILevelManager LevelManager;
        protected readonly ICacheManager CacheManager;

        private Level CurrentLevel => LevelManager.CurrentLevel;

        protected EditorPageCacheControlBase(AvatarHelper avatarHelper, ILevelManager levelManager, ICacheManager cacheManager, IDataFetcher dataFetcher)
        {
            AvatarHelper = avatarHelper;
            LevelManager = levelManager;
            CacheManager = cacheManager;
            _dataFetcher = dataFetcher;
        }

        protected async Task<IEnumerable<IFilesAttachedEntity>> GetFilesUsedByCurrentLevel()
        {
            var output = CurrentLevel.ExtractAllModelWithFiles();
            var controllers = CurrentLevel.Event.SelectMany(x => x.CharacterController);
            var umaBundles = await SelectUmaBundlesUsedByCharacters(controllers);
            output.AddRange(umaBundles);
            return output;
        }

        protected async Task<IEnumerable<IFilesAttachedEntity>> SelectUmaBundlesUsedByCharacters(IEnumerable<CharacterController> controllers)
        {
            var output = new List<UmaBundleFullInfo>(_dataFetcher.MetadataStartPack.GlobalUmaBundles);
            var charactersAndOutfit = controllers.DistinctBy(x => new { CharacterId = x.Character.Id, OutfitId = x.Outfit?.Id })
                                                 .Select(x => new CharacterAndOutfit
                                                  {
                                                      Character = x.Character,
                                                      Outfit = x.Outfit
                                                  });
            foreach (var characterAndOutfit in charactersAndOutfit)
            {
                var usedBundles = await AvatarHelper.GetUmaBundleForCharacter(characterAndOutfit);
                output.AddRange(usedBundles);
            }

            return output.DistinctBy(x => x.Id);
        }
    }
}