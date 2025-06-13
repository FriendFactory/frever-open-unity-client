using System;
using System.Collections.Generic;
using System.Linq;
using Bridge;
using Modules.VideoStreaming.Remix.Selection;
using UIManaging.Localization;
using UIManaging.Pages.Feed.Remix.Loaders;
using CharacterInfo = Bridge.Models.ClientServer.Assets.CharacterInfo;

namespace UIManaging.Pages.Feed.Remix.Collection
{
    internal sealed class CharacterSelectionListModel: BaseCharacterSelectionListModel
    {
        private readonly IBridge _bridge;
        
        private readonly List<CharacterCategoryModel> _characterCategoryModels;
        private readonly List<CharacterCellViewModel> _characterCellViewModels;

        private readonly Dictionary<CharacterCollectionType, string> _categoryNames;
        
        private CharacterCollectionType _lastCategory;
        private string _userNickname;
        private int _characterModelIndex;
        private int _characterCategoryIndex;

        //---------------------------------------------------------------------
        // Properties
        //---------------------------------------------------------------------
        
        public IReadOnlyList<CharacterCategoryModel> CharacterCategoryModels => _characterCategoryModels; 
        public IReadOnlyList<CharacterCellViewModel> CharacterCellViewModels => _characterCellViewModels;
        
        //---------------------------------------------------------------------
        // Ctors 
        //---------------------------------------------------------------------

        public CharacterSelectionListModel(SelectedCharacters selectedCharactersManager,
            IPaginatedCharactersLoader paginatedCharactersLoader, IBridge bridge, bool canDeselectCharacters, FeedLocalization localization) : base(
            selectedCharactersManager, paginatedCharactersLoader, canDeselectCharacters)
        {
            _bridge = bridge;
            _lastCategory = CharacterCollectionType.MyCharacters;
            
            _characterCategoryModels = new List<CharacterCategoryModel>();
            _characterCellViewModels = new List<CharacterCellViewModel>();
            
            _categoryNames =
                new Dictionary<CharacterCollectionType, string>()
                {
                    { CharacterCollectionType.MyCharacters, localization.CharacterSelectorCategoryMyFrevers },
                    { CharacterCollectionType.Friends, localization.CharacterSelectorCategoryFriends },
                    { CharacterCollectionType.FreverStars, localization.CharacterSelectorCategoryStarCreator },
                };
            
            AddCategoryModel(_categoryNames[_lastCategory]);
        }

        public override async void DownloadNextPage()
        {
            AwaitingData = true;
            
            // lazy async initialization
            if (string.IsNullOrEmpty(_userNickname))
            {
                _userNickname = (await _bridge.GetMyProfile()).Profile.NickName;
            }
            
            base.DownloadNextPage();
        }

        //---------------------------------------------------------------------
        // Protected 
        //---------------------------------------------------------------------
        
        protected override void AddCharacterModel(DownloadedCharacterModel model)
        {
            var characterName = GetCharacterName(_userNickname, model.CharacterInfo, model.Category);
            var selectedModel = SelectedCharactersManager.SelectedCharacterModels.FirstOrDefault(selected => selected.Id == model.Id);
            
            var characterModel = new CharacterButtonModel()
            {
                NickName = characterName,
                Character = model.CharacterInfo,
                CheckAccess = model.Category == CharacterCollectionType.Friends,
                OnClick = ProcessCharacterSelection,
                Selected = selectedModel != null,
                BorderCount = selectedModel?.Index ?? -1
            };

            _models.Add(characterModel);
            _characterCellViewModels.Add(new CharacterCellViewModel(_characterModelIndex++));
        }

        protected override void OnNewPageAppendedInternal()
        {
            var page = PaginatedCharactersLoader.CharacterModels.Skip(_models.Count).ToList();
            
            if (page.Count == 0) return;

            foreach (var model in page)
            {
                if (model.Category != _lastCategory)
                {
                    _lastCategory = model.Category;
                    var categoryName = _categoryNames[_lastCategory];
                    
                    AddCategoryModel(categoryName);
                }
                
                AddCharacterModel(model);
            }
        }
        
        //---------------------------------------------------------------------
        // Helpers 
        //---------------------------------------------------------------------

        private void AddCategoryModel(string categoryName)
        {
            var categoryModel = new CharacterCategoryModel(categoryName);
            
            _characterCategoryModels.Add(categoryModel);
            _characterCellViewModels.Add(new CharacterCellViewModel(_characterCategoryIndex++, true));
        }
        
        private static string GetCharacterName(string userNickName, CharacterInfo character, CharacterCollectionType collectionType)
        {
            switch (collectionType)
            {
                case CharacterCollectionType.MyCharacters:
                    return string.IsNullOrEmpty(character.Name) ? userNickName : character.Name;

                case CharacterCollectionType.Friends:
                case CharacterCollectionType.FreverStars:
                    return character.Name;

                default:
                    throw new ArgumentOutOfRangeException(nameof(collectionType), collectionType, null);
            }
        }

        public void UpdateSelection()
        {
            foreach (var characterModel in Models)
            {
                var model = characterModel;
                var selectedModel =
                    SelectedCharactersManager.SelectedCharacterModels.FirstOrDefault(x => x.Id == model.Id);

                if (selectedModel == null) continue;

                characterModel.Select();
                characterModel.SetBorderCount(selectedModel.Index);
            }
        }
    }
}