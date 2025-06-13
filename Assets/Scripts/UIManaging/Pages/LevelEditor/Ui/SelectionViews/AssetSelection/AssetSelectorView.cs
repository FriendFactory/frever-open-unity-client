using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Abstract;
using Common;
using DG.Tweening;
using EnhancedUI.EnhancedScroller;
using Extensions;
using Modules.Amplitude;
using JetBrains.Annotations;
using Modules.AssetsStoraging.Core;
using Modules.LevelManaging.Assets;
using Modules.LevelManaging.Editing.LevelManagement;
using Modules.LevelManaging.Editing.Templates;
using UIManaging.Common.SearchPanel;
using UIManaging.Pages.Common.TabsManager;
using UIManaging.Pages.LevelEditor.Ui.AssetSettingsViews;
using UIManaging.Pages.LevelEditor.Ui.SelectionItems;
using UIManaging.Pages.LevelEditor.Ui.SelectionViews.AssetSelection.AssetSelectors;
using UIManaging.Pages.LevelEditor.Ui.SelectionViews.AssetSelection.PaginationLoaders;
using UIManaging.Pages.LevelEditor.Ui.SelectionViews.AssetSelection.UIAnimation;
using UIManaging.Pages.LevelEditor.Ui.SelectionViews.Characters;
using UIManaging.Pages.LevelEditor.Ui.SelectionViews.Uploading;
using UnityEngine;
using UnityEngine.UI;
using Zenject;
using Event = Models.Event;

namespace UIManaging.Pages.LevelEditor.Ui.SelectionViews.AssetSelection
{
    internal class AssetSelectorView : BaseContextDataView<MainAssetSelectorModel>
    {
        private const float PREV_PAGE_SCROLL_POSITION_THRESHOLD = 0.05f;
        private const float NEXT_PAGE_SCROLL_POSITION_THRESHOLD = 0.1f;
        private const float NEXT_PAGE_SCROLL_POSITION_THRESHOLD_EXPANDED = 0.2f;
        private const int AMOUNT_OF_ITEMS_FOR_SHRINKED_GRID = 4;
        private const int INITIAL_PAGE_SIZE = 8;
        private const int EXPANDED_PAGE_SIZE = 28;
        private const float DEFAULT_SHRINK_SIZE = 550;
        private const float CHARACTER_SHRINK_SIZE = 650;

        [SerializeField] private LayoutElement _baseGridElementView;
        [SerializeField] private EnhancedScroller _collapsedScroller;
        [SerializeField] private EnhancedScroller _expandedScroller;
        [SerializeField] private SelectableEnhancedScrollerGridSpawner _mainGridSpawner;
        [SerializeField] private SelectableEnhancedScrollerGridSpawner _subGridSpawner;
        [SerializeField] private AssetSelectorAnimator assetSelectorAnimator;
        [SerializeField] private AssetCategoryTabsManagerView _tabsManagerView;
        [SerializeField] private CanvasGroup _canvasGroup;
        [SerializeField] private ExpandSelectionViewButton _expandButton;
        [SerializeField] private Button _revertToDefaultButton;  
        
        [Header("Contextual objects")]
        [SerializeField] private GameObject _spawnFormationPanel;
        [SerializeField] private AssetInformationAnimator _assetInformationAnimator;
        [SerializeField] private CanvasGroup _subAssetViewPanelCanvasGroup;
        [SerializeField] private CharactersSelectionPanel _charactersPanel;
        [SerializeField] private SwitchableCharactersSelectionPanel _switchableCharactersSelectionPanel;
        [SerializeField] private UploadingPanel _uploadingPanel;
        [SerializeField] private GameObject _outfitPanel;
        
        [Header("Rows prefabs")] 
        [SerializeField] private EnhancedScrollerCellView _gifsRow;
        [SerializeField] private EnhancedScrollerCellView _staticImagesRow;
        [SerializeField] private EnhancedScrollerCellView _switchableCharactersStaticImagesRow;
        [SerializeField] private EnhancedScrollerCellView _staticCharacterImagesRow;
        [SerializeField] private EnhancedScrollerCellView _staticImagesRowVertical;
        [SerializeField] private EnhancedScrollerCellView _spawnPositionStaticImagesRowVertical;
        [SerializeField] private EnhancedScrollerCellView _voiceFilterRow;
        [SerializeField] private EnhancedScrollerCellView _bodyAnimationRow;
        [SerializeField] private EnhancedScrollerCellView _outfitImagesRow;
        
        [Header("Asset settings")] 
        [SerializeField] private AssetSettingsView _bodyAnimaitonSettingsView;
        [SerializeField] private AssetSettingsView _setLocationSettingsView;
        [SerializeField] private AssetSettingsView _cameraAnimationSettingsView;
        [SerializeField] private AssetSettingsView _cameraFilterSettingsView;
        
        [Header("Assets search")]
        [SerializeField] private SearchPanelView _searchPanel;

        private AssetSettingsView _currentAssetSettingsView;
        private AdvancedAssetSettingsView _currentAdvancedAssetSettingsView;
        private Dictionary<Type, AssetSettingsView> _assetSettingsViews;
        private SelectionScrollData _selectionScrollData = new SelectionScrollData();

        private Dictionary<Type, EnhancedScrollerCellView> _rowsForTypes;
        private AssetSelectorModel _subAssetSelectorModel;
        private Sequence _fadePanelSequence;
        private bool _isUpdatingAsset;
        private ILevelManager _levelManager;
        private AmplitudeManager _amplitudeManager;
        private ITemplateProvider _templateProvider;
        private IDataFetcher _dataFetcher;
        private long? _searchSelectedItemId;
        private readonly List<CancellationTokenSource> _activeTokens = new List<CancellationTokenSource>();
        private Event _defaultEvent;
        private bool _requestShrinkedScrollPositionUpdate;
        private bool _requestExpandedScrollPositionUpdate;
        private readonly Dictionary<string, string> _searchQueryCache = new Dictionary<string, string>();

