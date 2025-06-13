using Bridge.Models.ClientServer.Assets;
using Bridge.Models.Common;
using EnhancedUI.EnhancedScroller;
using Filtering;
using Modules.CharacterManagement;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Bridge.ClientServer.Assets.Wardrobes;
using Bridge.Results;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Zenject;
using Resolution = Bridge.Models.Common.Files.Resolution;
using Extensions;
using JetBrains.Annotations;
using UIManaging.Pages.LevelEditor.Ui.Wardrobe;

namespace UIManaging.Pages.UmaEditorPage.Ui.WardrobePanel
{
    internal sealed class WardrobeItemsHolder : BaseWardrobePanelUIHolder, IEnhancedScrollerDelegate
    {
        public enum CategoryType
        {
            Regular,
            Outfit,
            ThemeCollection
        }
        
        private const int PRELOAD_ROW_COUNT = 7;
        private const float AUTOSCROLL_DURATION = 0.5f;
        private const float SCROLLER_THRESHOLD = 10;

        [SerializeField]
        private ScrollRect _scrollRect;
        [SerializeField]
        private EnhancedScroller _enhancedScroller;
        [SerializeField]
        private int _countInRow = 4;
        [SerializeField]
        private WardrobeRowItem _rowPrefab;
        [SerializeField]
        private GameObject _outfitHintText;
        [SerializeField]
        private TextMeshProUGUI _noAssetsAvailableText;
        [SerializeField] 
        private UmaEditorPanel _umaEditorPanel;
        [SerializeField] 
        private UmaLevelEditor _umaLevelEditor;
        [SerializeField]
        private WardrobeRowItem _outfitRowPrefab;
        [SerializeField]
        private Toggle _myAssetsToggle;
        [SerializeField]
        private Toggle _allAssetsToggle;
        [SerializeField]
        private GameObject _assetsTogglePanel;
        [SerializeField]
        private float _sizeWithoutPrices = 245;
        [SerializeField]
        private float _sizeWithPrices = 288;

        [Inject] private OutfitsManager _outfitsManager;
        [Inject] private WardrobesResponsesCache _wardrobesResponsesCache;
        private long _genderId;

        public event Action<IEntity> WardrobeItemSelected;
        public event Action<IEntity> WardrobePurchaseRequested;
        public long? CurrentSubCategoryId { get; set; }
        public long? CurrentThemeCollectionId { get; set; }
        public long GenderId
        {
            get => _genderId;
            set
            {
                if (_genderId == value) return;
                ClearItems();
                _genderId = value;
            } }
        public long? TaskId { get; set; }
        public FilteringSetting _filteringSetting { get; set; } = new() { Sorting = AssetSorting.NewestFirst };

        private Action _additionalButtonAction;

        private readonly List<IEntity> _mySubCategoryEntities = new List<IEntity>();
        private readonly List<IEntity> _currentSubCategoryEntities = new List<IEntity>();
        private readonly List<IEntity> _selectedItems = new List<IEntity>();

        private bool _additionalButtonAvailable;
        private IEntity _loadingItem;
        private AdditionalButtonStyle _currentStyle;
        private WardrobesInputHandler _wardrobesInputHandler;

        private readonly Dictionary<RequestType, List<WardrobeRequest>> _runningRequests =
            new Dictionary<RequestType, List<WardrobeRequest>>();

        private readonly CancellationTokenSource _destroyCancellationTokenSource = new CancellationTokenSource();

        private CategoryType _categoryType;
        private MyWardrobeListInfoProvider _myWardrobeListInfoProvider;
        private MyWardrobesListInfo _currentMyWardrobesListInfo;

        [Inject]
        [UsedImplicitly]
        public void Construct()
        {
            _myWardrobeListInfoProvider = new MyWardrobeListInfoProvider(_bridge);
            _wardrobesInputHandler = new WardrobesInputHandler(_umaEditorPanel ?? (IWardrobeChangesPublisher)_umaLevelEditor);
        }
        
