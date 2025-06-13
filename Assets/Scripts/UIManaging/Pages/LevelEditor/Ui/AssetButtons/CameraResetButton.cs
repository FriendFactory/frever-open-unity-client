using System;
using Modules.AssetsStoraging.Core;
using Modules.CameraSystem.CameraSystemCore;
using Modules.LevelManaging.Editing.CameraManaging.CameraInputHandling;
using Modules.LevelManaging.Editing.LevelManagement;
using Modules.LevelManaging.Editing.Templates;
using UIManaging.Pages.LevelEditor.Ui.AssetSettingsViews;
using UIManaging.Pages.LevelEditor.Ui.SelectionViews.AssetSelection;
using UIManaging.Pages.LevelEditor.Ui.SelectionViews.AssetSelection.AssetSelectors;
using UnityEngine;
using UnityEngine.UI;
using Zenject;
using Event = Models.Event;

namespace UIManaging.Pages.LevelEditor.Ui.AssetButtons
{
    internal sealed class CameraResetButton : BaseAssetButton
    {
        [SerializeField] private CameraAnimationSettingsView _cameraAnimationSettingsView;
        [SerializeField] private Image _icon;

        [Inject] private IDataFetcher _dataFetcher;
        [Inject] private ILevelManager _levelManager;
        [Inject] private ICameraSystem _cameraSystem;
        [Inject] private ICameraInputController _cameraInputController;
        [Inject] private ITemplateProvider _templateProvider;

        private Event _defaultEvent;
        private CameraAssetSelector _cameraAssetSelector;

        //---------------------------------------------------------------------
        // Events
        //---------------------------------------------------------------------

        public event Action<bool> StateChanged;

        //---------------------------------------------------------------------
        // Messages
        //---------------------------------------------------------------------

        private void Awake()
        {
            _cameraInputController.CameraModificationCompleted += OnSettingsChanged;
        }

        private void OnDestroy()
        {
            _cameraInputController.CameraModificationCompleted -= OnSettingsChanged;
        }

        //---------------------------------------------------------------------
        // Public
        //---------------------------------------------------------------------

        public void Setup(bool isNewLevel)
        {
            SetActive(!isNewLevel);
            UpdateDefaultEvent();
        }

        public void SetAssetSelectionManagerCameraModel(CameraAssetSelector cameraAssetSelector)
        {
            _cameraAssetSelector = cameraAssetSelector;
            _cameraAssetSelector.OnSelectedItemChangedEvent += OnCameraAnimationTemplateChanged;
            _cameraAnimationSettingsView.SettingChanged += OnSettingsChanged;
        }

        //---------------------------------------------------------------------
        // Protected
        //---------------------------------------------------------------------

        protected override void OnClicked()
        {
            ResetAllSettings();
        }

        //---------------------------------------------------------------------
        // Helpers
        //---------------------------------------------------------------------

        private async void UpdateDefaultEvent()
        {
            _defaultEvent = await _templateProvider.GetTemplateEvent(_dataFetcher.DefaultUserAssets.TemplateId);
        }

        private void ResetAllSettings()
        {
            _cameraSystem.SetupCameraSettingsStartValues();

            if (_defaultEvent != null)
            {
                _cameraAssetSelector.SetSelectedItemsAsInEvent(_levelManager, _defaultEvent);
            }

            _cameraAnimationSettingsView.ResetAllSettings();
            SetActive(false);
        }

        private void OnCameraAnimationTemplateChanged(AssetSelectionItemModel itemModel)
        {
            if (itemModel.IsDefault) return;
            SetActive(true);
        }

        private void OnSettingsChanged()
        {
            SetActive(true);

        }

        private void SetActive(bool isActive)
        {
            _icon.color = new Color(_icon.color.r, _icon.color.g, _icon.color.b, isActive ? 1 : 0.5f);
            Interactable = isActive;
            StateChanged?.Invoke(isActive);
        }
    }
}