        private readonly DbModelType[] _displayAssetInfoTypes = { DbModelType.SetLocation, DbModelType.CameraFilterVariant, DbModelType.VoiceFilter };

        //---------------------------------------------------------------------
        // Ctors
        //---------------------------------------------------------------------

        [Inject]
        [UsedImplicitly]
        private void Construct(ILevelManager levelManager, AmplitudeManager amplitudeManager, ITemplateProvider templateProvider, IDataFetcher dataFetcher)
        {
            _levelManager = levelManager;
            _amplitudeManager = amplitudeManager;
            _templateProvider = templateProvider;
            _dataFetcher = dataFetcher;
        }

        //---------------------------------------------------------------------
        // Messages
        //---------------------------------------------------------------------

        private void Awake()
        {
            _rowsForTypes = new Dictionary<Type, EnhancedScrollerCellView>
            {
                {typeof(CameraAssetSelector), _gifsRow},
                {typeof(BodyAnimationAssetSelector), _bodyAnimationRow},
                {typeof(VfxAssetSelector), _gifsRow},
                {typeof(SetLocationAssetSelector), _staticImagesRow},
                {typeof(CharacterAssetSelector), _staticCharacterImagesRow},
                {typeof(CharacterAssetSwitchSelector), _switchableCharactersStaticImagesRow},
                {typeof(SpawnPositionAssetSelector), _spawnPositionStaticImagesRowVertical},
                {typeof(VoiceFilterAssetSelector), _voiceFilterRow},
                {typeof(OutfitAssetSelector), _outfitImagesRow},
                {typeof(CameraFilterAssetSelector), _staticImagesRow},
                {typeof(CameraFilterVariantAssetSelector), _staticImagesRowVertical},
            };

            _assetSettingsViews = new Dictionary<Type, AssetSettingsView>
            {
                {typeof(BodyAnimationAssetSelector), _bodyAnimaitonSettingsView},
                {typeof(SetLocationAssetSelector), _setLocationSettingsView},
                {typeof(CameraAssetSelector), _cameraAnimationSettingsView},
                {typeof(OutfitAssetSelector), _bodyAnimaitonSettingsView},
                {typeof(CameraFilterAssetSelector), _cameraFilterSettingsView},
            };

            foreach (var settingsView in _assetSettingsViews)
            {
                settingsView.Value.Setup();
            }

            CoroutineSource.Instance.ExecuteAtEndOfFrame(SetupPadding);
        }

        private void OnEnable()
        {
            _levelManager.SetLocationChangeFinished += RefreshSetLocationPanels;
            _tabsManagerView.TabsCreated += OnCategoryTabsCreated;
            _expandButton.AddListener(OnExpandButtonClicked);
        }

        private void OnDisable()
        {
            _tabsManagerView.TabsCreated -= OnCategoryTabsCreated;
            _levelManager.SetLocationChangeFinished -= RefreshSetLocationPanels;
            _expandButton.CleanUp();
            CancelAllActiveTasks();
            if (IsDestroyed) return;
            _subAssetViewPanelCanvasGroup.alpha = 0f;
            assetSelectorAnimator.ChangeRevertButtonState(false);
        }

        protected override void OnDestroy()
        {
            foreach (var settingsView in _assetSettingsViews)
            {
                settingsView.Value.CleanUp();
            }
            
            base.OnDestroy();
        }

        //---------------------------------------------------------------------
        // Public
        //---------------------------------------------------------------------

        public void HideAdvancedAssetSettingsView()
        {
            _currentAdvancedAssetSettingsView?.HideAdvancedSettingsView();
        }

        public void PlayShrinkAnimation(bool instant = false)
        {
            _searchPanel.Deselect();
            
            if (ContextData == null)
            {
                return;
            }

            assetSelectorAnimator.PlayShrinkAnimation(instant);
        }

        public void SetStartingItemToScrollPosition()
        {
            long targetItemId;
            
            if (IsExpandedOrExpanding())
            {
                targetItemId = _selectionScrollData.GetExpandedPositionForCategory(
                    ContextData.AssetType, GetCurrentCategoryIndex());
            }
            else
            {
                targetItemId = _selectionScrollData.GetShrinkedPositionForCategory(
                        ContextData.AssetType, GetCurrentCategoryIndex());
            }

            if (targetItemId != 0)
            {
                ContextData.SetStartingItem(GetCurrentCategoryIndex(), targetItemId);
            }
        }
        
        public void ApplySavedScrollPosition()
        {
            var models = GetItemsToShow();
            
            if (IsExpandedOrExpanding())
            {
                var targetItemId = _selectionScrollData.GetExpandedPositionForCategory(
                    ContextData.AssetType, GetCurrentCategoryIndex());
                var index = FindIndexByPredicate(models, item => item.ItemId != targetItemId);

                if (index < 0)
                {
                    index = 0;
                }
                
                _mainGridSpawner.SetScrollPosition(index / AMOUNT_OF_ITEMS_FOR_SHRINKED_GRID);
            }
            else
            {
                var targetItemId = _selectionScrollData.GetShrinkedPositionForCategory(
                        ContextData.AssetType, GetCurrentCategoryIndex());
                var index = FindIndexByPredicate(models, item => item.ItemId != targetItemId);

                if (index < 0)
                {
                    index = 0;
                }
                
                _mainGridSpawner.SetFirstRowScrollPosition(index, true);
            }
            
            UpdateScrollViews();
        }
        
        public bool IsExpandedOrExpanding()
        {
            return assetSelectorAnimator.IsExpanded || assetSelectorAnimator.IsExpanding;
        }
        
