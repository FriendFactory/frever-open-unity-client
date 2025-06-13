using System;
using Modules.LevelManaging.Editing.LevelManagement;
using TMPro;
using UIManaging.Pages.LevelEditor.Ui.PostRecordEditor.FeatureControls;
using UIManaging.Pages.LevelEditor.Ui.PostRecordEditor.TaskCheckListComponents;
using UIManaging.Pages.LevelEditor.Ui.RecordingButton;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace UIManaging.Pages.LevelEditor.Ui.PostRecordEditor
{
    public sealed class EventTimelinePostRecordingItemView : EventTimelineItemView
    {
        [Inject] private ILevelManager _levelManager;
        [Inject] private IDeleteEventFeatureControl _deleteEventFeatureControl;

        public event Action<EventTimelineItemModel> EditButtonClicked; 
        
        [SerializeField] private Button _editButton;
        [SerializeField] private DeleteTargetEventButton _deleteTargetEventButton;
        [SerializeField] private TaskEventHint _taskEventHint;
        [SerializeField] private TMP_Text _durationText;

        private void OnDisable()
        {
            _taskEventHint.Hide();
        }

        public void ShowHint()
        {
            _taskEventHint.Show();
        }
        
        public void HideHint()
        {
            _taskEventHint.Hide();
        }
        
        protected override void OnInitialized()
        {
            base.OnInitialized();
            OnIsSelectedChanged(ContextData);
            _editButton.onClick.AddListener(OnEditButtonClicked);
            _deleteTargetEventButton.SetTargetEvent(ContextData.Event.Id);

            var duration = ContextData.Event.Length / 1000f;
            _durationText.text = $"{duration:F1}s";
        }

        protected override void OnIsSelectedChanged(EventTimelineItemModel model)
        {
            base.OnIsSelectedChanged(model);
            RefreshDeleteButton(model);
        }

        protected override void BeforeCleanup()
        {
            base.BeforeCleanup();
            _editButton.onClick.RemoveListener(OnEditButtonClicked);
        }

        private void OnEditButtonClicked()
        {
            ContextData.SetIsSelected(true);
            RefreshSelectionObject();
            EditButtonClicked?.Invoke(ContextData);
        }

        private void RefreshDeleteButton(EventTimelineItemModel model)
        {
            var deleteButtonShouldBeActive = _deleteEventFeatureControl.IsFeatureEnabled && model.IsSelected && _levelManager.CurrentLevel.Event.Count > 1;
            _deleteTargetEventButton.gameObject.SetActive(deleteButtonShouldBeActive);
        }
    }
}
