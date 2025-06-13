using System;
using System.Collections.Generic;
using EnhancedUI.EnhancedScroller;
using UIManaging.Pages.LevelEditor.Ui.PostRecordEditor.TaskCheckListComponents;
using Zenject;

namespace UIManaging.Pages.LevelEditor.Ui.PostRecordEditor
{
    internal sealed class EventTimelinePostRecordingView : EventsTimelineView<EventTimelinePostRecordingItemView>
    {
        [Inject] private TaskCheckListController _taskCheckListController;
        
        private readonly List<EventTimelinePostRecordingItemView> _eventViews = new List<EventTimelinePostRecordingItemView>();
        
        public event Action<EventTimelineItemModel> EventEditButtonClicked;

        protected override EventsTimelineModel TimelineModel => EditorPageModel?.PostRecordEventsTimelineModel;

        public void ShowHints()
        {
            foreach (var view in _eventViews)
            {
                var hasAssetsUnchanged = _taskCheckListController.GetUnchangedAssetInEvent(view.ContextData.Event).Length > 0;
                if (hasAssetsUnchanged)
                {
                    view.ShowHint();
                }
            }
        }
        
        public void HideHints()
        {
            foreach (var view in _eventViews)
            {
                view.HideHint();
            }
        }

        protected override void OnCellViewInstantiated(EnhancedScroller enhancedScroller, EnhancedScrollerCellView cellView)
        {
            var itemView = cellView.GetComponent<EventTimelinePostRecordingItemView>();
            itemView.EditButtonClicked += OnEditButtonClicked;
        }

        protected override void CellViewVisibilityChanged(EnhancedScrollerCellView cellView)
        {
            base.CellViewVisibilityChanged(cellView);
            var itemView = cellView.GetComponent<EventTimelinePostRecordingItemView>();
            if (cellView.active)
            {
                itemView.ContextData.IsSelectedChanged += OnSelectedChanged;
                _eventViews.Add(itemView);
            }
            else
            {
                itemView.ContextData.IsSelectedChanged -= OnSelectedChanged;
                _eventViews.Remove(itemView);
            }
        }

        private void OnEditButtonClicked(EventTimelineItemModel itemModel)
        {
            EventEditButtonClicked?.Invoke(itemModel);
        }
        
        private void OnSelectedChanged(EventTimelineItemModel itemModel)
        {
            if (_enhancedScroller == null || !_enhancedScroller.gameObject.activeSelf || !_enhancedScroller.gameObject.activeInHierarchy) return;
            if (itemModel?.Event == null || !itemModel.IsSelected) return;

            JumpTo(itemModel.Event.LevelSequence);
        }
    }
}
