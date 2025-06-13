using EnhancedUI.EnhancedScroller;

namespace UIManaging.Pages.LevelEditor.Ui.PostRecordEditor
{
    internal sealed class EventTimelinePreviewView : EventsTimelineView<EventTimelinePreviewItemView>
    {
        protected override EventsTimelineModel TimelineModel => EditorPageModel?.PreviewEventsTimelineModel;
        
        protected override void OnCellViewInstantiated(EnhancedScroller enhancedScroller,
            EnhancedScrollerCellView cellView)
        {
        }

        protected override void OnShown()
        {
            base.OnShown();
            LevelManager.PlayingEventSwitched += OnNextEventPlaying;
        }

        protected override void OnHidden()
        {
            base.OnHidden();
            LevelManager.PlayingEventSwitched -= OnNextEventPlaying;
        }

        private void OnNextEventPlaying()
        {
            var currentEventIndex = LevelManager.TargetEvent.LevelSequence;
            JumpTo(currentEventIndex);
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            LevelManager.PlayingEventSwitched -= OnNextEventPlaying;
        }
    }
}