using Bridge.Models.Common;
using Extensions;
using Modules.Amplitude;
using JetBrains.Annotations;
using Modules.LevelManaging.Editing.LevelManagement;
using UIManaging.Pages.Common.SongOption.Common;
using UnityEngine;
using Zenject;
using PlayMode = Modules.LevelManaging.Editing.Players.EventPlaying.PlayMode;

namespace UIManaging.Pages.Common.SongOption.SongTitlePanel
{
    public class SongTitlePanel : MonoBehaviour
    {
        [SerializeField] protected GameObject NoSongSelectedPanel;
        [SerializeField] protected SongSelectedPanel SongSelectedPanel;

        [Inject] protected ILevelManager LevelManager;
        [Inject] private SongSelectionController _songSelectionController;
        private bool _noSongWasSelected;
        
        //---------------------------------------------------------------------
        // Messages
        //---------------------------------------------------------------------

        protected virtual void Awake()
        {
            _songSelectionController.SongApplied += SetAppliedSong;
            LevelManager.EventLoadingCompleted += OnEventLoaded;
        }

        protected virtual void OnDestroy()
        {
            _songSelectionController.SongApplied -= SetAppliedSong;
            LevelManager.EventLoadingCompleted -= OnEventLoaded;
        }

        //---------------------------------------------------------------------
        // Public
        //---------------------------------------------------------------------

        public void SetAppliedSong(IPlayableMusic selected, int activationCue)
        {
            if (selected != null)
            {
                SongSelectedPanel.UpdateSongTitle(selected);
                DisplaySongSelectedPanel();
            }
            else
            {
                DisplayNoSongSelectedPanel();
            }
        }

        public void DisplayNoSongSelectedPanel()
        {
            NoSongSelectedPanel.SetActive(true);
            SongSelectedPanel.SetActive(false);
        }

        //---------------------------------------------------------------------
        // Protected
        //---------------------------------------------------------------------
        
        protected void ShowSongSelectionUI()
        {
            if (_noSongWasSelected)
            {
                NoSongSelectedPanel.SetActive(true);
                return;
            }

            SongSelectedPanel.DisplaySelectionUI();
        }

        protected void ShowSongPlayingUI()
        {
            NoSongSelectedPanel.SetActive(false);
            SongSelectedPanel.DisplayPlayingUI();
        }

        protected void RefreshState()
        {
            var targetEvent = LevelManager.TargetEvent;
            var musicController = targetEvent.GetMusicController();

            if (musicController == null)
            {
                DisplayNoSongSelectedPanel();
                return;
            }
            
            var song = musicController.Song;
            var userSound = musicController.UserSound;
            var externalSong = musicController.ExternalTrack;
            var currentMusic = song ?? (IPlayableMusic) userSound ?? externalSong;
            SetAppliedSong(currentMusic, musicController.ActivationCue);
        }

        //---------------------------------------------------------------------
        // Helpers
        //---------------------------------------------------------------------
        
        private void OnEventLoaded()
        {
            var playMode = LevelManager.CurrentPlayMode;
            if (playMode == PlayMode.Preview || playMode == PlayMode.Recording || playMode == PlayMode.PreviewWithCameraTemplate) return;
            RefreshState();
        }

        private void DisplaySongSelectedPanel()
        {
            SongSelectedPanel.gameObject.SetActive(true);
            NoSongSelectedPanel.SetActive(false);
        }
    }
}