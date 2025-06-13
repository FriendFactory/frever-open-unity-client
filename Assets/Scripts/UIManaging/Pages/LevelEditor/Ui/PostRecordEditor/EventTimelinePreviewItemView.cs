using Modules.LevelManaging.Editing.LevelManagement;
using Zenject;

namespace UIManaging.Pages.LevelEditor.Ui.PostRecordEditor
{
    internal sealed class EventTimelinePreviewItemView : EventTimelineItemView
    {
        [Inject] private ILevelManager _levelManager;

        private bool IsTargetEvent => _levelManager.TargetEvent.Id == ContextData.Event.Id;

        //---------------------------------------------------------------------
        // Messages
        //---------------------------------------------------------------------
        
        private void OnEnable()
        {
            _levelManager.LevelPreviewStarted += OnPlayingEventSwitched;
            _levelManager.NextLevelPiecePlayingStarted += OnPlayingEventSwitched;
            _levelManager.PlayingEventSwitched += OnPlayingEventSwitched;
            _levelManager.EventLoadingCompleted += OnPlayingEventSwitched;
        }

        private void OnDisable()
        {
            _levelManager.EventLoadingCompleted -= OnPlayingEventSwitched;
            _levelManager.NextLevelPiecePlayingStarted -= OnPlayingEventSwitched;
            _levelManager.PlayingEventSwitched -= OnPlayingEventSwitched;
            _levelManager.LevelPreviewStarted -= OnPlayingEventSwitched;
        }

        protected override void OnDestroy()
        {
            ContextData.CleanUp();
            base.OnDestroy();
        }

        //---------------------------------------------------------------------
        // Protected
        //---------------------------------------------------------------------

        protected override void OnInitialized()
        {
            base.OnInitialized();
            SetSelected(IsTargetEvent);
        }

        protected override void BeforeCleanup()
        {
            base.BeforeCleanup();
            SetSelected(false);
        }
        
        protected override bool ShouldHighlight()
        {
            return IsTargetEvent;
        }
        
        //---------------------------------------------------------------------
        // Helpers
        //---------------------------------------------------------------------
        
        private void SetSelected(bool value)
        {
            ContextData.SetIsSelected(value);
            RefreshSelectionObject();
        }
        
        private void OnPlayingEventSwitched()
        {
            SetSelected(IsTargetEvent);
        }
    }
}