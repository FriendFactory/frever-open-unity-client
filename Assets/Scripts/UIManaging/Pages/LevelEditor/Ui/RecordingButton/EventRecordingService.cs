using System;
using System.IO;
using Bridge.Models.Common;
using Bridge.Models.Common.Files;
using Extensions;
using JetBrains.Annotations;
using Modules.LevelManaging.Assets;
using Modules.LevelManaging.Editing.LevelManagement;
using UIManaging.SnackBarSystem;
using UnityEngine;
using Zenject;
using Event = Models.Event;
using PlayMode = Modules.LevelManaging.Editing.Players.EventPlaying.PlayMode;

namespace UIManaging.Pages.LevelEditor.Ui.RecordingButton
{
    [UsedImplicitly]
    public sealed class EventRecordingService  : ITickable
    {
        private readonly EventRecordingStateController _stateController;
        private readonly ILevelManager _levelManager;
        private readonly SnackBarHelper _snackBarHelper;
        
        private float _maxRecordingDurationMs;
        private float _recordedDuration;
        
        private bool _hasSong;

        //---------------------------------------------------------------------
        // Events
        //---------------------------------------------------------------------
        
        public event Action RecordingStarted;
        public event Action RecordingEnded;
        public event Action RecordingCancelled;
        public event Action<float> RecordTick;
        
        //---------------------------------------------------------------------
        // Properties
        //---------------------------------------------------------------------
        
        public bool IsRecording { get; private set; }
        public bool IsRecordingAllowed { get; private set; }
        public bool IsSavingLastRecording { get; private set; }
        
        //---------------------------------------------------------------------
        // Ctors
        //---------------------------------------------------------------------

        internal EventRecordingService(EventRecordingStateController stateController, ILevelManager levelManager, SnackBarHelper snackBarHelper)
        {
            _stateController = stateController;
            _levelManager = levelManager;
            _snackBarHelper = snackBarHelper;

            _stateController.StateUpdated += SetRecordingAllowed;
        }

        //---------------------------------------------------------------------
        // Public
        //---------------------------------------------------------------------

        public void StartRecording()
        {
            if (IsRecording || !IsRecordingAllowed)
                return;

            var hasExternalTrack = _levelManager.TargetEvent?.HasExternalTrack() ?? false;
            var hasSong = _levelManager.TargetEvent?.HasMusic() ?? false;
            var songAsset = _levelManager.GetCurrentAudioAsset();
            _hasSong = (hasExternalTrack || hasSong) && songAsset != null;
            
            string reason = null;
            if (_hasSong && !_levelManager.CanUseForRecording(songAsset.Entity as IPlayableMusic, ref reason))
            {
                _snackBarHelper.ShowFailSnackBar(reason);
                return;
            }

            _maxRecordingDurationMs = GetMaxRecordingLength(songAsset);
            _recordedDuration = 0;
            IsRecording = true;
            _levelManager.StartRecordingEvent();

            RecordingStarted?.Invoke();
        }

        private long GetMaxRecordingLength(IAudioAsset songAsset)
        {
            var maxLevelLength = _levelManager.MaxLevelDurationSec.ToMilliseconds();
            var leftForRecording = maxLevelLength - _levelManager.LevelDurationSeconds.ToMilliseconds();

            if (!_hasSong) return leftForRecording;
            
            var musicLeftForRecording = songAsset.MusicModel.IsLicensed()
                ? _levelManager.GetAllowedDurationForNextRecordingInSec(songAsset.Id).ToMilliseconds()
                : songAsset.TotalLength.ToMilliseconds();
            return (long)Mathf.Min(leftForRecording, musicLeftForRecording);
        }

        public void StopRecording()
        {
            if (!IsRecording) return;
            
            var reachMinThreshold = _recordedDuration.ToMilliseconds() > _levelManager.MinEventDurationMs;
            if (reachMinThreshold)
            {
                EndRecording();
            }
            else
            {
                CancelRecording();
            }
        }

        public void Tick()
        {
            if (!IsRecording) return;

            _recordedDuration += Time.deltaTime;
            var reachedEnd = _recordedDuration.ToMilliseconds() >= _maxRecordingDurationMs;

            if (reachedEnd)
            {
                StopRecording();
                return;
            }
            
            RecordTick?.Invoke(_recordedDuration);
        }

        public void Activate()
        {
            _stateController.StartControl();
        }

        public void Deactivate()
        {
            _stateController.StopControl();
        }
        
        //---------------------------------------------------------------------
        // Helpers
        //---------------------------------------------------------------------
        
        private void CancelRecording()
        {
            ResetRecording();
            OnRecordingCancelled();
            RecordingCancelled?.Invoke();
        }

        private void EndRecording()
        {
            ResetRecording();
            OnRecordingStopped();
            RecordingEnded?.Invoke();
        }

        private async void OnRecordingStopped()
        {
            IsSavingLastRecording = true;
            await _levelManager.StopRecordingEventAsync();
            IsSavingLastRecording = false;
            SaveRecordedEvent();
            _levelManager.StopCurrentPlayMode();
            _levelManager.PlayEvent(PlayMode.PreRecording, _levelManager.TargetEvent);
        }

        private void OnRecordingCancelled()
        {
            _levelManager.CancelRecordingEvent();
        }
        
        private void SaveRecordedEvent()
        {
            _levelManager.SaveRecordedEvent();
            RemoveTempThumbnailsForEvent(_levelManager.TargetEvent);
        }

        private void RemoveTempThumbnailsForEvent(Event ev)
        {
            if (ev?.Files == null) return;

            foreach (var t in ev.Files)
            {
                if (t.FileType == FileType.Thumbnail)
                {
                    File.Delete(t.FilePath);
                }
            }

            ev.Files.Clear();
        }
        
        private void ResetRecording()
        {
            IsRecording = false;
            _recordedDuration = 0;
        }
        
        private void SetRecordingAllowed(bool isAllowed)
        {
            IsRecordingAllowed = isAllowed;
        }
    }
}