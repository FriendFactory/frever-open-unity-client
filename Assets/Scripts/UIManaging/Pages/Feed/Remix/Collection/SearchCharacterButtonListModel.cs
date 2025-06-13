using System.Linq;
using Extensions;
using Modules.VideoStreaming.Remix.Selection;
using UIManaging.Pages.Feed.Remix.Loaders;

namespace UIManaging.Pages.Feed.Remix.Collection
{
    internal sealed class SearchCharacterButtonListModel: BaseCharacterSelectionListModel
    {
        public SearchCharacterButtonListModel(SelectedCharacters selectedCharacterManager,
            IPaginatedCharactersLoader paginatedCharactersLoader, bool canDeselectCharacters) : base(
            selectedCharacterManager, paginatedCharactersLoader, canDeselectCharacters) { }

        protected override void AddCharacterModel(DownloadedCharacterModel model)
        {
            var selectedModel = SelectedCharactersManager.SelectedCharacterModels.FirstOrDefault(selected => selected.Id == model.Id);
            
            var characterModel = new CharacterButtonModel()
            {
                NickName = model.CharacterInfo.Name,
                Character = model.CharacterInfo,
                CheckAccess = model.Category == CharacterCollectionType.Friends,
                OnClick = ProcessCharacterSelection,
                Selected = selectedModel != null,
                BorderCount = selectedModel?.Index ?? -1
            };

            _models.Add(characterModel);
        }

        protected override void OnNewPageAppendedInternal()
        {
            if (PaginatedCharactersLoader.CharacterModels.Count <= _models.Count) return;

            PaginatedCharactersLoader.CharacterModels.Skip(_models.Count).ForEach(AddCharacterModel);
        }
    }
}
