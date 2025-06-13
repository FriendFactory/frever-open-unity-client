using System;
using Bridge.Models.ClientServer.Tasks;
using Modules.LevelManaging.Editing.LevelManagement;
using UIManaging.PopupSystem;
using UnityEngine;
using Zenject;

namespace UIManaging.Pages.LevelEditor.Ui.PostRecordEditor.TaskCheckListComponents
{
    public sealed class TaskCheckList : MonoBehaviour
    {
        [SerializeField] private TaskHintsController _taskHintsController;
        
        [Inject] private TaskCheckListController _taskCheckListController;
        [Inject] private ILevelManager _levelManager;
        [Inject] private PopupManagerHelper _popupManagerHelper;

        private TaskFullInfo _taskFullInfo;

        //---------------------------------------------------------------------
        // Messages
        //---------------------------------------------------------------------

        public event Action Hidden;
        
        private void OnDestroy()
        {
            _taskCheckListController.CleanUp();
        }

        //---------------------------------------------------------------------
        // Public
        //---------------------------------------------------------------------
        
        public void Initialize(TaskFullInfo taskInfo)
        {
            _taskFullInfo = taskInfo;
        }

        public void ShowOverlay()
        {
            _popupManagerHelper.ShowTaskInfoPopup(_taskFullInfo, Hide);
        }

        public void ShowHints()
        {
            if (_taskCheckListController.TaskLevel == null)
            {
                return;
            }

            var assetsToShowHints = _taskCheckListController.GetUnchangedAssetInEvent(_levelManager.TargetEvent);
            var showEventHints = _taskCheckListController.TaskLevel.Event.Count > 1;
            _taskHintsController.Show(assetsToShowHints, showEventHints);
        }
        
        public void HideHints()
        {
            _taskHintsController.Hide();
        }

        public void Hide()
        {
            ShowHints();
            Hidden?.Invoke();
            gameObject.SetActive(false);
        }
    }
}