        //---------------------------------------------------------------------
        // Protected
        //---------------------------------------------------------------------

        protected override void OnInitialized()
        {
            _defaultEvent = _templateProvider.GetTemplateEventFromCache(_dataFetcher.DefaultTemplateId);
            
            ContextData.UpdateRequestValues();
            
            if (ContextData.TabsManagerArgs != null)
            {
                _tabsManagerView.Init(ContextData.TabsManagerArgs);
            }
            
            _mainGridSpawner.OnRowCreated += OnRowCreated;
            _levelManager.AssetUpdateCompleted += OnSelectedAssetChanged;
            _tabsManagerView.TabSelectionCompleted += OnTabSelectionCompleted;
            _tabsManagerView.TabSelectionStarted += OnTabSelectionStarted;
            _expandedScroller.scrollerScrolled += OnExpandedScrollerScrolled;
            _mainGridSpawner.EnhancedScroller.cellViewWillRecycle += OnCellViewWillRecycle;
            ContextData.OnSelectedItemChangedEvent += OnSelectedItemChanged;
            ContextData.OnSelectionChangedByCodeEvent += OnSelectionChangedByCode;
            ContextData.ItemsAdded += OnItemsAdded;
            _levelManager.AssetUpdateStarted += OnAssetStartUpdating;
            _levelManager.AssetUpdateCompleted += OnAssetStopUpdating;
            ContextData.AssetSelectionHandler.OnAmountOfSelectedItemsChangedEvent += OnAmountOfSelectedItemsChanged;
            
            assetSelectorAnimator.SetShrinkedAndExpandedSizes(ContextData.AssetType == DbModelType.Character? CHARACTER_SHRINK_SIZE : DEFAULT_SHRINK_SIZE);
            assetSelectorAnimator.OnExpandAnimationStartedEvent += OnExpandAnimationStarted;
            assetSelectorAnimator.OnShrinkAnimationStartedEvent += OnShrinkAnimationStarted;
            assetSelectorAnimator.OnShrinkAnimationCompletedEvent += OnShrinkAnimationComplete;
            
            ContextData.SetDefaultPageSize(INITIAL_PAGE_SIZE);
            OnStart();
            RefreshFormationPanel();
            RefreshCharactersPanel();
            RefreshCreateOutfitPanel();
            TryDisplayAssetInformationPanel(ContextData.AssetType);

            var setLocationAsset = _levelManager.GetTargetEventSetLocationAsset();
            if (setLocationAsset != null) RefreshSetLocationPanels(setLocationAsset);
            
            ContextData.OnOpened();
        }
        
        protected override void BeforeCleanup()
        {
            base.BeforeCleanup();
            CancelAllActiveTasks();
            SaveCurrentScrollPositions();
            
            if (ContextData != null)
            {
                ContextData.OnSelectedItemChangedEvent -= OnSelectedItemChanged;
                ContextData.OnSelectionChangedByCodeEvent -= OnSelectionChangedByCode;
                _levelManager.AssetUpdateStarted -= OnAssetStartUpdating;
                _levelManager.AssetUpdateCompleted -= OnAssetStopUpdating;
                ContextData.ItemsAdded -= OnItemsAdded;
                ContextData.AssetSelectionHandler.OnAmountOfSelectedItemsChangedEvent -= OnAmountOfSelectedItemsChanged;
            }

            _mainGridSpawner.CleanUp();
            if (_subAssetSelectorModel != null)
            {
                _subGridSpawner.CleanUp();
            }

            
            _expandedScroller.scrollerScrolled -= OnExpandedScrollerScrolled;
            _mainGridSpawner.OnRowCreated -= OnRowCreated;
            _mainGridSpawner.EnhancedScroller.cellViewWillRecycle -= OnCellViewWillRecycle;
            
            _levelManager.AssetUpdateCompleted -= OnSelectedAssetChanged;
            _tabsManagerView.TabSelectionCompleted -= OnTabSelectionCompleted;
            _tabsManagerView.TabSelectionStarted -= OnTabSelectionStarted;

            assetSelectorAnimator.OnExpandAnimationStartedEvent -= OnExpandAnimationStarted;
            assetSelectorAnimator.OnShrinkAnimationStartedEvent -= OnShrinkAnimationStarted;
            assetSelectorAnimator.OnShrinkAnimationCompletedEvent -= OnShrinkAnimationComplete;
            
            PlayShrinkAnimation(true);

            _revertToDefaultButton.onClick.RemoveListener(OnRevertToDefaultButtonClicked);
            
            _searchPanel.InputCompleted -= OnSearchInputChanged;
            _searchPanel.InputCleared -= OnSearchInputCleared;
            _searchPanel.ClearInputButtonClicked -= OnClearSearchInputClicked;
            _searchPanel.OnSelect.RemoveListener(OnSearchInputSelected);
            _searchPanel.OnDeselect.RemoveListener(OnSearchInputDeselected);

            if (_currentAssetSettingsView != null)
            {
                _currentAssetSettingsView.Hide();
            }

            ContextData?.OnClosed();
        }


        //---------------------------------------------------------------------
        // Helpers
        //---------------------------------------------------------------------
        
        private void SetupPadding()
        {
            var scrollerWidth = _mainGridSpawner.EnhancedScroller.GetComponent<RectTransform>().GetSize().x;
            var itemWidth = _baseGridElementView.minWidth;
            var rowPadding = _gifsRow.GetComponent<EnhancedScroller>().padding.left;
            var offset = (int)(scrollerWidth - itemWidth * AMOUNT_OF_ITEMS_FOR_SHRINKED_GRID) / 2 - rowPadding;
            _expandedScroller.padding = new RectOffset(offset,
                                                       offset, 
                                                       _expandedScroller.padding.top, 
                                                       _expandedScroller.padding.bottom);
            _expandedScroller.GetComponentInChildren<LayoutGroup>().padding = _expandedScroller.padding;
        }
        
