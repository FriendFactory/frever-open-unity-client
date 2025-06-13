using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Common;
using Common.BridgeAdapter;
using EnhancedUI.EnhancedScroller;
using Extensions;
using JetBrains.Annotations;
using Models;
using Modules.UserScenarios.Common;
using Navigation.Args;
using Navigation.Core;
using UIManaging.Common.Args.Views.LevelPreviews;
using UIManaging.Common.InputFields;
using UIManaging.Common.SelectableGrid;
using UIManaging.PopupSystem;
using UIManaging.PopupSystem.Configurations;
using UIManaging.SnackBarSystem;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace UIManaging.Pages.DraftsPage
{
    public sealed class DraftsPage : GenericPage<DraftsPageArgs>, IEnhancedScrollerDelegate
    {
        private const float LOADING_NEXT_PAGE_POSITION_THRESHOLD = 0.1f;
        private const int TAKE_NEXT_COUNT = 21;
        
        [SerializeField] private int countInRow = 3;
        [SerializeField] private DeletableGridManager _deletableGridManager;
        [SerializeField] private EnhancedScroller _scroller;
        [SerializeField] private EnhancedScrollerCellView _levelPreviewsRowPrefab;
        [SerializeField] private GameObject _actionPanel;
        [SerializeField] private Button _editButton;
        [SerializeField] private Button _deleteButton;
        [SerializeField] private float _thumbnailAspectRatio = 0.62f;

        private IScenarioManager _scenarioManager;
        private ILevelService _levelService;
        private PopupManager _popupManager;
        private SnackBarHelper _snackBarHelper;
        
        private List<BaseLevelItemArgs> _levelPreviewItemArgs;
        
        private int _draftsLeftToDelete;
        private float _cellSize;
        private string _draftDeletedMessage;
        private bool _isDownloading;
        private BaseLevelItemArgs _selectedItem;
        private DraftsPageLoc _loc;

        //---------------------------------------------------------------------
        // Properties
        //---------------------------------------------------------------------

        public override PageId Id => PageId.DraftsPage;

        //---------------------------------------------------------------------
        // Ctors
        //---------------------------------------------------------------------
        
        [Inject]
        [UsedImplicitly]
        public void Construct(ILevelService levelService, PopupManager popupManager, SnackBarHelper snackBarHelper, IScenarioManager scenarioManager)
        {
            _levelService = levelService;
            _popupManager = popupManager;
            _snackBarHelper = snackBarHelper;
            _scenarioManager = scenarioManager;
        }

        //---------------------------------------------------------------------
        // Messages
        //---------------------------------------------------------------------
        
        private void Start()
        {
            _cellSize = _scroller.GetComponent<RectTransform>().rect.width / countInRow / _thumbnailAspectRatio;
            _scroller.Delegate = this;
        }

        private void OnEnable()
        {
            _deletableGridManager.SelectionModeOn += OnSelectionModeOn;
            _editButton.onClick.AddListener(EditSelectedDraft);
            _deleteButton.onClick.AddListener(ShowDeletePopup);
        }
        
        private void OnDisable() 
        {
            _deletableGridManager.SelectionModeOn -= OnSelectionModeOn;
            _editButton.onClick.RemoveListener(EditSelectedDraft);
            _deleteButton?.onClick.RemoveListener(ShowDeletePopup);
        }

        //---------------------------------------------------------------------
        // Public
        //---------------------------------------------------------------------
        #region IEnhancedScrollerDelegate

        public int GetNumberOfCells(EnhancedScroller scroller)
        {
            return Mathf.CeilToInt(_levelPreviewItemArgs.Count / (float)countInRow);
        }

        public float GetCellViewSize(EnhancedScroller scroller, int dataIndex)
        {
            return _cellSize;
        }

        public EnhancedScrollerCellView GetCellView(EnhancedScroller scroller, int dataIndex, int cellIndex)
        {
            var cellsView = scroller.GetCellView(_levelPreviewsRowPrefab);
            var rowView = cellsView.GetComponent<DraftsRow>();
            rowView.SelectedItem = _selectedItem;
#if UNITY_EDITOR
            cellsView.name = $"[Row] {dataIndex}";
#endif
            var selectedLevels = _levelPreviewItemArgs.Skip(dataIndex * countInRow).Take(countInRow).ToArray();
            rowView.Setup(selectedLevels);
            _deletableGridManager.OnCellViewInstantiated(cellsView);
            return cellsView;
        }
        #endregion

        //---------------------------------------------------------------------
        // Protected
        //---------------------------------------------------------------------

        protected override void OnInit(PageManager manager)
        {
			Manager = manager;
            _loc = GetComponent<DraftsPageLoc>();
        }

        protected override void OnDisplayStart(DraftsPageArgs pageArgs)
        {
            base.OnDisplayStart(pageArgs);
            SetupLevelsData(pageArgs.LevelsArgs);
            var gridArgs = new DeletableGridManagerArgs(_loc.PageHeader, DeleteSelectedDrafts, OnBackButtonClick);
            _deletableGridManager.Init(gridArgs);
            _scroller.scrollerScrolled += ScrollerScrolled;
        }

        protected override void OnHidingBegin(Action onComplete)
        {
            _scroller.scrollerScrolled -= ScrollerScrolled;
            _actionPanel.SetActive(false);
            base.OnHidingBegin(onComplete);
        }
        
        //---------------------------------------------------------------------
        // Helpers
        //---------------------------------------------------------------------

        private void DeleteSelectedDrafts(long[] draftsIds)
        {
            ShowLoadingPopup(_loc.DeletingDraftsMessage);

            var levels = _levelPreviewItemArgs.Select(arg => arg.Level).ToArray();
            var draftsToDelete = levels.Where(character => draftsIds.Contains(character.Id)).ToArray();
            _draftsLeftToDelete = draftsToDelete.Length;
            _draftDeletedMessage = _draftsLeftToDelete > 1 ? _loc.DraftsDeletedMessage : _loc.DraftDeletedMessage;

            for (var i = 0; i < draftsToDelete.Length; i++)
            {
                SendDeleteLevelRequest(draftsToDelete[i].Id, OnLevelDeleted);
            }
        }

        private void ShowLoadingPopup(string popupText)
        {
            var loadingPopupConfig = new InformationPopupConfiguration()
            {
                PopupType = PopupType.Loading, Title = popupText
            };
            _popupManager.SetupPopup(loadingPopupConfig);
            _popupManager.ShowPopup(loadingPopupConfig.PopupType);
        }

        private async void SendDeleteLevelRequest(long levelId, Action<long> onSuccess)
        {
            var result = await _levelService.DeleteLevelAsync(levelId);

            if (result.IsError)
            {
                Debug.LogError($"Failed to delete level with id={levelId}. Reason: {result.ErrorMessage}");
                ExitDeletingMode();
            }
            
            if (result.IsSuccess)
            {
                onSuccess?.Invoke(levelId);
            }
        }

        private void SetupLevelsData(BaseLevelItemArgs[] levels)
        {
            _levelPreviewItemArgs = new List<BaseLevelItemArgs>();
            foreach (var level in levels)
            {
                var arg = new LevelDraftPreviewItemArgs(level.Level, OnDraftClicked);
                _levelPreviewItemArgs.Add(arg);
            }
        }

        private void OnLevelDeleted(long id)
        {
            var levelIndex = _levelPreviewItemArgs.FindIndex(args => args.Level.Id == id);
            
            if (levelIndex < 0) 
            {
                return;
            }
            
            _levelPreviewItemArgs.RemoveAt(levelIndex);

            _draftsLeftToDelete--;
            
            if (_draftsLeftToDelete <= 0)
            {
                _scroller.ReloadData();
                ExitDeletingMode();
            }
            _selectedItem = null;
            RefreshSelection();
        }

        private void ExitDeletingMode()
        {
            _deletableGridManager.TurnOffSelectMode();
            _popupManager.ClosePopupByType(PopupType.Loading);
            _snackBarHelper.ShowInformationSnackBar(_draftDeletedMessage, 2);
        }
        
        private async Task DownloadNextDraftModels()
        {
            var result = await _levelService.GetLevelDraftsAsync(TAKE_NEXT_COUNT, _levelPreviewItemArgs.Count);

            if (result.IsSuccess)
            {
                var drafts = result.Levels.Select(level => new LevelDraftPreviewItemArgs(level, OnDraftClicked));
                _levelPreviewItemArgs.AddRange(drafts);
            }
            else if (!result.IsCancelled)
            {
                Debug.LogError($"Failed to download level models. Reason: {result.ErrorMessage}");
            }
        }

        private bool ShouldDownloadNextPageVideos()
        {
            return !_isDownloading && _scroller.NormalizedScrollPosition <= LOADING_NEXT_PAGE_POSITION_THRESHOLD;
        }
        
        private async void DownloadNextPage()
        {
            _isDownloading = true;
            await DownloadNextDraftModels();
            
            _scroller._Resize(true);
            _scroller._RefreshActive();
            _isDownloading = false;
        }
        
        private void OnBackButtonClick()
        {
            Manager.MoveBack();
        }
        
        private void ScrollerScrolled(EnhancedScroller scroller, Vector2 val, float scrollPosition)
        {
            if (!ShouldDownloadNextPageVideos()) return;

            DownloadNextPage();
        }

        private void OnDraftClicked(BaseLevelItemArgs levelItemArgs)
        {
            if (_selectedItem == levelItemArgs)
            {
                _selectedItem = null;
            }
            else
            {
                _selectedItem = levelItemArgs;
            }
            RefreshSelection();
        }

        private void RefreshSelection()
        { 
            foreach (var item in _scroller.GetActiveViews()) 
            {
                var draftsRow = item.GetComponent<DraftsRow>();
                draftsRow.SelectedItem = _selectedItem;
                draftsRow.UpdateSelectedItem();
            }
            _actionPanel.SetActive(_selectedItem != null);
        }

        private void OnSelectionModeOn()
        {
            _selectedItem = null;
            RefreshSelection();
        }

        private void EditSelectedDraft()
        {
            if (_selectedItem == null) return;

            DownloadFullLevelData(_selectedItem.Level);
        }

        private async void DownloadFullLevelData(Level level)
        {
            _editButton.interactable = false;
            
            var result = await _levelService.GetLevelAsync(level.Id);

            if (IsDestroyed) return;
            
            if (result.IsSuccess)
            {
                OnLevelDataLoaded(result.Level);
            }
            else if (result.ErrorMessage.Contains(Constants.ErrorMessage.ASSET_INACCESSIBLE_IDENTIFIER))
            {
                _snackBarHelper.ShowAssetInaccessibleSnackBar();
            }

            await Task.Delay(TimeSpan.FromSeconds(0.2f));
            
            if (IsDestroyed) return;

            _editButton.interactable = true;
        }

        void OnLevelDataLoaded(Level level)
        {
            var isTaskDraft = level.SchoolTaskId.HasValue && !level.RemixedFromLevelId.HasValue;
            if (isTaskDraft)
            {
                _scenarioManager.ExecuteTaskDraftEditing(level);
            }
            else
            {
                AdvancedInputFieldUtils.AddBindingsFromText(level.Description);
                _scenarioManager.ExecuteDraftEditing(level);
            }
        }

        private void ShowDeletePopup()
        {
            if (_selectedItem == null) return;

            var configuration = new DialogDarkPopupConfiguration
            {
                PopupType = PopupType.DialogDarkV3,
                Title = _loc.DeletePopupTitle,
                Description = _loc.DeletePopupDesc,
                YesButtonSetTextColorRed = true,
                YesButtonText = _loc.DeletePopupPositive,
                NoButtonText = _loc.DeletePopupNegative,
                OnYes = DeleteSelectedDraft,
            };

            _popupManager.SetupPopup(configuration);
            _popupManager.ShowPopup(configuration.PopupType);
        }

        private void DeleteSelectedDraft()
        {
            ShowLoadingPopup(_loc.DeletingDraftsMessage);
            _draftDeletedMessage = _loc.DraftDeletedMessage;

            SendDeleteLevelRequest(_selectedItem.Level.Id, OnLevelDeleted);
        }
    }
}