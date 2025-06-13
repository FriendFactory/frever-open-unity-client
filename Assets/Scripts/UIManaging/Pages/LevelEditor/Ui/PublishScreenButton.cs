using System.Linq;
using System.Threading.Tasks;
using Bridge;
using Bridge.Models.ClientServer.Assets;
using Common;
using Extensions;
using Models;
using Modules.LevelManaging.Editing.LevelManagement;
using UIManaging.Pages.LevelEditor.Ui.PostRecordEditor;
using UIManaging.Pages.LevelEditor.Ui.PostRecordEditor.TaskCheckListComponents;
using UIManaging.SnackBarSystem;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace UIManaging.Pages.LevelEditor.Ui
{
    internal sealed class PublishScreenButton: MonoBehaviour
    {
        [SerializeField] private Button _button;
        [SerializeField] private OnPointerClickHandler _nextButtonClickHandler;
        [SerializeField] private LoadingOverlay _loadingOverlay;
        [SerializeField] private TaskCheckList _taskCheckList;

        [Inject] private ILevelManager _levelManager;
        [Inject] private PostRecordEditorPageModel _editorPageModel;
        [Inject] private ILevelAssetsUnCompressingService _levelAssetsUnCompressingService;
        [Inject] private SnackBarHelper _snackBarHelper;
        [Inject] private TaskCheckListController _taskCheckListController;
        [Inject] private IBridge _bridge;

        private bool IsTask => _levelManager.CurrentLevel?.SchoolTaskId != null &&
                               _levelManager.CurrentLevel?.SchoolTaskId != 0;

        //---------------------------------------------------------------------
        // Messages
        //---------------------------------------------------------------------

        private void Reset()
        {
            _button = GetComponentInChildren<Button>();
        }

        private void OnEnable()
        {
            _nextButtonClickHandler.Click += OnClicked;
        }

        private void Update()
        {
            if (IsTask && _taskCheckListController.IsEnabled)
            {
                _button.interactable = _taskCheckListController.HasAllAssetsChanged();
            }
        }

        private void OnDisable()
        {
            _nextButtonClickHandler.Click -= OnClicked;
        }

        //---------------------------------------------------------------------
        // Helpers
        //---------------------------------------------------------------------

        private void OnClicked()
        {
            if (IsTask && _taskCheckListController.IsEnabled && !_taskCheckListController.HasAllAssetsChanged())
            {
                _taskCheckList.ShowHints();
                return;
            }

            _loadingOverlay.Show(true);
            StartUnpackingAllBundlesFromLevel();

            var levelCopy = _levelManager.CurrentLevel.Clone();
            _levelManager.SaveLevel(levelCopy, OnSaved, OnSaveFailed);
            
            return;
            void OnSaved(Level level)
            {
                _loadingOverlay.Hide();
                _levelManager.CurrentLevel = level;
                _levelManager.ClearTempFiles();
                _editorPageModel.ClosePostRecordEditor();
                _editorPageModel.OnMovingForward?.Invoke();
            }
            
            void OnSaveFailed(string message)
            {
                if (message.Contains(Constants.ErrorMessage.ASSET_INACCESSIBLE_IDENTIFIER))
                {
                    _snackBarHelper.ShowInformationSnackBar(Constants.ErrorMessage.UNABLE_TO_SAVE_LEVEL_INACCESSIBLE_ASSETS);
                }
                else
                {
                    Debug.LogError(message);
                }
                
                _loadingOverlay.Hide();
            }
        }

        private async void StartUnpackingAllBundlesFromLevel()
        {
            await _levelAssetsUnCompressingService.UnCompressHeavyBundles(_levelManager.CurrentLevel);
        }
    }
}