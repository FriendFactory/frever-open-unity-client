using System;
using System.Collections.Generic;
using System.Linq;
using Bridge.Models.VideoServer;
using EnhancedUI.EnhancedScroller;
using TMPro;
using UIManaging.Common.Args.Buttons;
using UIManaging.Common.PageHeader;
using UIManaging.Localization;
using UIManaging.PopupSystem;
using UIManaging.PopupSystem.Configurations;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace UIManaging.Common.SelectableGrid
{
    public class DeletableGridManager : BaseSelectableGridManager
    {
        [SerializeField] private EnhancedScroller _enhancedScroller;
        [SerializeField] private Button _deleteButton;
        [SerializeField] private Button _backButton;
        [SerializeField] private TextMeshProUGUI _deleteButtonText;
        [SerializeField] private GameObject _deleteDialog;
        [SerializeField] private Button _cancelButton;

        [Inject] private PopupManager _popupManager;
        [Inject] private DraftGridLocalization _localization;

        private readonly List<long> _selectedIds = new List<long>();

        public event Action SelectionModeOn;

        private List<SelectableGridView> _selectableGridViews = new List<SelectableGridView>();

        private DeletableGridManagerArgs _deletableGridManagerArgs;
        private ButtonArgs _cancelButtonArgs;
        private ButtonArgs _selectButtonArgs;
        private ButtonArgs _closeButtonArgs;
        private bool _isSelectedModeActive;

        //---------------------------------------------------------------------
        // Messages
        //---------------------------------------------------------------------
        
        private void Awake()
        {
            _selectButtonArgs = new ButtonArgs(_localization.SelectButton, TurnOnSelectMode);
            _cancelButtonArgs = new ButtonArgs(_localization.CancelButton, TurnOffSelectMode);
            _closeButtonArgs = new ButtonArgs(string.Empty, OnCloseButtonClicked);
        }

        private void OnEnable()
        {
            _enhancedScroller.cellViewReused += OnCellViewReused;
            _enhancedScroller.cellViewWillRecycle += OnCellViewRecycled;
            RefreshDeleteSelectedCharactersButton();
            _deleteButton.onClick.AddListener(OnDeleteButtonClicked);
            _cancelButton.onClick.AddListener(TurnOffSelectMode);
        }

        private void OnDisable()
        {
            _enhancedScroller.cellViewReused -= OnCellViewReused;
            _enhancedScroller.cellViewWillRecycle -= OnCellViewRecycled;
            _deleteButton.onClick.RemoveListener(OnDeleteButtonClicked);
            _cancelButton.onClick.RemoveListener(TurnOffSelectMode);
        }

        //---------------------------------------------------------------------
        // Public
        //---------------------------------------------------------------------

        public void Init(DeletableGridManagerArgs deletableGridManagerArgs)
        {
            _deletableGridManagerArgs = deletableGridManagerArgs;
            var pageHeaderArgs = new PageHeaderActionArgs(_deletableGridManagerArgs.Title, _closeButtonArgs, _selectButtonArgs);
            pageHeaderActionView.Init(pageHeaderArgs);
        }

        public void OnCellViewInstantiated(EnhancedScrollerCellView cellView)
        {
            var selectableGridViews = cellView.GetComponentsInChildren<SelectableGridView>();

            foreach (var view in selectableGridViews)
            {
                view.OnSelectedStatusChangedEvent += OnItemSelectedStatusChanged;
                view.OnIdChangedEvent += OnIdChanged;
                SetupGridView(view);
            }
        }

        public void TurnOffSelectMode()
        {
            pageHeaderActionView.InitializeRightButton(_selectButtonArgs);
            _backButton.interactable = true;

            foreach (var view in _selectableGridViews)
            {
                view.OnSelectedStatusChangedEvent -= OnItemSelectedStatusChanged;
                view.OnIdChangedEvent -= OnIdChanged;
                view.SetSelectedModeActive(false);
            }

            _selectedIds.Clear();
            RefreshDeleteSelectedCharactersButton();
            _isSelectedModeActive = false;
        }
        //---------------------------------------------------------------------
        // Helpers
        //---------------------------------------------------------------------

        private void OnIdChanged(SelectableGridView selectableGridView)
        {
            var isSelected = _selectedIds.Contains(selectableGridView.Id);
            selectableGridView.SetToggle(isSelected);
        }
        
        private void TurnOnSelectMode()
        {
            pageHeaderActionView.InitializeRightButton(_cancelButtonArgs);
            _backButton.interactable = false;
            _selectableGridViews = _enhancedScroller.transform.GetComponentsInChildren<SelectableGridView>().ToList();
            
            foreach (var view in _selectableGridViews)
            {
                view.OnSelectedStatusChangedEvent += OnItemSelectedStatusChanged;
                view.OnIdChangedEvent += OnIdChanged;
                view.SetSelectedModeActive(true);
                RefreshView(view);
            }
            
            _isSelectedModeActive = true;
            SelectionModeOn?.Invoke();
        }

        private void OnCloseButtonClicked()
        {
            TurnOffSelectMode();
            _deletableGridManagerArgs.OnCloseButtonClicked?.Invoke();
        }

        private void OnDeleteButtonClicked()
        {
            ShowDeletePopup();
        }

        private void ConfirmDeletion()
        {
            for (var i = _selectedIds.Count - 1; i >= 0; i--)
            {
                var view = _selectableGridViews.FirstOrDefault(v => v.Id == _selectedIds[i]);
                if (view != null) _selectableGridViews.Remove(view);
            }
            
            _deletableGridManagerArgs.OnDeleteButtonClicked?.Invoke(_selectedIds.ToArray());
            TurnOffSelectMode();
        }

        private void OnItemSelectedStatusChanged(long itemId, bool isSelected)
        {
            if (isSelected)
            {
                if (!_selectedIds.Contains(itemId))
                {
                    _selectedIds.Add(itemId);
                }
            }
            else
            {
                _selectedIds.Remove(itemId);
            }

            RefreshDeleteSelectedCharactersButton();
        }

        private void OnCellViewRecycled(EnhancedScrollerCellView cellView)
        {
            var selectableGridViews = cellView.GetComponentsInChildren<SelectableGridView>();

            foreach (var view in selectableGridViews)
            {
                _selectableGridViews.Remove(view);
            }
        }
        
        private void OnCellViewReused(EnhancedScroller scroller, EnhancedScrollerCellView cellView)
        {
            var selectableGridViews = cellView.GetComponentsInChildren<SelectableGridView>();

            SetupSelectableGridViews(selectableGridViews);
        }

        private void SetupSelectableGridViews(SelectableGridView[] gridViews) 
        { 
            foreach (var view in gridViews)
            {
                SetupGridView(view);
            }
        }

        private void SetupGridView(SelectableGridView view)
        {
            view.SetSelectedModeActive(_isSelectedModeActive);
            _selectableGridViews.Add(view);
            RefreshView(view);
        }

        private void RefreshView(SelectableGridView view)
        {
            var isSelected = _selectedIds.Contains(view.Id);
            view.SetToggle(isSelected);
        }
        
        private void RefreshDeleteSelectedCharactersButton()
        {
            var counterText = _selectedIds.Count <= 1 
                ? _localization.DeleteVideosButtonFormat 
                : _localization.DeleteVideosPluralButtonFormat;
            _deleteButtonText.text = string.Format(counterText, _selectedIds.Count);
            var showDeleteButton = _selectedIds.Count > 0;
            _deleteDialog.SetActive(showDeleteButton);
        }
        
        private void ShowDeletePopup()
        {
            var title = (_selectedIds.Count <= 1)
                ? _localization.DeleteVideosPopupTitle
                : string.Format(_localization.DeleteVideosPopupTitlePluralFormat, _selectedIds.Count);

            var configuration = new DialogDarkPopupConfiguration
            {
                PopupType = PopupType.DialogDarkV3,
                Title = title,
                Description = _localization.DeleteVideosPopupDesc,
                YesButtonSetTextColorRed = true,
                YesButtonText = _localization.DeleteVideosConfirmButton,
                NoButtonText = _localization.DeleteVideosCancelButton,
                OnYes = ConfirmDeletion,
            };
                
            _popupManager.SetupPopup(configuration);
            _popupManager.ShowPopup(configuration.PopupType);
        }
    }
}