        private void Awake() 
        {
            _enhancedScroller.Delegate = this;
            _enhancedScroller.scrollerScrolled += OnScrollerScrolled;
            foreach (RequestType requestType in Enum.GetValues(typeof(RequestType)))
            {
                _runningRequests[requestType] = new List<WardrobeRequest>();
            }
        }

        private void OnEnable()
        {
            _wardrobesInputHandler?.Enable();
        }
        private void OnDisable()
        {
            _myWardrobeListInfoProvider.Clear();
			_wardrobesInputHandler.Disable();
        }

        private void OnDestroy()
        {
            _destroyCancellationTokenSource.CancelAndDispose();
        }

        public async void ShowItems(CategoryType categoryType, bool showAdditionalButton = false, Action additionalButtonAction = null)
        {
            CancelRunningRequests();

            ClearItems();
            
            _categoryType = categoryType;
            _allAssetsToggle.isOn = true;
            _additionalButtonAvailable = showAdditionalButton;
            _additionalButtonAction = additionalButtonAction;
            _currentStyle = categoryType == CategoryType.Outfit ? AdditionalButtonStyle.CreateOutfit : AdditionalButtonStyle.Clear;
            _currentMyWardrobesListInfo = null;
            
            var take = PRELOAD_ROW_COUNT * _countInRow;
            IEnumerable<IEntity> items;
            
            var startDataIndex = 0;

            switch (_categoryType)
            {
                case CategoryType.Outfit:
                    var skip = _currentSubCategoryEntities.Count;
                    var saveMethod = _additionalButtonAvailable ? SaveOutfitMethod.Manual : SaveOutfitMethod.Automatic;
                    items = await GetOutfits(take, skip, saveMethod, GenderId);
                    AddNewItemsRange(_currentSubCategoryEntities, items);
                    ChangeAssetsToggleVisibility(false);
                    break;
                case CategoryType.ThemeCollection:
                    
                    if (!CurrentThemeCollectionId.HasValue)
                    {
                        Debug.LogError("Invalid theme collection id");
                        return;
                    }
                    
                    var tokenTheme = _destroyCancellationTokenSource.Token;
                    _currentMyWardrobesListInfo = await _myWardrobeListInfoProvider.GetMyWardrobeListInfo(null, null, CurrentThemeCollectionId, GenderId, tokenTheme);
                    if (tokenTheme.IsCancellationRequested) return;

                    if (_currentMyWardrobesListInfo != null)
                    {
                        ChangeAssetsToggleVisibility(_currentMyWardrobesListInfo.WardrobesCount > 0);
                        var resp = await GetMyThemeCollectionsWardrobeList(null, _currentMyWardrobesListInfo.WardrobesCount, 0, CurrentThemeCollectionId ?? 0);
                        if (resp.IsRequestCanceled) return;
                        items = resp.IsSuccess ? resp.Models : Array.Empty<WardrobeShortInfo>();
                        _mySubCategoryEntities.AddRange(items);
                        startDataIndex = _currentMyWardrobesListInfo.WardrobesCount == 0 ? 
                            0 : Mathf.CeilToInt((_currentMyWardrobesListInfo.WardrobesCount + (_additionalButtonAvailable ? 1 : 0)) / (float)_countInRow);
                    }
                    else
                    {
                        ChangeAssetsToggleVisibility(false);
                    }
                    
                    var collection = await GetThemeCollectionsWardrobeList(null, take, CurrentThemeCollectionId ?? 0);
                    if (collection.IsError || collection.IsRequestCanceled) return;
                    items = collection.Models;
                    AddNewItemsRange(_currentSubCategoryEntities, items);
                    break;
                case CategoryType.Regular:
                    var categoryId = (CurrentSubCategoryId < 0 && CurrentSubCategoryId != null) ? -CurrentSubCategoryId : null;
                    var subCategoryId = categoryId != null ? null : CurrentSubCategoryId;
                    
                    var token = _destroyCancellationTokenSource.Token;
                    _currentMyWardrobesListInfo = await _myWardrobeListInfoProvider.GetMyWardrobeListInfo(categoryId, subCategoryId, null, GenderId, token);
                    if (token.IsCancellationRequested) return;

                    if (_currentMyWardrobesListInfo != null)
                    {
                        ChangeAssetsToggleVisibility(_currentMyWardrobesListInfo.WardrobesCount > 0);
                        var resp = await GetMyWardrobeList(null, _currentMyWardrobesListInfo.WardrobesCount, 0, categoryId, subCategoryId);
                        if (resp.IsRequestCanceled) return;
                        items = resp.IsSuccess ? resp.Models : Array.Empty<WardrobeShortInfo>();
                        _mySubCategoryEntities.AddRange(items);
                        startDataIndex = _currentMyWardrobesListInfo.WardrobesCount == 0? 
                            0: Mathf.CeilToInt((_currentMyWardrobesListInfo.WardrobesCount + (_additionalButtonAvailable ? 1 : 0)) / (float)_countInRow);
                    }
                    else
                    {
                        ChangeAssetsToggleVisibility(false);
                    }
                    var wardrobes = await GetWardrobeList(null, take, categoryId, subCategoryId);
                    if (wardrobes.IsError || wardrobes.IsRequestCanceled) return;
                    items = wardrobes.Models;
                    AddNewItemsRange(_currentSubCategoryEntities, items);
                    break;
                default:
                    return;
            }

            if (_enhancedScroller == null || !_enhancedScroller.isActiveAndEnabled) return; //possibly already destroyed      
            _enhancedScroller.ReloadData();
            
            // with introduction of the new DressUp flow we want to start the scroll position from the purchased items [FREV-20321]
            // I will keep old logic for now if something will change in the future
            // _enhancedScroller.JumpToDataIndex(startDataIndex, forceCalculateRange:true);

            var hasItems = items.Any() || _currentMyWardrobesListInfo?.WardrobesCount > 0;
            _outfitHintText.SetActive(!hasItems && _categoryType == CategoryType.Outfit);
            _noAssetsAvailableText.SetActive(!hasItems && _categoryType != CategoryType.Outfit);
        }

