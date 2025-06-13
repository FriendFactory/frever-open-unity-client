using Extensions;
using Modules.LevelManaging.Assets;
using UnityEngine;
using UnityEngine.UI;

namespace UIManaging.Pages.Common.SongOption.SongTitlePanel
{
    public sealed class EditPageSongTitlePanel : SongTitlePanel
    {
        [SerializeField] private Button _removeButton;

        protected override void Awake()
        {
            base.Awake();
            LevelManager.LevelPreviewStarted += ShowSongPlayingUI;
            LevelManager.LevelPreviewCompleted += ShowSongSelectionUI;
            LevelManager.PlayingEventSwitched += OnNextLevelPieceStarted;
            LevelManager.PreviewCancelled += ShowSongSelectionUI;
            _removeButton.onClick.AddListener(OnRemoveClicked);
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            LevelManager.LevelPreviewStarted -= ShowSongPlayingUI;
            LevelManager.LevelPreviewCompleted -= ShowSongSelectionUI;
            LevelManager.PlayingEventSwitched -= OnNextLevelPieceStarted;
            LevelManager.PreviewCancelled -= ShowSongSelectionUI;
            _removeButton.onClick.RemoveListener(OnRemoveClicked);
        }

        private void OnNextLevelPieceStarted()
        {
            var eventHasMusic = LevelManager.TargetEvent.HasMusic();
            SongSelectedPanel.SetActive(eventHasMusic);

            if (!eventHasMusic) return;
            
            ShowSongPlayingUI();
            RefreshState();
        }

        private void OnRemoveClicked()
        {
            LevelManager.ChangeSong(null, onCompleted:OnComplete);

            void OnComplete(IAsset asset)
            {
                SetAppliedSong(null, 0);
            }
        }
    }
}
