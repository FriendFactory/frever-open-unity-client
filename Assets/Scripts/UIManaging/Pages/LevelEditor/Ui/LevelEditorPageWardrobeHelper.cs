using System;
using System.Linq;
using System.Threading.Tasks;
using Bridge.Models.ClientServer.Assets;
using Bridge.Models.Common;
using Common.Abstract;
using Common.UserBalance;
using Extensions;
using JetBrains.Annotations;
using Modules.AssetsStoraging.Core;
using Modules.EditorsCommon;
using Modules.LevelManaging.Editing.LevelManagement;
using Modules.WardrobeManaging;
using Navigation.Args;
using UIManaging.Pages.Common.UsersManagement;
using UIManaging.Pages.LevelEditor.Ui.EditingFlow;
using UIManaging.Pages.LevelEditor.Ui.EditingFlow.ShoppingCart;
using UIManaging.Pages.LevelEditor.Ui.Wardrobe;
using UIManaging.Pages.UmaEditorPage.Ui.ShoppingCartInfo;
using UnityEngine;
using Zenject;

namespace UIManaging.Pages.LevelEditor.Ui
{
    internal sealed class LevelEditorPageWardrobeHelper: BaseContextlessPanel
    {
        [SerializeField] private LevelEditorPage _levelEditorPage; 
        [SerializeField] private UmaLevelEditor _wardrobeUmaEditor;
        [SerializeField] private global::Common.ShoppingCart.ShoppingCart _shoppingCart;
        [SerializeField] private ShoppingCartHelper _shoppingCartHelper;
        [SerializeField] private UserBalanceView _userBalanceView;
        
        private ILevelManager _levelManager;
        private IDataFetcher _dataFetcher;
        private IEditorSettingsProvider _editorSettingsProvider;
        private LevelEditorPageModel _pageModel;
        private IWardrobeStore _wardrobeStore;
        private LocalUserDataHolder _userData;
        private ShoppingCartInfoModel _shoppingCartInfoModel;
        
        private bool _closingWardrobePanel;

        public long GenderId { get; set; }
        
        [Inject, UsedImplicitly]
        private void Construct(ILevelManager levelManager, IDataFetcher dataFetcher, IEditorSettingsProvider editorSettingsProvider, LevelEditorPageModel pageModel,
            IWardrobeStore wardrobeStore, LocalUserDataHolder userData, ShoppingCartInfoModel shoppingCartInfoModel)
        {
            _levelManager = levelManager;
            _dataFetcher = dataFetcher;
            _editorSettingsProvider = editorSettingsProvider;
            _pageModel = pageModel;
            _wardrobeStore = wardrobeStore;
            _userData = userData;
            _shoppingCartInfoModel = shoppingCartInfoModel;
        }

        public void ShowWardrobe(long categoryId, long? subcategoryId)
        {
            _wardrobeUmaEditor.Show(categoryId, subcategoryId);
        }
        
        protected override void OnInitialized()
        {
            _shoppingCart.ItemSelectionChanged += OnShoppingCartSelectionChanged;
            
            _levelManager.OnOutfitUpdated += OnOutfitUpdated;
            
            InitializeWardrobe();
        }

        protected override void BeforeCleanUp()
        {
            _shoppingCart.ItemSelectionChanged -= OnShoppingCartSelectionChanged;
            
            _levelManager.OnOutfitUpdated -= OnOutfitUpdated;
            
            _wardrobeUmaEditor.CleanUp();
        }

        private async void InitializeWardrobe()
        {
            var outfitsUsedInLevel = _levelManager.CurrentLevel.Event.SelectMany((ev) => ev.CharacterController.Select(controller => controller.OutfitId)).ToHashSet();
            var categoryTypeId = _dataFetcher.MetadataStartPack.WardrobeCategoryTypes.Last().Id;
            var defaultEditorSettings = await _editorSettingsProvider.GetDefaultEditorSettings();
            
            var args = new UmaEditorArgs
            {
                BackButtonAction = () => OnMoveNext(false),
                ConfirmAction = _ => OnMoveNext(false),
                ClickedOutside = () => OnMoveNext(false),
                SaveOutfitAsFavouriteAction = SaveOutfitAsFavourite,
                // character controller data is not used in UmaLevelEditor
                Character = null,
                Outfit = null,
                CharacterEditorSettings = defaultEditorSettings.CharacterEditorSettings,
                ConfirmActionType = CharacterEditorConfirmActionType.SaveOutfitAsAutomatic,
                CategoryTypeId = categoryTypeId,
                OutfitsUsedInLevel = outfitsUsedInLevel,
                Gender = _dataFetcher.MetadataStartPack.GetGenderById(GenderId)
            };

            _wardrobeUmaEditor.Initialize(args);
        }
        