        public void UpdateSelections(IEntity[] entities)
        {
            _loadingItem = null;
            _selectedItems.Clear();
            _selectedItems.AddRange(entities);
            _enhancedScroller.RefreshActiveCellViews();
        }

        public void UpdateLoadingItem(IEntity entity)
        {
            _loadingItem = entity;
            _enhancedScroller.RefreshActiveCellViews();
        }

        public void UpdateAfterPurchase(IEntity entity)
        {
            _loadingItem = null;
           
            _clothesCabinet.SetItemAsPurchased(entity.Id);
            
            var subCategories = (entity as ISubCategorizable).SubCategories;
            if (subCategories.Any(x => x == CurrentSubCategoryId))
            {
                _currentMyWardrobesListInfo.WardrobesCount++;
                _mySubCategoryEntities.Add(entity);
            }
           
            _myWardrobeListInfoProvider.Clear();
            _enhancedScroller._RecycleAllCells();
            _enhancedScroller._Resize(true);
            _enhancedScroller._RefreshActive();
            ChangeAssetsToggleVisibility(true);
        }

        public override void Clear()
        {
            ClearItems();
            WardrobeItemSelected = null;
        }

        public IEnumerable<IEntity> GetSelectedItems()
        {
            return _selectedItems; 
        }

        public void ScrollToMy()
        {
            _enhancedScroller.JumpToDataIndex(dataIndex: 0, tweenType: EnhancedScroller.TweenType.linear, tweenTime: AUTOSCROLL_DURATION);
        }

        public void ScrollToAll()
        {
            int myAssetLastDataIndex = 0;
            if (_mySubCategoryEntities.Count > 0)
            {
                myAssetLastDataIndex = Mathf.CeilToInt((_mySubCategoryEntities.Count + (_additionalButtonAvailable ? 1 : 0)) / (float)_countInRow);
            }
            _enhancedScroller.JumpToDataIndex(dataIndex: myAssetLastDataIndex, tweenType: EnhancedScroller.TweenType.linear, tweenTime: AUTOSCROLL_DURATION);
        }

        public void ChangeAssetsToggleVisibility(bool visible)
        {
            _assetsTogglePanel.SetActive(visible);
        }

