using System;
using System.Linq;
using Bridge.Models.ClientServer.Assets;
using Extensions;
using Modules.AssetsStoraging.Core;
using Modules.LevelManaging.Editing.LevelManagement;
using UIManaging.Pages.Common.TabsManager;
using UIManaging.Pages.LevelEditor.Ui.SelectionViews.AssetSelection.PaginationLoaders;
using UIManaging.Pages.LevelEditor.Ui.SelectionViews.AssetSelection.SelectionHandlers;
using Event = Models.Event;
using Resolution = Bridge.Models.Common.Files.Resolution;

namespace UIManaging.Pages.LevelEditor.Ui.SelectionViews.AssetSelection.AssetSelectors
{
    public class BodyAnimationAssetSelector : PaginatedAssetSelectorModel<BodyAnimationPaginationLoader, BodyAnimationInfo>
    {
        public override DbModelType AssetType => DbModelType.BodyAnimation;
        
        public long? MainMovementTypeId
        {
            get => RecommendedLoader?.MainMovementTypeId;

            set
            {
                if (RecommendedLoader != null && ShowRecommendedCategory && RecommendedLoader.MainMovementTypeId != value)
                {
                    RecommendedLoader.MainMovementTypeId = value;
                    RequestScrollPositionUpdate = true;
                    ResetCache();
                }
            }
        }

        public long[] AllMovementTypeIds
        {
            set
            {
                foreach (var loader in Loaders.Values)
                {
                    loader.AllMovementTypeIds = value;
                }
            }
        }

        public long RaceId
        {
            set
            {
                foreach (var loader in Loaders.Values)
                {
                    loader.RaceId = value;
                }
            }
        }

        public int CharacterCount
        {
            get => _characterCount;
            set
            {
                if (_characterCount != value)
                {
                    RequestScrollPositionUpdate = true;
                    ResetCache();
                    
                    if (RecommendedLoader != null)
                    {
                        RecommendedLoader.CharacterCount = value;
                    }
                    
                    if (MyAssetsLoader != null)
                    {
                        MyAssetsLoader.CharacterCount = value;
                    }

                    foreach (var loader in Loaders.Values)
                    {
                        loader.CharacterCount = value;
                    }
                    
                    _characterCount = value;
                }
            }
        }

        private int _characterCount;
        private readonly bool _showRevertButton;
        
        public BodyAnimationAssetSelector(PaginatedAssetSelectorParameters<BodyAnimationPaginationLoader, BodyAnimationInfo> parameters,
            bool showRevertButton = true) : base(parameters)
        {
            _showRevertButton = showRevertButton;
            
            var assetSelectionHandler = new SingleItemAssetSelectionHandler(1, false);
            SetAssetSelectionHandler(assetSelectionHandler);
        }

        public override bool ShouldShowRevertButton()
        {
            return _showRevertButton;
        }

        public override void SetSelectedItemsAsInEvent(ILevelManager levelManager, Event @event,
            IDataFetcher dataFetcher = null,
            bool silent = true)
        {
            base.SetSelectedItemsAsInEvent(levelManager, @event, dataFetcher, silent);

            if (!silent)
            {
                AssetSelectionHandler.UnselectAllSelectedItems();
            }

            if (levelManager.EditingCharacterSequenceNumber < 0)
            {
                SetSelectedItems(GetSameSelectedBodyAnimationForAllCharacters(@event), silent: silent);
                return;
            }
            
            var targetCharacterController = @event.GetCharacterController(levelManager.EditingCharacterSequenceNumber);

            if (targetCharacterController == null)
            {
                targetCharacterController = @event.GetFirstCharacterController();
            }

            var activeBodyAnimation = targetCharacterController.GetBodyAnimation();

            if (dataFetcher != null)
            {
                var assetData = dataFetcher.DefaultUserAssets.PurchasedAssetsData;
                var purchasedIds = assetData.GetPurchasedAssetsByType(DbModelType.BodyAnimation);
            
                AddItems(new[] { new AssetSelectionBodyAnimationModel(
                                 Resolution._128x128, activeBodyAnimation,
                                 dataFetcher.MetadataStartPack.BodyAnimationCategories.First(x => x.Id == activeBodyAnimation.BodyAnimationCategoryId).Name)
                             {
                                 IsPurchased = purchasedIds.Contains(activeBodyAnimation.Id)
                             }
                });
            }
            
            SetSelectedItems(new[] { activeBodyAnimation.Id }, 
                             new[] { activeBodyAnimation.BodyAnimationCategoryId }, silent);
        }

        public override bool AreSelectedItemsAsInEvent(ILevelManager levelManager, Event @event)
        {
            if (AssetSelectionHandler.SelectedModels.Count == 0)
            {
                return false;
            }
            
            if (levelManager.EditingCharacterSequenceNumber < 0)
            {
                var ids = GetSameSelectedBodyAnimationForAllCharacters(@event);
                return ids.Length == 1 && AssetSelectionHandler.SelectedModels[0].ItemId == ids[0];
            }
            
            var targetCharacterController = @event.GetCharacterController(levelManager.EditingCharacterSequenceNumber) ??
                                            @event.GetFirstCharacterController();

            var activeBodyAnimationId = targetCharacterController.GetBodyAnimationId();
            
            return AssetSelectionHandler.SelectedModels[0].ItemId == activeBodyAnimationId;
        }

        public long? GetCurrentBodyAnimationId(ILevelManager levelManager, Event @event)
        {
            if (levelManager.EditingCharacterSequenceNumber < 0)
            {
                var bodyAnims = GetSameSelectedBodyAnimationForAllCharacters(@event);
                return bodyAnims.Length > 0 ? (long?)bodyAnims[0] : null;
            }
            
            var targetCharacterController = @event.GetCharacterController(levelManager.EditingCharacterSequenceNumber);

            if (targetCharacterController == null)
            {
                targetCharacterController = @event.GetFirstCharacterController();
            }

            return targetCharacterController.GetBodyAnimation()?.Id;
        }

        private long[] GetSameSelectedBodyAnimationForAllCharacters(Event @event)
        {
            var uniqueBodyAnimationIds = @event.GetUniqueBodyAnimationIds();
            return uniqueBodyAnimationIds.Length == 1 ? uniqueBodyAnimationIds : Array.Empty<long>(); 
        }

        public void ResetCache()
        {
            foreach (var loader in Loaders.Values)
            {
                loader.Reset();
            }
            
            MyAssetsLoader?.Reset();
            RecommendedLoader?.Reset();
            SearchLoader?.Reset();
        }
        
        protected override void SetupTabManagerArgs()
        {
            var tabs = GetTabs();
            var initialIndex = tabs.Any() ? GetInitialTabIndex(tabs) : 0;
            
            TabsManagerArgs = new AssetCategoryTabsManagerArgs(tabs, initialIndex, ShowMyAssetsCategory, () => ShowRecommendedCategory && MainMovementTypeId.HasValue);
        }
        
        public override void SetFilter(string filter = null)
        {
            base.SetFilter(filter);
            
            if (string.IsNullOrEmpty(filter))
            {
                return;
            }

            if (SearchLoader != null)
            {
                SearchLoader.CharacterCount = CharacterCount;
            }
        }
    }
}