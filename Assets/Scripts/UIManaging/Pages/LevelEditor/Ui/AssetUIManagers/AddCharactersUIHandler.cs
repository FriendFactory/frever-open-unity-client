using System.Linq;
using Bridge.Models.ClientServer.Assets;
using Bridge.Models.Common;
using Bridge.Models.Common.Files;
using Common;
using Modules.CameraSystem.CameraSystemCore;
using Modules.LevelManaging.Editing.LevelManagement;
using UIManaging.Pages.LevelEditor.Ui.SelectionViews.AssetSelection;
using UIManaging.Pages.LevelEditor.Ui.SelectionViews.AssetSelection.AssetSelectors;
using UIManaging.Pages.LevelEditor.Ui.SelectionViews.AssetSelection.PaginationLoaders;

namespace UIManaging.Pages.LevelEditor.Ui.AssetUIManagers
{
    internal sealed class AddCharactersUIHandler : BaseCharactersUIHandler
    {
        private readonly ICameraSystem _cameraSystem;
        private readonly CharactersPaginationLoader.Factory _factory;
        
        private CharacterAssetSelector _assetSelectorsHolder;
        
        //---------------------------------------------------------------------
        // Properties
        //---------------------------------------------------------------------
        
        public override CharacterUIHandlerType Type => CharacterUIHandlerType.AddCharacters;
        
        //---------------------------------------------------------------------
        // Ctors
        //---------------------------------------------------------------------

        public AddCharactersUIHandler(BaseEditorPageModel levelEditorPageModel, ILevelManager levelManager, 
            ICameraSystem cameraSystem, CharactersPaginationLoader.Factory factory) : base(levelEditorPageModel, levelManager)
        {
            _cameraSystem = cameraSystem;
            _factory = factory;
        }
        
        //---------------------------------------------------------------------
        // Public
        //---------------------------------------------------------------------

        public override void SetupView(CharacterFullInfo[] selectedItems, long universeId)
        {
            var assetSelectorParameters = new PaginatedAssetSelectorParameters<CharactersPaginationLoader, CharacterInfo>
            {
                DisplayName = "Characters", 
                Categories = CATEGORIES,
                LoaderCreator = categoryId => _factory.CreateCategoryLoader(categoryId, () => _assetSelectorsHolder, universeId),
                SearchLoaderCreator = filter => _factory.CreateSearchLoader(filter, () => _assetSelectorsHolder, universeId, Constants.FRIENDS_TAB_INDEX), 
                MyAssetsLoader = null,
                ShowMyAssetsCategory = false 
            };
            
            _assetSelectorsHolder = new CharacterAssetSelector(OnCharacterItemClicked, 1, 3, _cameraSystem, assetSelectorParameters);
            _assetSelectorsHolder.AddItems(selectedItems.Select(item => new AssetSelectionCharacterModel(item)));
            _assetSelectorsHolder.SetSelectedItems(selectedItems.Select(item => item.Id).ToArray());
            
            AssetSelectorsHolder = new AssetSelectorsHolder(new MainAssetSelectorModel[] { _assetSelectorsHolder });
        }

        public override void Initialize()
        {
            PageModel.CharacterButtonClicked += OnCharacterButtonClicked;
            PageModel.CharacterLoaded += OnCharacterLoaded;
        }

        public override void CleanUp()
        {
            PageModel.CharacterButtonClicked -= OnCharacterButtonClicked;
            PageModel.CharacterLoaded -= OnCharacterLoaded;
        }

        public void AddModels(AssetSelectionCharacterModel[] models)
        {
            _assetSelectorsHolder?.AddItems(models);
        }
        
        //---------------------------------------------------------------------
        // Helpers
        //---------------------------------------------------------------------

        private void OnCharacterButtonClicked(long characterId)
        {
            var characterModel = _assetSelectorsHolder.GetItemsToShow()
                .FirstOrDefault(character => character.ItemId == characterId);
            _assetSelectorsHolder.AssetSelectionHandler.ChangeItemSelection(characterModel, false);
        }

        private void OnCharacterItemClicked(object character)
        {
            _assetSelectorsHolder.LockItems();
            PageModel.OnCharacterItemClicked(character);
        }

        private void OnCharacterLoaded()
        {
            _assetSelectorsHolder.UnlockItems();
        }
    }
}