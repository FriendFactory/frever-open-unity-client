using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Bridge.ClientServer.Assets.Wardrobes;
using Bridge.Models.ClientServer.Assets;
using Bridge.Models.Common;
using Bridge.Results;
using EnhancedUI.EnhancedScroller;
using Extensions;
using JetBrains.Annotations;
using UIManaging.Pages.LevelEditor.Ui.Wardrobe;
using UnityEngine;
using Zenject;
using Resolution = Bridge.Models.Common.Files.Resolution;

namespace UIManaging.Pages.UmaEditorPage.Ui.WardrobePanel
{
    public class EquippedItemsHolder : BaseWardrobePanelUIHolder, IEnhancedScrollerDelegate
    {
        private const int PRELOAD_ROW_COUNT = 7;
        private const int TITLE_INDEX = 0;
        
        [SerializeField]
        private WardrobeRowItem _rowPrefab;
        [SerializeField]
        private int _countInRow = 4;
        [SerializeField]
        private EnhancedScroller _enhancedScroller;
        [SerializeField] 
        private UmaEditorPanel _umaEditorPanel;
        [SerializeField] 
        private UmaLevelEditor _umaLevelEditor;
        [SerializeField]
        private float _entitySize = 245;
        [SerializeField]
        private EnhancedScrollerCellView _titlePrefab;
        [SerializeField]
        private float _titleHeight = 60f;

        [Inject] private WardrobesResponsesCache _wardrobesResponsesCache;
        private long _genderId;

        public event Action<IEntity> WardrobeItemSelected;
        public long GenderId
        {
            get => _genderId;
            set
            {
                if (_genderId == value) return;
                Clear();
                _genderId = value;
            } 
        }
        
        private MyWardrobeListInfoProvider _myWardrobeListInfoProvider;
        private MyWardrobesListInfo _currentMyWardrobesListInfo;
        private List<IEntity> _entities = new ();
        
        private readonly List<IEntity> _selectedItems = new ();
        private readonly Dictionary<RequestType, List<WardrobeRequest>> _runningRequests = new ();
        private readonly CancellationTokenSource _destroyCancellationTokenSource = new ();

        private WardrobesInputHandler _wardrobesInputHandler;
        private IEntity _loadingItem;
        private Coroutine _resizeCoroutine;

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
            foreach (RequestType requestType in Enum.GetValues(typeof(RequestType)))
            {
                _runningRequests[requestType] = new List<WardrobeRequest>();
            }
        }

        private void OnDestroy()
        {
            _destroyCancellationTokenSource.CancelAndDispose();
        }
        
        private void OnDisable()
        {
            _wardrobesInputHandler?.Disable();
        }

        private void OnEnable()
        {
            _wardrobesInputHandler?.Enable();
        }
        
        public async void ShowItems()
        {
            CancelRunningRequests();
            
            _entities.Clear();
            
            AddNewItemsRange(_entities, _selectedItems);
            if (_enhancedScroller == null || !_enhancedScroller.isActiveAndEnabled) 
                return;
        
            _enhancedScroller.ReloadData();
        }
        
        public override void Clear()
        {
            _entities.Clear();
            _selectedItems.Clear();
            _loadingItem = null;
            if (!_enhancedScroller || !_enhancedScroller.Container)
            {
                return;
            }
            _enhancedScroller.ScrollPosition = 0;
            _enhancedScroller.ClearAll();
        }

        public void UpdateSelections(IEnumerable<IEntity> entities)
        {
            _loadingItem = null;
            _selectedItems.Clear();
            _selectedItems.AddRange(entities);
            _enhancedScroller?.RefreshActiveCellViews();
        }

        public void UpdateLoadingState(IEntity entity)
        {
            _loadingItem = entity;
            _enhancedScroller?.RefreshActiveCellViews();
        }
        public int GetNumberOfCells(EnhancedScroller scroller)
        {
            // The first item is the title
            return 1 + Mathf.CeilToInt(_entities.Count / (float)_countInRow);
        }

        public float GetCellViewSize(EnhancedScroller scroller, int dataIndex)
        {
            if (dataIndex == TITLE_INDEX)
            {
                return _titleHeight;
            }
            return _entitySize;
        }