        #region IEnhancedScrollerDelegate
        public int GetNumberOfCells(EnhancedScroller scroller)
        {
            var myItemsCount = _mySubCategoryEntities.Count;
            var currentItemsCount = _currentSubCategoryEntities.Count;

            if (_additionalButtonAvailable)
            {
                if (myItemsCount > 0)
                {
                    myItemsCount++;
                }
                else
                {
                    currentItemsCount++;
                }
            }
            
            return Mathf.CeilToInt(myItemsCount / (float) _countInRow) + Mathf.CeilToInt(currentItemsCount / (float) _countInRow);
        }

        public float GetCellViewSize(EnhancedScroller scroller, int dataIndex)
        {
            var items = GetRowEntities(_countInRow, dataIndex);
            return (_categoryType == CategoryType.Outfit || HasPriceOrTier(items)) ? _sizeWithPrices : _sizeWithoutPrices;
        }

        public EnhancedScrollerCellView GetCellView(EnhancedScroller scroller, int dataIndex, int cellIndex)
        {
            var rowView = scroller.GetCellView(_categoryType == CategoryType.Outfit ? _outfitRowPrefab : _rowPrefab);
            var rowItem = rowView as WardrobeRowItem;

            var showAdjustmentButton = _additionalButtonAvailable && dataIndex == 0;
            var rowButtonStyle = showAdjustmentButton ? _currentStyle : AdditionalButtonStyle.Nothing;
            var resolution = _categoryType == CategoryType.Outfit ? Resolution._256x256 : Resolution._128x128;

            IEntity[] items = GetRowEntities(_countInRow - (showAdjustmentButton ? 1 : 0), dataIndex);
            switch (_categoryType)
            {
                case CategoryType.Outfit:
                    RequestMoreOutfits(dataIndex);
                    break;
                case CategoryType.Regular:
                    RequestMore(_enhancedScroller.EndCellViewIndex);
                    break;
                case CategoryType.ThemeCollection:
                    RequestMoreThemeCollections(_enhancedScroller.EndCellViewIndex);
                    break;
            }

            var args = new WardrobeRowArgs()
            {
                Bridge = _bridge,
                Items = items,
                SelectedItems = _selectedItems,
                AdditionalButtonStyle = rowButtonStyle,
                OnAdditionalPressed = OnAdjustmentsButtonClicked,
                ThumbnailResolution = resolution,
                OnItemSelected = OnItemSelected,
                OnPurchaseRequested = OnPurchaseRequested,
                ClothesCabinet = _clothesCabinet,
                WardrobesInputHandler = _wardrobesInputHandler,
            };
            rowItem.Setup(args);
            rowItem.LoadingStateCallback = SetItemLoadingState;         

            return rowView;
        }

        public void OnScrollerScrolled(EnhancedScroller scroller, Vector2 val, float scrollPosition)
        {
            CheckMyOnScreen(scrollPosition);
        }
        #endregion
        
        private void CheckMyOnScreen(float scrollPosition)
        {
            if (_currentMyWardrobesListInfo == null) return;

            var myLastIndex =  Mathf.CeilToInt((_currentMyWardrobesListInfo.WardrobesCount + (_additionalButtonAvailable ? 1 : 0)) / (float)_countInRow) - 1;
            var myLastIndexPosition = _enhancedScroller.GetScrollPositionForDataIndex(myLastIndex, EnhancedScroller.CellViewPositionEnum.Before);

            if (myLastIndexPosition >= (scrollPosition - SCROLLER_THRESHOLD))
            {
                _myAssetsToggle.isOn = true;
            }
            else
            { 
                _allAssetsToggle.isOn = true;
            }
        }

