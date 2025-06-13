using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Bridge.Results;
using Common;
using JetBrains.Annotations;
using CharacterInfo = Bridge.Models.ClientServer.Assets.CharacterInfo;
using Resolution = Bridge.Models.Common.Files.Resolution;

namespace UIManaging.Pages.LevelEditor.Ui.SelectionViews.AssetSelection.PaginationLoaders
{
    [UsedImplicitly]
    public sealed class CharactersPaginationLoader : AssetSelectorPaginationLoader<CharacterInfo>
    {
        private CharactersPaginationLoader(PaginationLoaderType type, Func<AssetSelectorModel> selectorModel, long universeId,
            NullableLong categoryId = null, string filter = null, NullableLong taskId = null) :
            base(type, selectorModel, universeId,categoryId, filter, taskId)
        {
        }

        protected override Task<ArrayResult<CharacterInfo>> GetAssetListAsync(long? targetId, int takeNext,
            int takePrevious, long? categoryId, string filter,
            CancellationToken token)
        {
            switch (categoryId)
            {
                case Constants.FRIENDS_TAB_INDEX:
                    return Bridge.GetFriendsMainCharacters(targetId, takeNext, takePrevious, UniverseId, filter, token);
                case Constants.STAR_CREATORS_TAB_INDEX:
                    return Bridge.GetStarCharacters(targetId, takeNext, takePrevious, UniverseId, filter, token);
                default:
                    return Bridge.GetMyCharacters(targetId, takeNext, takePrevious, UniverseId, filter, token);
            }
        }

        protected override IList<AssetSelectionItemModel> CreateItemModels(IList<CharacterInfo> page)
        {
            var selectableItems = 
                page.Select(character => new AssetSelectionCharacterModel(Resolution._512x512, character, CategoryId, character.GroupId != Bridge.Profile.GroupId))
                    .OrderByDescending(model => model.ItemId)
                    .ToList<AssetSelectionItemModel>();

            return selectableItems;
        }


        [UsedImplicitly]
        public class Factory : PaginationLoaderFactoryBase<CharactersPaginationLoader, CharacterInfo>
        {
        }
    }
}