        public EnhancedScrollerCellView GetCellView(EnhancedScroller scroller, int dataIndex, int cellIndex)
        {
            if (dataIndex == TITLE_INDEX)
            {
                var titleView = scroller.GetCellView(_titlePrefab);
                return titleView;
            }

            var rowView = scroller.GetCellView(_rowPrefab.GetComponent<WardrobeRowItem>());
            var rowItem = rowView as WardrobeRowItem;

            var resolution = Resolution._128x128;
    
            IEntity[] items = GetRowEntities(_countInRow, dataIndex - 1);
    
            var args = new WardrobeRowArgs()
            {
                Bridge = _bridge,
                Items = items,
                SelectedItems = _selectedItems,
                AdditionalButtonStyle = AdditionalButtonStyle.Nothing,
                ThumbnailResolution = resolution,
                OnItemSelected = OnItemSelected,
                ClothesCabinet = _clothesCabinet,
                WardrobesInputHandler = _wardrobesInputHandler,
            };
            rowItem.Setup(args);
            rowItem.LoadingStateCallback = SetItemLoadingState;         

            return rowView;
        }
        
        
        private IEntity[] GetRowEntities(int countInRow, int dataIndex)
        {
            IEntity[] items;
            var startIndex = dataIndex <= 0 ? 0 : dataIndex * _countInRow;

            var count = Mathf.Clamp(countInRow, 0, _entities.Count - startIndex);
            items = _entities.GetRange(startIndex, count).ToArray();
            return items;
        }

        private async void RequestMore(int dataIndex)
        {
            var adjustedDataIndex = dataIndex - 1;
            if (adjustedDataIndex < 0) return;

            var totalModelsCount = _entities.Count;
            var lastVisibleModelIndex = adjustedDataIndex * _countInRow;
            var remainedModelsCount = totalModelsCount - lastVisibleModelIndex;
            var minBufferSize = PRELOAD_ROW_COUNT * _countInRow;
            if (remainedModelsCount >= minBufferSize) return;
   
            var take = PRELOAD_ROW_COUNT * _countInRow;
            RequestNextWardrobes(take);
            if (_currentMyWardrobesListInfo != null && _currentMyWardrobesListInfo.WardrobesCount > 0)
            {
                var myDiff = _currentMyWardrobesListInfo.WardrobesCount;
                if (myDiff <= 0) return;

                var takeMy = Mathf.Clamp(myDiff, 1, PRELOAD_ROW_COUNT * _countInRow);
        
                var resp = await GetMyWardrobeList(_entities.FirstOrDefault()?.Id, takeMy);
                if (resp.IsSuccess && AddNewItemsRange(_entities, resp.Models))
                {
                    Resize();
                }
            }
        }

        private Task<ArrayResult<WardrobeShortInfo>> GetMyWardrobeList(long? startId, int takeNext)
        {
            var filter = new MyWardrobeFilterModel
            {
                Target = startId,
                TakeNext = takeNext,
                TakePrevious = 0,
                WardrobeCategoryId = null,
                WardrobeSubCategoryId = null,
                GenderId = GenderId
            };

            return GetWardrobes(RequestType.General, filter, () => new MyWardrobeListRequest(_bridge, filter));
        }

        private async void RequestNextWardrobes(int take)
        {
            var resp = await GetWardrobeList(_entities.LastOrDefault()?.Id, take);
            if (resp.IsError || resp.IsRequestCanceled) return;
            var items = resp.Models;
            if (AddNewItemsRange(_entities, items))
            {
                Resize();
            }
        }

        private void Resize()
        {
            if (!gameObject.activeSelf) return;
            if (_resizeCoroutine is not null)
            {
                StopCoroutine(_resizeCoroutine);
                _resizeCoroutine = null;
            }

            _resizeCoroutine = StartCoroutine(ResizeScroller());
        }

        private IEnumerator ResizeScroller()
        {
            yield return new WaitForEndOfFrame();
            _enhancedScroller._Resize(true);
        }

        private bool AddNewItemsRange(ICollection<IEntity> baseCollection, IEnumerable<IEntity> entities)
        {
            if (!entities.Any(e => e is WardrobeFullInfo || e is WardrobeShortInfo))
            {
                return false;
            }
            
            var enumerable = entities
                .Select(e => e switch
                {
                    WardrobeFullInfo fullInfo => fullInfo.ToShortInfo(),
                    WardrobeShortInfo shortInfo => shortInfo,
                    _ => null
                })
                .Where(e => e != null);
            
            var added = false;
            foreach (var item in enumerable)
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

        private Task<ArrayResult<WardrobeShortInfo>> GetWardrobeList(long? startId, int take)
        {
            var filter = new WardrobeFilter
            {
                Target = startId,
                TakeNext = take,
                TakePrevious = 0,
                WardrobeCategoryId = null,
                WardrobeSubCategoryId = null,
                GenderId = GenderId,
                TagIds = null
            };

            return GetWardrobes(RequestType.General, filter, () => new GeneralWardrobeListRequest(_bridge, filter));
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

        private void OnItemSelected(IEntity entity)
        {
            WardrobeItemSelected?.Invoke(entity);
        }

        private void SetItemLoadingState(WardrobeUIItem item)
        {
            item.IsLoading = item != null && item.Entity != null && item.Entity == _loadingItem;
        }

        private void CancelRunningRequests()
        {
            _runningRequests.Values
                .SelectMany(requests => requests)
                .ToList()
                .ForEach(request => request.Cancel());
        }
    }
}