        private IEntity[] GetRowEntities(int countInRow, int dataIndex)
        {
            IEntity[] items;
            var startIndex = dataIndex <= 0 ? 0 : dataIndex * _countInRow - (_additionalButtonAvailable ? 1 : 0);

            if (_currentMyWardrobesListInfo != null && _currentMyWardrobesListInfo.WardrobesCount > 0 && _mySubCategoryEntities.Count > startIndex)
            {
                var count = Mathf.Clamp(countInRow, 0, _mySubCategoryEntities.Count - startIndex);
                items = _mySubCategoryEntities.GetRange(startIndex, count).ToArray();
            }
            else
            {
                if (_mySubCategoryEntities.Count > 0)
                {
                    var myAssetLastDataIndex = dataIndex - Mathf.CeilToInt((_mySubCategoryEntities.Count + (_additionalButtonAvailable ? 1 : 0)) / (float)_countInRow);
                    startIndex = myAssetLastDataIndex * _countInRow;
                }
                
                var count = Mathf.Clamp(countInRow, 0, _currentSubCategoryEntities.Count - startIndex);
                items = _currentSubCategoryEntities.GetRange(startIndex, count).ToArray();
            }
            return items;
        }

        private void RequestMore(int dataIndex)
        {
            var totalModelsCount = _currentSubCategoryEntities.Count + _mySubCategoryEntities.Count;
            var lastVisibleModelIndex = dataIndex * _countInRow;
            var remainedModelsCount = totalModelsCount - lastVisibleModelIndex;
            var minBufferSize = PRELOAD_ROW_COUNT * _countInRow;
            if (remainedModelsCount >= minBufferSize) return;
           
            var take = PRELOAD_ROW_COUNT * _countInRow;
            RequestNextWardrobes(take);
            if (_currentMyWardrobesListInfo != null && _currentMyWardrobesListInfo.WardrobesCount > 0)
            {
                var myDiff = _currentMyWardrobesListInfo.WardrobesCount - _mySubCategoryEntities.Count;
                if (myDiff <= 0) return;

                var takeMy = Mathf.Clamp(myDiff, 1, PRELOAD_ROW_COUNT * _countInRow);
                RequestMyWardrobes(0, takeMy);
            }
        }
        
        private void RequestMoreThemeCollections(int dataIndex)
        {
            var totalModelsCount = _currentSubCategoryEntities.Count + _mySubCategoryEntities.Count;
            var lastVisibleModelIndex = dataIndex * _countInRow;
            var remainedModelsCount = totalModelsCount - lastVisibleModelIndex;
            var minBufferSize = PRELOAD_ROW_COUNT * _countInRow;
            if (remainedModelsCount >= minBufferSize) return;
           
            var take = PRELOAD_ROW_COUNT * _countInRow;
            RequestNextThemeCollectionsWardrobes(take);
            if (_currentMyWardrobesListInfo != null && _currentMyWardrobesListInfo.WardrobesCount > 0)
            {
                var myDiff = _currentMyWardrobesListInfo.WardrobesCount - _mySubCategoryEntities.Count;
                if (myDiff <= 0) return;

                var takeMy = Mathf.Clamp(myDiff, 1, PRELOAD_ROW_COUNT * _countInRow);
                RequestMyWardrobes(0, takeMy);
            }
        }

        private void RequestMoreOutfits(int dataIndex)
        {
            var totalCount = _currentSubCategoryEntities.Count + _mySubCategoryEntities.Count;
            var countDif = totalCount - dataIndex * _countInRow;

            if (countDif < PRELOAD_ROW_COUNT * _countInRow)
            {
                var take = PRELOAD_ROW_COUNT * _countInRow - countDif;
                var skip = _currentSubCategoryEntities.Count;
                var saveMethod = _additionalButtonAction != null ? SaveOutfitMethod.Manual : SaveOutfitMethod.Automatic;
                RequestNextOutfits(take, skip, saveMethod);
            }
        }

        private void ClearItems()
        {
            _mySubCategoryEntities.Clear();
            _currentSubCategoryEntities.Clear();
            
            if(!_enhancedScroller.Container) return;
            _enhancedScroller.ScrollPosition = 0;
            _enhancedScroller.ClearAll();
        }

        private bool HasPriceOrTier(IEnumerable<IEntity> items)
        {
            return items.Any(x => 
            {
                if (x is WardrobeShortInfo shortInfo)
                {
                    return shortInfo.AssetOffer != null || shortInfo.AssetTier != null;
                }
                return true;
            });
        }

        private void OnItemSelected(IEntity entity)
        {
            WardrobeItemSelected?.Invoke(entity);
        }
        
