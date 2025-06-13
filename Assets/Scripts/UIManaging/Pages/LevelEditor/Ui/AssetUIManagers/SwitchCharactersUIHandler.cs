using System.Linq;
using Bridge.Models.Common;
using Bridge.Models.ClientServer.Assets;
using Common;
using Extensions;
using Modules.CharacterManagement;
using Modules.LevelManaging.Editing.LevelManagement;
using UIManaging.Pages.LevelEditor.Ui.SelectionViews.AssetSelection;
using UIManaging.Pages.LevelEditor.Ui.SelectionViews.AssetSelection.AssetSelectors;
using UIManaging.Pages.LevelEditor.Ui.SelectionViews.AssetSelection.PaginationLoaders;

namespace UIManaging.Pages.LevelEditor.Ui.AssetUIManagers
{
    internal sealed class SwitchCharactersUIHandler : BaseCharactersUIHandler
    {
        private readonly CharacterManager _characterManager;
        private readonly CharactersPaginationLoader.Factory _factory;
        
        private CharacterAssetSwitchSelector _characterAssetSwitchSelector;

        //---------------------------------------------------------------------
        // Properties
        //---------------------------------------------------------------------
        
        public override CharacterUIHandlerType Type => CharacterUIHandlerType.SwitchCharacters;

        //---------------------------------------------------------------------
        // Ctors
        //---------------------------------------------------------------------

        public SwitchCharactersUIHandler(BaseEditorPageModel levelEditorPageModel, ILevelManager levelManager,
            CharacterManager characterManager, CharactersPaginationLoader.Factory factory)
            : base(levelEditorPageModel, levelManager)
        {
            _characterManager = characterManager;
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
                LoaderCreator = categoryId => _factory.CreateCategoryLoader(categoryId, () => _characterAssetSwitchSelector, universeId),
                SearchLoaderCreator = filter => _factory.CreateSearchLoader(filter, () => _characterAssetSwitchSelector, universeId, Constants.FRIENDS_TAB_INDEX), 
                MyAssetsLoader = null,
                ShowMyAssetsCategory = false 
            };
            
            _characterAssetSwitchSelector = new CharacterAssetSwitchSelector(SwitchableCharacterClicked, assetSelectorParameters);
            
            _characterAssetSwitchSelector.AddItems(selectedItems.Select(item => new AssetSelectionCharacterModel(item)));
            
            AssetSelectorsHolder = new AssetSelectorsHolder(new[] { _characterAssetSwitchSelector });
        }

        public override void Initialize()
        {
            PageModel.CharacterSwitchableButtonClicked += OnSwitchableControlPanelTargetCharacterClicked;
            PageModel.CharacterLoaded += OnCharacterLoaded;
        }

        public override void CleanUp()
        {
            PageModel.CharacterSwitchableButtonClicked -= OnSwitchableControlPanelTargetCharacterClicked;
            PageModel.CharacterLoaded -= OnCharacterLoaded;
        }
        
        //---------------------------------------------------------------------
        // Helpers
        //---------------------------------------------------------------------
        
        private async void SwitchableCharacterClicked(IEntity character)
        {
            if (SuppressCharacterViewUpdate)
            {
                return;
            }
            
            if (_characterAssetSwitchSelector == null)
            {
                return;
            }

            PageModel.OnCharacterToSwitchClicked();
            var allCharacters = LevelManager.TargetEvent.GetCharacters();
            var currentCharacterId = PageModel.SwitchTargetCharacterId;
            var currentCharacter = allCharacters.First(x => x.Id == currentCharacterId);

            var newCharacter = _characterAssetSwitchSelector.GetItemsToShow()
                    .FirstOrDefault(itm => itm.ItemId == character.Id)
                    ?.RepresentedObject as CharacterInfo;

            var unloadOld = true;
            if (PageModel.IsPostRecordEditorOpened)
            {
                var characterUsedInOtherEvents = LevelManager.CurrentLevel.Event.SelectMany(x => x.CharacterController)
                    .Select(x => x.CharacterId).Count(x => x == currentCharacterId) > 1;
                unloadOld = !characterUsedInOtherEvents;
            }
            
            var characterFullInfo = await _characterManager.GetCharacterAsync(character.Id);
            
            LevelManager.ReplaceCharacter(currentCharacter, characterFullInfo, unloadOld,
                (x) => PageModel.SetSwitchTargetCharacterId(newCharacter.Id));
        }
        
        private void OnSwitchableControlPanelTargetCharacterClicked(long characterId)
        {
            if (_characterAssetSwitchSelector == null) return;
            
            _characterAssetSwitchSelector.LockItems();
            var characterAssetModel = _characterAssetSwitchSelector.GetItemsToShow()
                .FirstOrDefault(itm => itm.ItemId == characterId);
            SuppressCharacterViewUpdate = true;
            _characterAssetSwitchSelector.AssetSelectionHandler.ChangeItemSelection(
                characterAssetModel, true);
            SuppressCharacterViewUpdate = false;
        }

        private void OnCharacterLoaded()
        {
            _characterAssetSwitchSelector.UnlockItems();
        }
    }
}