        private void PlayExpandAnimation()
        {
            if (ContextData == null)
            {
                return;
            }
            
            assetSelectorAnimator.PlayExpandAnimation();
        }

        private void OnRowCreated(SelectableOptimizedItemsRow rowView)
        {
            rowView.GetComponent<EnhancedScroller>().scrollerScrolled += OnShrinkedScrollerScrolled;
        }

        private void OnCellViewWillRecycle(EnhancedScrollerCellView rowView)
        {
            rowView.GetComponent<EnhancedScroller>().scrollerScrolled -= OnShrinkedScrollerScrolled;
        }

        private void OnExpandedScrollerScrolled(EnhancedScroller scroller, Vector2 val, float scrollPosition)
        {
            UpdateExpandedScroll();
        }
        
        private void UpdateExpandedScroll()
        {
            if (ContextData == null || IsDestroyed)
            {
                return;
            }

            var scrolledToNextPage = _expandedScroller.NormalizedScrollPosition <= NEXT_PAGE_SCROLL_POSITION_THRESHOLD_EXPANDED;

            if (!ContextData.AwaitingData && !ContextData.IsEndOfScroll(GetCurrentCategoryIndex()) && scrolledToNextPage)
            {
                ContextData.DownloadNextPage(GetCurrentCategoryIndex());
            }

            var scrolledToPrevPage = _expandedScroller.NormalizedScrollPosition >= 1 - PREV_PAGE_SCROLL_POSITION_THRESHOLD;

            if (!ContextData.AwaitingData && !ContextData.IsStartOfScroll(GetCurrentCategoryIndex())  && scrolledToPrevPage)
            {
                ContextData.DownloadFirstPage(GetCurrentCategoryIndex());
            }
        }

        private void OnShrinkedScrollerScrolled(EnhancedScroller scroller, Vector2 val, float scrollPosition)
        {
            UpdateShrinkedScroll();
        }
        
        private void UpdateShrinkedScroll() 
        {
            if (ContextData == null || IsDestroyed || _mainGridSpawner.EnhancedScroller.GetCellViewAtDataIndex(0) == null)
            {
                return;
            }

            var innerScroll = _mainGridSpawner.EnhancedScroller.GetCellViewAtDataIndex(0).GetComponent<EnhancedScroller>();
            var scrolledToNextPage = innerScroll.NormalizedScrollPosition >= 1 - NEXT_PAGE_SCROLL_POSITION_THRESHOLD;

            if (!ContextData.AwaitingData && !ContextData.IsEndOfScroll(GetCurrentCategoryIndex()) && scrolledToNextPage)
            {
                ContextData.DownloadNextPage(GetCurrentCategoryIndex());
            }

            var scrolledToPrevPage = innerScroll.NormalizedScrollPosition <= PREV_PAGE_SCROLL_POSITION_THRESHOLD;

            if (!ContextData.AwaitingData && !ContextData.IsStartOfScroll(GetCurrentCategoryIndex()) && scrolledToPrevPage)
            {
                ContextData.DownloadFirstPage(GetCurrentCategoryIndex());
            }
        }
        
        private void OnExpandButtonClicked()
        {
            if (IsExpandedOrExpanding())
            {
                PlayShrinkAnimation();
            }
            else
            {
                PlayExpandAnimation();
            }
        }
        
        private void OnSelectedItemChanged(AssetSelectionItemModel selectionItemModel)
        { 
            if (selectionItemModel.IsSelected && assetSelectorAnimator.IsExpanded)
            {
                SaveCurrentScrollPositions(); 
            }

            if (selectionItemModel.IsSelected && _searchPanel.HasInput && assetSelectorAnimator.IsExpanded)
            {
                _searchSelectedItemId = selectionItemModel.ItemId;
            }

            if (_isUpdatingAsset) return;
            

            if (ContextData.ShouldShowRevertButton())
            {
                var showRevertButton = !ContextData.AreSelectedItemsAsInEvent(_levelManager, _defaultEvent);
                assetSelectorAnimator.ChangeRevertButtonState(showRevertButton);
            }
        }

        private async void UpdateScrollsForEvents(bool allowAutoSwitchToRecommended = false)
        {
            var tokenSource = RegisterNewCancellationTokenSource();
            assetSelectorAnimator.PlayNotFoundScreenState(false, ContextData.NoAvailableAssetsMessage, true);

            if (ContextData.RequestScrollPositionUpdate)
            {
                ContextData.SetTabToSelection(allowAutoSwitchToRecommended);
                ContextData.UpdateTabs();
            } 
            else 
            {
                SetStartingItemToScrollPosition();
            }
            
            await ContextData.DownloadInitialPage(GetCurrentCategoryIndex(), true, tokenSource.Token);

            UnregisterAndDisposeCancellationTokenSource(tokenSource);
            
            if (tokenSource.IsCancellationRequested)
            {
                return;
            }
            
            var selectedItemId = ContextData.GetSelectedItemIdInCategory();

            if (selectedItemId >= 0)
            {
                if (IsExpandedOrExpanding())
                {
                    _requestShrinkedScrollPositionUpdate = true;
                    _selectionScrollData.SetExpandedViewScrollPosition(selectedItemId,
                                                                       ContextData.AssetType, 
                                                                       ContextData.TabsManagerArgs.SelectedTabIndex);
                }
                else
                {
                    _requestExpandedScrollPositionUpdate = true;
                    _selectionScrollData.SetShrinkedViewScrollPosition(selectedItemId, 
                                                                       ContextData.AssetType, 
                                                                       ContextData.TabsManagerArgs.SelectedTabIndex);
                }
            }
            
            if (ContextData.IsRecommendedCategory && ContextData.RequestScrollPositionUpdate) // check if we just moved into recommended category
            {
                ContextData.SetTabToSelection(allowAutoSwitchToRecommended); // try to move to another tab in case selected item is not recommended

                if (!ContextData.IsRecommendedCategory) // if previous call moved us out of recommended tab - update tab UI and call scroll update again
                {
                    ContextData.RequestScrollPositionUpdate = false;
                    ContextData.UpdateTabs();
                    UpdateScrollsForEvents(allowAutoSwitchToRecommended);
                    return; // other actions are not needed as new initial page will be requested
                }
            }
            
            ContextData.RequestScrollPositionUpdate = false;
            
            var shouldShowRevertButton = ContextData.ShouldShowRevertButton() && !ContextData.AreSelectedItemsAsInEvent(_levelManager, _defaultEvent);
            assetSelectorAnimator.ChangeRevertButtonState(shouldShowRevertButton);
            RefreshAllCurrentItems(true);
            
            ApplySavedScrollPosition();

            if (_subAssetSelectorModel != null)
            {
                SetupSubAssets();
            }
        }

