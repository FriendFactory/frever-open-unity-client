using System;
using System.Threading.Tasks;
using Common;
using Extensions;
using JetBrains.Annotations;
using Models;
using Modules.LevelManaging.Editing.LevelManagement;
using Modules.LocalStorage;
using Navigation.Args;
using UIManaging.Pages.LevelEditor.EditingPage;
using UIManaging.PopupSystem;
using UIManaging.PopupSystem.Configurations;
using UIManaging.SnackBarSystem;
using Zenject;

namespace UIManaging.Pages.LevelEditor.Ui.Exit
{
    [UsedImplicitly]
    internal sealed class StartOverMenuExitButtonClickHandler : ExitButtonClickHandler
    {
        [Inject] private PopupManager _popupManager;
        [Inject] private PopupManagerHelper _popupManagerHelper;
        [Inject] private ILevelManager _levelManager;
        [Inject] private LevelEditorPageModel _levelEditorPageModel;
        [Inject] private EditingPageLoading _loadingScreen;
        [Inject] private SnackBarHelper _snackBarHelper;

        public override ExitButtonBehaviour ExitButtonBehaviour => ExitButtonBehaviour.StartOverMenu;
        
        public override async Task HandleClickAsync()
        {
            Action onSaveDraft = await LevelContainsUnsavedChanges() ? SaveDraft : null;
            _popupManagerHelper.OpenStartOverLevelEditorPopup(() => OnExit(false), OnStartOver, onSaveDraft , OnCancel);
        }
        
        private Task<bool> LevelContainsUnsavedChanges()
        {
            return _levelEditorPageModel.HasUserChangedTheOriginalLevel();
        }

        private void OnStartOver()
        {
            _levelManager.CleanUp();
            _levelEditorPageModel.RequestStartOver();
        }

        private async void SaveDraft()
        {
            var level = _levelManager.CurrentLevel;
        
            if (level.IsEmpty()) return;

            var config = new SimulatedPageLoadingPopupConfiguration(
                "Saving to drafts",
                "Saving",
                null);
            _popupManager.SetupPopup(config);
            _popupManager.ShowPopup(config.PopupType);
        
            await level.ResetLocalIdsAsync();
        
            _levelManager.SaveLevel(level, OnDraftSaved, OnDraftSaveFailed);
        }
        
        private void OnDraftSaved(Level lvl)
        {
            LocalStorageManager.DeleteFiles();
            OnExit(true);
        }
        
        private void OnDraftSaveFailed(string error)
        {
            if (error.Contains(Constants.ErrorMessage.ASSET_INACCESSIBLE_IDENTIFIER))
            {
                _snackBarHelper.ShowInformationSnackBar(Constants.ErrorMessage.UNABLE_TO_SAVE_LEVEL_INACCESSIBLE_ASSETS);
            }
            else if(error.Contains(Constants.ErrorMessage.SOUND_INACCESSIBLE_IDENTIFIER))
            {
                _snackBarHelper.ShowInformationSnackBar(Constants.ErrorMessage.UNABLE_TO_SAVE_LEVEL_INACCESSIBLE_SOUND);
            }
            
            _levelManager.UseSameFaceFx = false;
            _levelManager.CancelLoading();
            _popupManagerHelper.HideLoadingOverlay();
        }

        private void OnCancel()
        {
            _levelEditorPageModel.OnExitCancelled();
        }
        
        private void OnExit(bool savedToDrafts)
        {
            _levelEditorPageModel.RequestExit(savedToDrafts);
        }
    }
}