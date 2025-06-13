using System;
using Components;
using Modules.RenderingPipelineManagement;
using Modules.VideoStreaming.UIAnimators;
using Navigation.Args;
using Navigation.Core;
using Settings;
using UIManaging.Common.Args.Buttons;
using UIManaging.Common.PageHeader;
using UIManaging.PopupSystem;
using UIManaging.PopupSystem.Configurations;
using UnityEngine;
using Zenject;

namespace UIManaging.Pages.AppSettingsPage.UI
{
    internal sealed class AdvancedSettingsPage : GenericPage<AdvancedSettingsPageArgs>
    {
        [SerializeField] private PageHeaderView _pageHeaderView;
        [SerializeField] private PageUiAnimator _pageUiAnimator;

        [SerializeField] private Sprite _settingsHeaderIcon;
        [SerializeField] private ViewSpawner _viewSpawner;

        [Inject] private PopupManager _popupManager;
        [Inject] private PageManager _pageManager;
        [Inject] private IRenderingPipelineManager _renderingPipelineManager;

        private AdvancedSettingsPageLoc _loc;

        //---------------------------------------------------------------------
        // Properties
        //---------------------------------------------------------------------

        public override PageId Id { get; } = PageId.AdvancedSettings;

        //---------------------------------------------------------------------
        // Protected
        //---------------------------------------------------------------------

        protected override void OnInit(PageManager pageManager)
        {
            _loc = GetComponent<AdvancedSettingsPageLoc>();

            _pageHeaderView.Init(new PageHeaderArgs(_loc.PageHeader, new ButtonArgs(string.Empty, _pageManager.MoveBack)));
            SpawnSettingsItems();
        }

        protected override void OnDisplayStart(AdvancedSettingsPageArgs args)
        {
            _pageUiAnimator.PrepareForDisplay();
            _pageUiAnimator.PlayShowAnimation(() => OnShowAnimationFinished(args));
        }

        protected override void OnHidingBegin(Action onComplete)
        {
            _pageUiAnimator.PlayHideAnimation(()=>base.OnHidingBegin(onComplete));
        }

        //---------------------------------------------------------------------
        // Helpers
        //---------------------------------------------------------------------

        private void SpawnSettingsItems()
        {
            Color titleColor = Color.white;
            var settingsItems = new[]
            {
                new SettingItemViewArgs
                {
                    Icon = _settingsHeaderIcon,
                    Header = _loc.SettingsHeader
                },                
                new SettingItemViewArgs
                {
                    Title = _loc.HiResRenderTitle,
                    Description = _loc.HiResRenderDesc,
                    ShowToggle = true,
                    IsToggleOn = !AppSettings.UseOptimizedRenderingScale,
                    OnToggleValueChanged = OnOptimizedRenderingScaleChanged,
                    TitleColor = titleColor,
                    ShowBottomDivider = true
                },
                new SettingItemViewArgs
                {
                    Title = _loc.HiResExportTitle,
                    Description = _loc.HiResExportDesc,
                    ShowToggle = true,
                    IsToggleOn = !AppSettings.UseOptimizedCapturingScale,
                    OnToggleValueChanged = OnOptimizedCapturingScaleChanged,
                    TitleColor = titleColor,
                    ShowBottomDivider = true
                },
                new SettingItemViewArgs
                {
                    Title = _loc.OptiMemTitle,
                    Description = _loc.OptiMemDesc,
                    ShowToggle = true,
                    IsToggleOn = AppSettings.UseOptimizedMemory,
                    OnToggleValueChanged = OnOptimizedMemoryChanged,
                    TitleColor = titleColor,
                    ShowBottomDivider = true
                }
            };
            _viewSpawner.Spawn<SettingItemViewArgs, SettingItemView>(settingsItems);
        }

        private void OnOptimizedRenderingScaleChanged(bool value)
        {
            AppSettings.UseOptimizedRenderingScale = !value;
            
            if (value)
            {
                ShowAlertPopup($"{_loc.HiResRenderAlert1}\n\n{_loc.HiResRenderAlert2}");
                _renderingPipelineManager.SetHighQualityPipeline();
            }
            else
            {
                _renderingPipelineManager.SetDefaultPipeline();
            }
        }

        private void OnOptimizedCapturingScaleChanged(bool value)
        {
            AppSettings.UseOptimizedCapturingScale = !value;
            if (!value) return;
            
            ShowAlertPopup($"{_loc.HiResExportAlert1}\n\n{_loc.HiResExportAlert2}");
        }
        
        private void OnOptimizedMemoryChanged(bool value)
        {
            AppSettings.UseOptimizedMemory = value;
            if (value) return;
            
            ShowAlertPopup(_loc.OptiMemAlert);
        }

        private void ShowAlertPopup(string description)
        {
            var confirmInformationPopup = new AlertPopupConfiguration
            {
                PopupType = PopupType.AlertWithTitlePopup,
                Description = description,
                ConfirmButtonText = _loc.CommonAlertButton,
                Title = _loc.CommonAlertTitle
            };

            _popupManager.SetupPopup(confirmInformationPopup);
            _popupManager.ShowPopup(confirmInformationPopup.PopupType);
        }

        private void OnShowAnimationFinished(AdvancedSettingsPageArgs args)
        {
            base.OnDisplayStart(args);
            _pageHeaderView.SetBackButtonInteractivity(true);
        }
    }
}