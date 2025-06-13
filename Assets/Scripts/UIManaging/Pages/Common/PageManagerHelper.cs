using Common;
using JetBrains.Annotations;
using Navigation.Args;
using Navigation.Core;
using UIManaging.Localization;
using UIManaging.PopupSystem;
using UIManaging.PopupSystem.Configurations;
using Zenject;

namespace UIManaging.Pages.Common
{
    [UsedImplicitly]
    public sealed class PageManagerHelper
    {
        [Inject] private PageManager _pageManager;
        [Inject] private PopupManager _popupManager;
        [Inject] private LoadingOverlayLocalization _loadingOverlayLocalization;

        private PageArgs _pageArgs;
        private bool _savePrevPageToHistory;

        public void MoveToLevelEditor(LevelEditorArgs levelEditorArgs, bool savePrevPageToHistory)
        {
            _savePrevPageToHistory = savePrevPageToHistory;

            var config = new SimulatedPageLoadingPopupConfiguration(
                _loadingOverlayLocalization.LevelEditorHeader,
                _loadingOverlayLocalization.LoadingProgressMessage,
                null);
            config.CancelActionRequested += levelEditorArgs.CancelLoadingAction;
            
            levelEditorArgs.HideLoadingPopupRequested += config.Hide;
            _pageArgs = levelEditorArgs;
            
            ShowLoadingPopup(config);
        }

        public void MoveToUmaEditor(UmaEditorArgs umaEditorArgs)
        {
            _savePrevPageToHistory = true;
            
            var config = new SimulatedPageLoadingPopupConfiguration(
                _loadingOverlayLocalization.WardrobeHeader,
                _loadingOverlayLocalization.LoadingProgressMessage,
                null);
            config.CancelActionRequested += umaEditorArgs.OnCancelLoadingRequested;
            
            umaEditorArgs.HideLoadingPopup += config.Hide;
            _pageArgs = umaEditorArgs;

            ShowLoadingPopup(config);
        }

        public void MoveToPiP(PostRecordEditorArgs pipArgs, bool savePrevPageToHistory)
        {
            _savePrevPageToHistory = savePrevPageToHistory;

            var config = new SimulatedPageLoadingPopupConfiguration(
                _loadingOverlayLocalization.LevelEditorHeader,
                _loadingOverlayLocalization.LoadingProgressMessage,
                null);
            config.CancelActionRequested += pipArgs.CancelLoadingAction;
            
            pipArgs.RequestHideLoadingPopup += config.Hide;
            _pageArgs = pipArgs;
            
            ShowLoadingPopup(config);
        }
        
        private void ShowLoadingPopup(BasePageLoadingPopupConfiguration config)
        {
            config.FadeInCompleted += OnFadeInAnimationDone;
            
            _popupManager.SetupPopup(config);
            _popupManager.ShowPopup(config.PopupType, true);
        }
        
        private void OnFadeInAnimationDone()
        {
            _pageManager.MoveNext(_pageArgs, _savePrevPageToHistory);
        }
    }
}