        private CancellationTokenSource RegisterNewCancellationTokenSource()
        {
            var cancellationToken = new CancellationTokenSource();
            _activeTokens.Add(cancellationToken);
            return cancellationToken;
        }

        private void UnregisterAndDisposeCancellationTokenSource(CancellationTokenSource source)
        {
            if (!_activeTokens.Contains(source)) return;
            _activeTokens.Remove(source);
            source.Dispose();
        }
        
        private void OnSelectedAssetChanged(DbModelType dbModelType)
        {
            TryDisplayAssetInformationPanel(dbModelType);
        }

        private void OnAssetStartUpdating(DbModelType type, long id)
        {
            _isUpdatingAsset = true;
            _revertToDefaultButton.interactable = false;
        }

        private void OnAssetStopUpdating(DbModelType type)
        {
            _isUpdatingAsset = false;
            var revertInteractable = !ContextData.AreSelectedItemsAsInEvent(_levelManager, _defaultEvent);
            _revertToDefaultButton.interactable = revertInteractable;

            if (ContextData.ShouldShowRevertButton())
            {
                assetSelectorAnimator.ChangeRevertButtonState(revertInteractable);
            }
        }

        private void OnItemsAdded(AssetSelectionItemModel[] addedItems, bool append)
        {
            RefreshAllCurrentItems(false, append);
        }

        private void OnAmountOfSelectedItemsChanged()
        {
            RefreshFormationPanel();
        }

        private void TryDisplayAssetInformationPanel(DbModelType type)
        {
            if (!ShouldDisplayAssetInformation(type)) return;

            var selectedModel = GetFirstSelectedItemModel(type);
            _assetInformationAnimator.ShowInformationPanel(selectedModel);
        }

        private void RefreshFormationPanel()
        {
            _spawnFormationPanel.SetActive(ShouldShowSpawnFormationPanel());
        }

        private bool ShouldShowSpawnFormationPanel()
        {
            return ContextData.ShouldShowSpawnFormationPanel();
        }

        private void RefreshCharactersPanel()
        {
            var shouldShowCharactersPanel = ContextData.ShouldShowCharactersSelectionPanel();
            _charactersPanel.gameObject.SetActive(shouldShowCharactersPanel);

            if (shouldShowCharactersPanel)
            {
                _charactersPanel.UpdateCharacterButtons();
            }

            var shouldShowSwitchCharactersPanel = ContextData.ShouldShowCharactersSwitchablePanel();
            _switchableCharactersSelectionPanel.gameObject.SetActive(shouldShowSwitchCharactersPanel);

            if (shouldShowSwitchCharactersPanel)
            {
                _switchableCharactersSelectionPanel.UpdateCharacterButtons();
            }
        }

        private void RefreshSetLocationPanels(ISetLocationAsset setLocationAsset)
        {
            var setLocation = setLocationAsset.RepresentedModel;
            var shouldShowUploadingPanel = ContextData.ShouldShowUploadingPanel(setLocation);
            _uploadingPanel.gameObject.SetActive(shouldShowUploadingPanel);

            var shouldShowTimeOfDayJogWheel = ContextData.ShouldShowTimeOfDayJogWheel(setLocationAsset);
            _setLocationSettingsView.gameObject.SetActive(shouldShowTimeOfDayJogWheel);
            
            if (shouldShowTimeOfDayJogWheel && !IsExpandedOrExpanding())
            {
                _setLocationSettingsView.PartialDisplay();
            }
            else
            {
                _setLocationSettingsView.PartialHide();
            }
            
            OnSelectedAssetChanged(DbModelType.CharacterSpawnPosition);
        }

