using System.Collections.Generic;
using Bridge;
using Common;
using Common.BridgeAdapter;
using Models;
using Modules.LocalStorage;
using Modules.TempSaves.Manager;
using Modules.UserScenarios.Common;
using Navigation.Core;
using UIManaging.PopupSystem;
using UIManaging.PopupSystem.Configurations;
using UIManaging.SnackBarSystem;
using UnityEngine;
using Zenject;

namespace Modules.TempSaves.Checker
{
    internal sealed class TempLevelSaveChecker : MonoBehaviour
    {
        private static readonly HashSet<PageId> VALID_PAGE_IDS = new()
        {
            PageId.HomePage,
            PageId.HomePageSimple,
            PageId.Feed,
            PageId.GamifiedFeed,
        };
        
        [Inject] private TempFileManager _tempFileManager;
        [Inject] private PageManager _pageManager;
        [Inject] private PopupManager _popupManager;
        [Inject] private IBridge _bridge;
        [Inject] private ILevelService _levelService;
        [Inject] private SnackBarHelper _snackBarHelper;
        [Inject] private IScenarioManager _scenarioManager;

        private Level _localLevel;
        private Level _savedLevel;

        //---------------------------------------------------------------------
        // Messages
        //---------------------------------------------------------------------

        private void Start()
        {
            _pageManager.PageDisplayed += OnPageDisplayed;
        }

        private void OnPageDisplayed(PageData pageData)
        {
            if (!VALID_PAGE_IDS.Contains(pageData.PageId)) return;

            CheckTempLevel();
            _pageManager.PageDisplayed -= OnPageDisplayed;
        }

        //---------------------------------------------------------------------
        // Helpers
        //---------------------------------------------------------------------

        private void CheckTempLevel()
        {
            _localLevel = _tempFileManager.GetData<Level>(Constants.FileDefaultPaths.LEVEL_TEMP_PATH);
            if(_localLevel == null) return;

            if (_localLevel.Id > 0)
            {
                DownloadLevelDataAsync(_localLevel.Id);
            }

            var configuration = new DialogPopupConfiguration()
            {
                PopupType = PopupType.UnfinishedVideo,
                Title = "One of your videos is unfinished",
                Description = "Would you like to continue editing your video?",
                NoButtonText = "No, delete it",
                OnNo = OnNoClicked,
                OnYes = OnYesClicked,
                YesButtonText = "Continue to edit"
            };

            _popupManager.SetupPopup(configuration);
            _popupManager.ShowPopup(configuration.PopupType);
        }

        private void OnYesClicked()
        {
            _scenarioManager.ExecuteLocalSavedLevelEditing(_localLevel, _savedLevel);
        }

        private void OnNoClicked()
        {
            LocalStorageManager.DeleteFiles();
            _tempFileManager.RemoveTempFile(Constants.FileDefaultPaths.LEVEL_TEMP_PATH);
            _snackBarHelper.ShowSuccessSnackBar("Video Deleted", 3);
        }

        private async void DownloadLevelDataAsync(long id)
        {
            var result = await _levelService.GetLevelAsync(id);
            if (result.IsSuccess)
            {
                _savedLevel = result.Level;
            }
        }
    }
}