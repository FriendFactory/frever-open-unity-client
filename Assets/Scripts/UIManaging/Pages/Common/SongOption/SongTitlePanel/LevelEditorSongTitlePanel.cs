using Extensions;

namespace UIManaging.Pages.Common.SongOption.SongTitlePanel
{
    public sealed class LevelEditorSongTitlePanel : SongTitlePanel
    {
        protected override void Awake()
        {
            base.Awake();
            LevelManager.RecordingStarted += ShowSelectedPanelIfNeeded;
            LevelManager.RecordingCancelled += ShowSongSelectionUI;
            LevelManager.RecordingEnded += ShowSongSelectionUI;
            LevelManager.EventPreviewStarted += ShowSelectedPanelIfNeeded;
            LevelManager.EventPreviewCompleted += ShowSongSelectionUI;
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            LevelManager.RecordingStarted -= ShowSelectedPanelIfNeeded;
            LevelManager.RecordingCancelled -= ShowSongSelectionUI;
            LevelManager.RecordingEnded -= ShowSongSelectionUI;
            LevelManager.EventPreviewStarted -= ShowSelectedPanelIfNeeded;
            LevelManager.EventPreviewCompleted -= ShowSongSelectionUI;
        }

        private void ShowSelectedPanelIfNeeded()
        {
            ShowSongPlayingUI();
            var eventHasMusic = LevelManager.TargetEvent.HasMusic();
            SongSelectedPanel.SetActive(eventHasMusic);
        }
    }
}