        private void OnMoveNext(bool saveManual)
        {
            if (!_levelManager.IsChangingWardrobe && !_wardrobeStore.IsProcessingPurchase)
            {
                HideWardrobePanel(saveManual);
            }
        }

        private async void HideWardrobePanel(bool saveManual)
        {
            if (_closingWardrobePanel) return;
            
            _closingWardrobePanel = true;
            
            if (_pageModel.PrevState != LevelEditorState.Dressing && _shoppingCartHelper.HasNotPurchasedItems)
            {
                var purchasedAll = await TryPurchaseAllAssetsAsync();
                if (!purchasedAll)
                {
                    _closingWardrobePanel = false;
                    return;
                }
            }
            
            _userBalanceView.SetActive(false);
            _pageModel.OnOutfitSaveStarted();

            await _wardrobeUmaEditor.UpdateOutfitThumbnail();
            
            if (_pageModel.PrevState == LevelEditorState.Default)
            {
                await _levelManager.SaveEditedOutfit(saveManual);
                // TODO: quick and dirty solution - outfit saving should be done through OutfitsManager for proper UI updates
                _wardrobeUmaEditor.OnOutfitAdded(null);
            }
            
            _pageModel.OnOutfitSaved();

            if (saveManual)
            {
                _closingWardrobePanel = false;
                return;
            }
            
            _pageModel.EnableOutfitChange();
            _wardrobeUmaEditor.Hide();
            _pageModel.ReturnToPrevState();
            
            _closingWardrobePanel = false;
        }

        private async void SaveOutfitAsFavourite()
        {
            try
            {
                if (_closingWardrobePanel) return;

                _closingWardrobePanel = true;

                if (_shoppingCartHelper.HasNotPurchasedItems)
                {
                    var purchasedAll = await TryPurchaseAllAssetsAsync();
                    if (!purchasedAll)
                    {
                        _closingWardrobePanel = false;
                        return;
                    }
                }
                
                _pageModel.OnOutfitSaveStarted();

                _shoppingCartInfoModel.RefreshState();

                await _wardrobeUmaEditor.UpdateOutfitThumbnail();
                await _levelManager.SaveEditedOutfit(true);
                // TODO: quick and dirty solution - outfit saving should be done through OutfitsManager for proper UI updates
                _wardrobeUmaEditor.OnOutfitAdded(null);

                _pageModel.OnOutfitSaved();

                _closingWardrobePanel = false;
            }
            catch (Exception e)
            {
                Debug.LogException(e);
            }
        }

        private async Task<bool> TryPurchaseAllAssetsAsync()
        {
            var tcs = new TaskCompletionSource<bool>();

            _shoppingCartHelper.ShoppingCartClosed += OnPanelClosed;
            _shoppingCartHelper.Show(() => tcs.TrySetResult(true));

            _userBalanceView.Initialize(new StaticUserBalanceModel(_userData));
            _userBalanceView.SetActive(true);

            await tcs.Task;

            return !_shoppingCartHelper.HasNotPurchasedItems;
            
            void OnPanelClosed()
            {
                _shoppingCartHelper.ShoppingCartClosed -= OnPanelClosed;
                
                tcs.TrySetResult(true);
            }
        }

        private void OnShoppingCartSelectionChanged(IEntity wardrobe, bool isSelected)
        {
            _wardrobeUmaEditor.OnWardrobeSelected(wardrobe);
        }
        
        private void OnOutfitUpdated(WardrobeFullInfo[] _)
        {
            _wardrobeUmaEditor.UpdateSelected();
            _shoppingCart.Unlock();
        }
    }
}