        private void OnPurchaseRequested(IEntity entity)
        {
            WardrobePurchaseRequested?.Invoke(entity);
        }

        private void OnAdjustmentsButtonClicked()
        {
            _additionalButtonAction?.Invoke();
        }

        private void SetItemLoadingState(WardrobeUIItem item)
        {
            item.IsLoading = item != null && item.Entity != null && item.Entity == _loadingItem;
        }

        private async void RequestNextWardrobes(int take)
        {
            var categoryId = CurrentSubCategoryId < 0 ? -CurrentSubCategoryId : null;
            var subCategoryId = CurrentSubCategoryId < 0 ? null : CurrentSubCategoryId;

            var resp = await GetWardrobeList(_currentSubCategoryEntities.LastOrDefault()?.Id, take, categoryId, subCategoryId);
            if (resp.IsError || resp.IsRequestCanceled) return;
            var items = resp.Models;
            if (AddNewItemsRange(_currentSubCategoryEntities, items))
            {
                Resize();
            }
        }
        
        private async void RequestNextThemeCollectionsWardrobes(int take)
        {
            if (!CurrentThemeCollectionId.HasValue)
            {
                Debug.LogError("Invalid theme collections id");
                return;
            }
            
            var resp = await GetThemeCollectionsWardrobeList(_currentSubCategoryEntities.LastOrDefault()?.Id, take, CurrentThemeCollectionId.Value);
            if (resp.IsError || resp.IsRequestCanceled) return;
            var items = resp.Models;
            if (AddNewItemsRange(_currentSubCategoryEntities, items))
            {
                Resize();
            }
        }

        private void Resize()
        {
            if (!gameObject.activeSelf) return;
            StopCoroutine(ResizeScroller());
            StartCoroutine(ResizeScroller());
        }
        
        private IEnumerator ResizeScroller()
        {
            yield return new WaitForEndOfFrame();
            _enhancedScroller._Resize(true);
        }

        private async void RequestMyWardrobes(int takeNext = 0, int takePrevious = 0)
        {
            var categoryId = CurrentSubCategoryId < 0 ? -CurrentSubCategoryId : null;
            var subCategoryId = CurrentSubCategoryId < 0 ? null : CurrentSubCategoryId;
            var resp = await GetMyWardrobeList(_mySubCategoryEntities.FirstOrDefault()?.Id, takeNext, takePrevious, categoryId, subCategoryId);
            if (resp.IsSuccess && AddNewItemsRange(_mySubCategoryEntities, resp.Models))
            {
                Resize();
            }
        }

        private async void RequestMyThemeCollectionsWardrobes(int takeNext = 0, int takePrevious = 0)
        {
            var categoryId = CurrentSubCategoryId < 0 ? -CurrentSubCategoryId : null;
            var subCategoryId = CurrentSubCategoryId < 0 ? null : CurrentSubCategoryId;
            var resp = await GetMyThemeCollectionsWardrobeList(_mySubCategoryEntities.FirstOrDefault()?.Id, takeNext, takePrevious, CurrentThemeCollectionId ?? 0);
            if (resp.IsSuccess && AddNewItemsRange(_mySubCategoryEntities, resp.Models))
            {
                Resize();
            }
        }
        
        private async void RequestNextOutfits(int take, int skip, SaveOutfitMethod saveOutfitMethod)
        {
            var token = _destroyCancellationTokenSource.Token;
            var items = await GetOutfits(take, skip, saveOutfitMethod, GenderId, token);
            if (token.IsCancellationRequested) return;

            foreach (var item in items)
            {
                if (_currentSubCategoryEntities.FindIndex(x => x.Id == item.Id) >= 0)
                {
                    continue;
                }
                _currentSubCategoryEntities.Add(item);
            }
            _enhancedScroller._Resize(true);
        }