        private void OnStart()
        {
            var modelType = ContextData.GetType();
            
            _currentAssetSettingsView = _assetSettingsViews.FirstOrDefault(x => x.Key == modelType).Value;
            _currentAssetSettingsView?.Display();

            var advancedSettingsView = _currentAssetSettingsView as AdvancedAssetSettingsView;
            advancedSettingsView?.SetMainViewCanvasGroup(_canvasGroup);
            _currentAdvancedAssetSettingsView = advancedSettingsView;

            _searchPanel.InputCompleted += OnSearchInputChanged;
            _searchPanel.InputCleared += OnSearchInputCleared;
            _searchPanel.ClearInputButtonClicked += OnClearSearchInputClicked;
            _searchPanel.OnSelect.AddListener(OnSearchInputSelected);
            _searchPanel.OnDeselect.AddListener(OnSearchInputDeselected);
            
            var shouldShowRevertButton = ContextData.ShouldShowRevertButton();
            _revertToDefaultButton.gameObject.SetActive(shouldShowRevertButton);
            
            if (shouldShowRevertButton)
            {
                _revertToDefaultButton.onClick.AddListener(OnRevertToDefaultButtonClicked);
                var revertInteractable = !ContextData.AreSelectedItemsAsInEvent(_levelManager, _defaultEvent);
                _revertToDefaultButton.interactable = revertInteractable;
            }
            
            var rowPrefab = _rowsForTypes[modelType];
            _mainGridSpawner.SetRowSize(rowPrefab.GetComponent<RowData>().RowSize);
            _mainGridSpawner.SetRowPrefab(rowPrefab);
            _mainGridSpawner.SetAmountOfItemsPerRowSilent(AMOUNT_OF_ITEMS_FOR_SHRINKED_GRID);

            var isGridScrollable = IsExpandedOrExpanding();
            _mainGridSpawner.SetGridScrollable(isGridScrollable);
            _mainGridSpawner.SetRowsScrollable(!isGridScrollable);
            _mainGridSpawner.Initialize(ContextData.GridSpawnerModel);
            _subAssetSelectorModel = ContextData.SubSelector;
            SetupContent();

            if (_subAssetSelectorModel != null)
            {
                FadeInSubAssetViewPanel();
            }
            else
            {
                FadeOutSubAssetViewPanel();
            }

            UpdateScrollsForEvents(true);
        }

        private void SetupContent()
        {
            assetSelectorAnimator.SearchPanelEnabled = ContextData.IsSearchable();
            _expandButton.UseSearchIcon(ContextData.IsSearchable());

            var cachedSearch = string.Empty;
            
            if (assetSelectorAnimator.SearchPanelEnabled)
            {
                _searchQueryCache.TryGetValue(ContextData.DisplayName, out cachedSearch);
            }
            
            assetSelectorAnimator.AlwaysInstantAnimations = true;

            _searchPanel.SetTextWithoutNotify(cachedSearch ?? string.Empty);

            if (string.IsNullOrEmpty(cachedSearch))
            {
                OnSearchInputCleared();
            }
            else
            {
                OnSearchInputChanged(cachedSearch);
            }

            assetSelectorAnimator.AlwaysInstantAnimations = false;
        }
        
        private void SetupSubAssets()
        {
            var modelType = _subAssetSelectorModel.GetType();
            var rowPrefab = _rowsForTypes[modelType];
            _subGridSpawner.SetRowPrefab(rowPrefab);

            _subGridSpawner.SetGridScrollable(false);
            _subGridSpawner.Initialize(_subAssetSelectorModel.GridSpawnerModel);
            var items = _subAssetSelectorModel.GetItemsToShow();
            _subAssetSelectorModel.GridSpawnerModel.SetItems(items);
        }

        private void OnRevertToDefaultButtonClicked()
        {
            if (_levelManager.IsChangingAsset || _defaultEvent == null)
            {
                return;
            }
            
            ContextData.SetSelectedItemsAsInEvent(_levelManager, _defaultEvent, silent: false);
            _subAssetSelectorModel?.SetSelectedItemsAsInEvent(_levelManager, _defaultEvent, silent: false);
        }

        private void OnTabSelectionStarted(int index)
        {
            SaveCurrentScrollPositions();
            ContextData.InterruptDownload();
            _mainGridSpawner.ContextData.SetItems(Array.Empty<AssetSelectionItemModel>());
            assetSelectorAnimator.PlayNotFoundScreenState(false, ContextData.NoAvailableAssetsMessage, true);
        }

        private async void OnTabSelectionCompleted(int index)
        {
            _mainGridSpawner.ContextData.SetItems(Array.Empty<AssetSelectionItemModel>());
            assetSelectorAnimator.PlayNotFoundScreenState(false, ContextData.NoAvailableAssetsMessage, true);
            SetStartingItemToScrollPosition();

            var tokenSource = RegisterNewCancellationTokenSource();
            
            await ContextData.DownloadInitialPage(GetCurrentCategoryIndex(), token: tokenSource.Token);
            
            UnregisterAndDisposeCancellationTokenSource(tokenSource);
            if(tokenSource.IsCancellationRequested) return;
            
            RefreshAllCurrentItems(true);
            
            ApplySavedScrollPosition();
        }

        private void UpdateScrollViews()
        {
            if (IsExpandedOrExpanding())
            {
                _expandedScroller._RefreshActive();
            }
            else
            {
                var innerScroll = _mainGridSpawner.EnhancedScroller.GetCellViewAtDataIndex(0)?.GetComponent<EnhancedScroller>();

                if (innerScroll != null)
                {
                    innerScroll._RefreshActive();
                }
            }
        }
        
        private void OnSearchInputChanged(string input)
        {
            assetSelectorAnimator.PlayCategoryTabState(false);
            _searchQueryCache[ContextData.DisplayName] = input.ToLower();
            UpdateItemsForSearch(input.ToLower());
        }

        private void OnSearchInputCleared()
        {
            assetSelectorAnimator.PlayCategoryTabState(true);
            _searchQueryCache.Remove(ContextData.DisplayName);
            ContextData.SetFilter();
            RefreshAllCurrentItems(true);
        }

        private void OnSearchInputSelected(string text)
        {
            if (IsExpandedOrExpanding()) return;
            PlayExpandAnimation();
        }
        
        private void OnSearchInputDeselected(string text)
        {
            if (!_searchPanel.HasInput) return;
            
            StartCoroutine(SendAmplitudeSearchEventWithDelay());
        }

        private void OnClearSearchInputClicked(string lastInput)
        {
            if (!IsExpandedOrExpanding()) SendAmplitudeSearchAssetEvent(lastInput);
        }

        private int GetCurrentCategoryIndex() => ContextData.TabsManagerArgs.SelectedTabIndex;

