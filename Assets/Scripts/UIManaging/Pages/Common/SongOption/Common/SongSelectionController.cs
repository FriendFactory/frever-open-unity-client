using System;
using Bridge.Models.Common;
using JetBrains.Annotations;
using Modules.LevelManaging.Editing.LevelManagement;
using UIManaging.SnackBarSystem;


namespace UIManaging.Pages.Common.SongOption.Common
{
    [UsedImplicitly]
    public sealed class SongSelectionController
    {
        private readonly ILevelEditor _levelEditor;
        private readonly SnackBarHelper _snackBarHelper;

        public event Action<IPlayableMusic, IPlayableMusic> PlayedSongChanged;
        public event Action<IPlayableMusic, IPlayableMusic> SelectedSongChanged;
        public event SongSelected SongApplied;

        private IPlayableMusic _playedMusic;
        private IPlayableMusic _selectedSong;

        public bool IsSelectionForEventRecording { get;set; }
        
        public IPlayableMusic PlayedMusic
        {
            get => _playedMusic;
            set
            {
                if (_playedMusic != null && value != null && _playedMusic.Id == value.Id) return;
                
                var prev = _playedMusic;
                _playedMusic = value;
                
                PlayedSongChanged?.Invoke(prev, _playedMusic);
            }
        }

        public IPlayableMusic SelectedSong
        {
            get => _selectedSong;
            set
            {
                if (value?.Id == _selectedSong?.Id) return;
                var prev = _selectedSong;
                _selectedSong = value;
                SelectedSongChanged?.Invoke(prev, _selectedSong);
            }
        }

        public SongSelectionController(ILevelEditor levelEditor, SnackBarHelper snackBarHelper)
        {
            _levelEditor = levelEditor;
            _snackBarHelper = snackBarHelper;
        }

        public void ApplySong(IPlayableMusic song, int activationCue, bool ignoreMusicUsageError = false)
        {
            string reason = null;
            if (song != null)
            {
                if (!ignoreMusicUsageError 
                    && IsSelectionForEventRecording 
                    && !_levelEditor.CanUseForRecording(song, ref reason))
                {
                    _snackBarHelper.ShowFailSnackBar(reason);
                    return;
                }
                
                if (!IsSelectionForEventRecording && !_levelEditor.CanUseForReplacing(song, activationCue, ref reason))
                {
                    _snackBarHelper.ShowFailSnackBar(reason);
                    return;
                }
            }
            
            SongApplied?.Invoke(song, activationCue);
            if (song == null)
            {
                Reset();
            }
            else
            {
                _selectedSong = SelectedSong;
            }
        }

        private void Reset()
        {
            SelectedSong = null;
            PlayedMusic = null;
        }
    }

    public delegate void SongSelected(IPlayableMusic selectedSong, int activationCue);
}