        private Task<ArrayResult<WardrobeShortInfo>> GetWardrobeList(long? startId, int take, long? categoryId = null, long? subCategoryId = null)
        {
            var filter = new WardrobeFilter
            {
                Target = startId,
                TakeNext = take,
                TakePrevious = 0,
                WardrobeCategoryId = categoryId,
                WardrobeSubCategoryId = subCategoryId,
                GenderId = GenderId,
                PriceFilter = _filteringSetting.AssetPriceFilter,
                Sorting = _filteringSetting.Sorting,
                TaskId = TaskId,
                TagIds = null
            };

            return GetWardrobes(RequestType.General, filter, () => new GeneralWardrobeListRequest(_bridge, filter));
        }

        private Task<ArrayResult<WardrobeShortInfo>> GetMyWardrobeList(long? startId, int takeNext, int takePrevious, long? categoryId, long? subCategoryId)
        {
            var filter = new MyWardrobeFilterModel
            {
                Target = startId,
                TakeNext = takeNext,
                TakePrevious = takePrevious,
                WardrobeCategoryId = categoryId,
                WardrobeSubCategoryId = subCategoryId,
                GenderId = GenderId
            };

            return GetWardrobes(RequestType.My, filter, () => new MyWardrobeListRequest(_bridge, filter));
        }

        private Task<ArrayResult<WardrobeShortInfo>> GetThemeCollectionsWardrobeList(long? startId, int take, long themeCollectionId)
        {
            var filter = new WardrobeFilter
            {
                Target = startId,
                TakeNext = take,
                TakePrevious = 0,
                GenderId = GenderId,
                PriceFilter = _filteringSetting.AssetPriceFilter,
                Sorting = _filteringSetting.Sorting,
                TaskId = TaskId,
                ThemeCollectionId = themeCollectionId,
                TagIds = null
            };

            return GetWardrobes(RequestType.General, filter, () => new GeneralWardrobeListRequest(_bridge, filter));
        }
        
        private Task<ArrayResult<WardrobeShortInfo>> GetMyThemeCollectionsWardrobeList(long? startId, int takeNext, int takePrevious, long themeCollectionId)
        {
            var filter = new MyWardrobeFilterModel
            {
                Target = startId,
                TakeNext = takeNext,
                TakePrevious = takePrevious,
                GenderId = GenderId,
                ThemeCollectionId = themeCollectionId
            };

            return GetWardrobes(RequestType.My, filter, () => new MyWardrobeListRequest(_bridge, filter));
        }

        private async Task<ArrayResult<WardrobeShortInfo>> GetWardrobes(RequestType requestType, IWardrobeFilter filter, Func<WardrobeRequest> requestCreationDelegate)
        {
            if (_wardrobesResponsesCache.TryGetFromCache(requestType, filter, out var response))
            {
                return response;
            }

            var alreadyRunning = _runningRequests[requestType].FirstOrDefault(x => !x.IsCancelled && x.Filter.Equals(filter));
            if (alreadyRunning != null)
            {
                while (!alreadyRunning.IsFinished)
                {
                    await Task.Delay(30);
                }

                return alreadyRunning.Result;
            }

            var req = requestCreationDelegate();
            _runningRequests[requestType].Add(req);
            await req.RunAsync();
            _runningRequests[requestType].Remove(req);
            if (req.Result.IsSuccess)
            {
                _wardrobesResponsesCache.AddResponse(requestType, filter, req.Result);
            }
            return req.Result;
        }

        private async Task<IEnumerable<IEntity>> GetOutfits(int take, int skip, SaveOutfitMethod saveOutfitMethod, long genderId, CancellationToken token = default)
        {
            return await _outfitsManager.GetOutfitShortInfoList(take, skip, saveOutfitMethod, genderId, token);
        }
    
        private bool AddNewItemsRange(ICollection<IEntity> baseCollection, IEnumerable<IEntity> entities)
        {
            var added = false;
            foreach (var item in entities)
            {
                if (baseCollection.Any(x => x.Id == item.Id))
                {
                    continue;
                }
                baseCollection.Add(item);
                added = true;
            }
            return added;
        }
        
        private void CancelRunningRequests()
        {
            foreach (var keyValuePair in _runningRequests)
            {
                foreach (var runningRequest in keyValuePair.Value)
                {
                    runningRequest.Cancel();
                }
                keyValuePair.Value.Clear();
            }
        }
    }
}