        private void RefreshAllCurrentItems(bool withReload, bool append = true)
        {
            var items = GetItemsToShow();
                
            if (withReload)
            {
                _mainGridSpawner.ContextData.SetItems(items);
            }
            else
            {
                _mainGridSpawner.ContextData.AddItems(items.Where(item => _mainGridSpawner.ContextData.Items
                                                                     .All(item2 => item.ItemId != item2.ItemId)), append);
            }
            
            assetSelectorAnimator.PlayNotFoundScreenState(items.Count == 0, ContextData.NoAvailableAssetsMessage);
        }
        
        private async void UpdateItemsForSearch(string searchQuery)
        {
            ContextData.SetFilter(searchQuery);
            
            _mainGridSpawner.ContextData.SetItems(Array.Empty<AssetSelectionItemModel>());
            
            var tokenSource = RegisterNewCancellationTokenSource();
            
            await ContextData.DownloadInitialPage(GetCurrentCategoryIndex(), token:tokenSource.Token);
            
            UnregisterAndDisposeCancellationTokenSource(tokenSource);
            
            if (tokenSource.IsCancellationRequested)
            {
                return;
            }
            
            RefreshAllCurrentItems(true);
        }

        private async void OnExpandAnimationStarted()
        {
            SaveCurrentScrollPositions();
            _expandButton.UseSearchIcon(false);
            assetSelectorAnimator.PlayScrollersVisibilityState(true);
            assetSelectorAnimator.PlayNotFoundScreenState(false, ContextData.NoAvailableAssetsMessage, true);

            if (_mainGridSpawner.EnhancedScroller != _expandedScroller)
            {
                _mainGridSpawner.EnhancedScroller = _expandedScroller;
            }
            
            if (_subAssetSelectorModel != null) FadeOutSubAssetViewPanel();
            if (_currentAssetSettingsView != null) _currentAssetSettingsView.PartialHide();
            
            _mainGridSpawner.SetGridScrollable(true);
            _mainGridSpawner.SetRowsScrollable(false);
            _mainGridSpawner.SetFirstRowScrollPosition(0);
            
            ContextData.SetDefaultPageSize(EXPANDED_PAGE_SIZE);
            
            _mainGridSpawner.ContextData.SetItems(Array.Empty<AssetSelectionItemModel>());
            SetStartingItemToScrollPosition();

            var tokenSource = RegisterNewCancellationTokenSource();
            
            await ContextData.DownloadInitialPage(GetCurrentCategoryIndex(), true, tokenSource.Token);
            
            UnregisterAndDisposeCancellationTokenSource(tokenSource);
            if (tokenSource.IsCancellationRequested)
            {
                return;
            }
            
            RefreshAllCurrentItems(true);
            
            if (_requestExpandedScrollPositionUpdate)
            {
                var selectedItemId = ContextData.GetSelectedItemIdInCategory();
                
                _requestExpandedScrollPositionUpdate = false;

                if (selectedItemId >= 0)
                {
                    _selectionScrollData.SetExpandedViewScrollPosition(selectedItemId, 
                                                                       ContextData.AssetType, 
                                                                       ContextData.TabsManagerArgs.SelectedTabIndex);
                }
            }
            
            ApplySavedScrollPosition();
        }

        private void OnShrinkAnimationStarted()
        {
            SaveCurrentScrollPositions();
            assetSelectorAnimator.PlayNotFoundScreenState(false, ContextData.NoAvailableAssetsMessage, true);
            if (_subAssetSelectorModel != null) FadeInSubAssetViewPanel();
            if (_currentAssetSettingsView != null) _currentAssetSettingsView.PartialDisplay();
        }

        private async void OnShrinkAnimationComplete()
        {
            _expandButton.UseSearchIcon(ContextData.IsSearchable());
            assetSelectorAnimator.PlayScrollersVisibilityState(false);

            if (_mainGridSpawner.EnhancedScroller != _collapsedScroller)
            {
                _mainGridSpawner.EnhancedScroller = _collapsedScroller;
            }
            
            _mainGridSpawner.SetGridScrollable(false);
            _mainGridSpawner.SetRowsScrollable(true);
            
            ContextData.SetDefaultPageSize(8);
            
            _mainGridSpawner.ContextData.SetItems(Array.Empty<AssetSelectionItemModel>());
            SetStartingItemToScrollPosition();

            var tokenSource = RegisterNewCancellationTokenSource();
            
            await ContextData.DownloadInitialPage(GetCurrentCategoryIndex(), true, tokenSource.Token);
            
            UnregisterAndDisposeCancellationTokenSource(tokenSource);
            if (tokenSource.IsCancellationRequested)
            {
                return;
            }
            
            RefreshAllCurrentItems(true);
            
            if (_requestShrinkedScrollPositionUpdate) 
            {
                var selectedItemId = ContextData.GetSelectedItemIdInCategory();
                
                _requestShrinkedScrollPositionUpdate = false;

                if (selectedItemId >= 0)
                {
                    _selectionScrollData.SetShrinkedViewScrollPosition(selectedItemId, 
                                                                       ContextData.AssetType, 
                                                                       ContextData.TabsManagerArgs.SelectedTabIndex);
                }
            }
            
            ApplySavedScrollPosition();
        }

        private void SaveCurrentScrollPositions()
        {
            if (IsDestroyed)
            {
                return;
            } 
            
            if (assetSelectorAnimator.IsShrinking || assetSelectorAnimator.IsExpanded)
            {
                var scroller = _mainGridSpawner.EnhancedScroller;
                var index = scroller.NumberOfCells == 0 
                    ? 0 
                    : scroller.SnapCellViewIndex - scroller.StartCellViewIndex + scroller.StartDataIndex;
                var itemId = scroller.GetCellViewAtDataIndex(index)
                                            ?.GetComponent<SelectableOptimizedItemsRow>()
                                            ?.Items
                                            ?.FirstOrDefault()
                                            ?.ItemId ?? 0;
                
                _selectionScrollData.SetExpandedViewScrollPosition(itemId, ContextData.AssetType, GetCurrentCategoryIndex());
            }
            else
            {
                var scroller = _mainGridSpawner.GetFirstRowEnhancedScroller();
                if (scroller == null)
                {
                    return;
                }

                var index = scroller.NumberOfCells == 0 
                    ? 0 
                    : scroller.SnapCellViewIndex - scroller.StartCellViewIndex + scroller.StartDataIndex;
                var itemId = scroller.GetCellViewAtDataIndex(index)
                                            ?.GetComponent<AssetSelectionItemView>()
                                            ?.ContextData
                                            ?.ItemId ?? 0;
                
                _selectionScrollData.SetShrinkedViewScrollPosition(itemId, ContextData.AssetType, GetCurrentCategoryIndex());
            }
        }

        private void CancelAllActiveTasks()
        {
            foreach (var activeToken in _activeTokens)
            {
                activeToken.Cancel();
            }

            _activeTokens.Clear();
        }

        private void FadeInSubAssetViewPanel()
        {
            PlayFadeAnimation(true);
        }

        private void FadeOutSubAssetViewPanel()
        {
            PlayFadeAnimation(false);
        }

        private void PlayFadeAnimation(bool fadeIn)
        {
            if (IsDestroyed) return;
            
            var startAlpha = fadeIn ? 0f : 1f;
            var targetAlpha = fadeIn ? 1f : 0f;
            
            _fadePanelSequence?.Kill();
            _subAssetViewPanelCanvasGroup.alpha = startAlpha;
            _fadePanelSequence = DOTween.Sequence();
            _fadePanelSequence.Append(_subAssetViewPanelCanvasGroup.DOFade(targetAlpha, 0.3f).SetUpdate(true));
            _fadePanelSequence.SetUpdate(true);
            _subAssetViewPanelCanvasGroup.blocksRaycasts = fadeIn;
            _subGridSpawner.EnhancedScroller.enabled = fadeIn;
        }
        
        // We need to wait because deselection of input field happens before our custom events.
        // Data we would like to send with events cant be done properly otherwise (like sending selected item id). 
        private IEnumerator SendAmplitudeSearchEventWithDelay()
        {
            var searchText = _searchPanel.Text;
            
            yield return new WaitForSeconds(0.1f);
            SendAmplitudeSearchAssetEvent(searchText);
        }

        private void SendAmplitudeSearchAssetEvent(string searchInput)
        {
            var displayedItemsId = _mainGridSpawner.ContextData.Items.Select(x => x.ItemId).ToArray();
            var searchMetaData = new Dictionary<string, object>
            {
                [AmplitudeEventConstants.EventProperties.ASSET_SEARCH_ASSET_TYPE] = ContextData.AssetType,
                [AmplitudeEventConstants.EventProperties.ASSET_SEARCH_DISPLAYED_ASSETS_ID] = string.Join(",", displayedItemsId),
                [AmplitudeEventConstants.EventProperties.ASSET_SEARCH_SELECTED_ASSET] = _searchSelectedItemId,
                [AmplitudeEventConstants.EventProperties.ASSET_SEARCH_QUERY] = searchInput
            };
            _amplitudeManager.LogEventWithEventProperties(AmplitudeEventConstants.EventNames.ASSET_SEARCH, searchMetaData);
            _searchSelectedItemId = null;
        }

        private AssetSelectionItemModel GetFirstSelectedItemModel(DbModelType type)
        {
            switch (type)
            {
                case DbModelType.CameraFilterVariant:
                case DbModelType.CharacterSpawnPosition:
                    return _subAssetSelectorModel?.AssetSelectionHandler.SelectedModels.FirstOrDefault();
                default:
                    return ContextData.AssetSelectionHandler.SelectedModels.FirstOrDefault();
            }
        }

        private bool ShouldDisplayAssetInformation(DbModelType modelType)
        {
            return _displayAssetInfoTypes.Contains(modelType) && ContextData.AssetType == modelType || _subAssetSelectorModel?.AssetType == modelType;
        }

        private void OnCategoryTabsCreated()
        {
            assetSelectorAnimator.ChangeRevertButtonState(false);
        }

        private void RefreshCreateOutfitPanel()
        {
            if(_outfitPanel == null) return;
            _outfitPanel.SetActive(ContextData.ShouldShowCreateNewOutfitPanel());
        }

        private List<AssetSelectionItemModel> GetItemsToShow()
        {
            return _searchQueryCache.ContainsKey(ContextData.DisplayName)
                ? ContextData.GetItemsToShow(PaginationLoaderType.Search, filter:_searchQueryCache[ContextData.DisplayName])
                : ContextData.IsMyAssetsCategory
                    ? ContextData.GetItemsToShow(PaginationLoaderType.MyAssets)
                    : ContextData.IsRecommendedCategory
                        ? ContextData.GetItemsToShow(PaginationLoaderType.Recommended)
                        : ContextData.GetItemsToShow(PaginationLoaderType.Category, GetCurrentCategoryIndex());

        }

        private static int FindIndexByPredicate(IReadOnlyList<AssetSelectionItemModel> items,
                                         Predicate<AssetSelectionItemModel> predicate)
        {
            var index = 0;

            while (index < items.Count && (predicate?.Invoke(items[index]) ?? false))
            {
                index++;
            }

            if (index < items.Count)
            {
                return index;
            }

            return -1;
        }
        
        private void OnSelectionChangedByCode()
        {
            UpdateScrollsForEvents(false);
        }
    }
}