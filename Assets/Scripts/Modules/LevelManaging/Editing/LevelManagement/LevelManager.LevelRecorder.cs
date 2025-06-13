using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Bridge.Models.ClientServer.Assets;
using Bridge.Models.Common.Files;
using Common;
using Extensions;
using Modules.FaceAndVoice.Voice.Recording.Core;
using Modules.LevelManaging.Editing.EventRecording;
using Modules.LevelManaging.Editing.Players;
using Modules.LevelManaging.Editing.RecordingModeSelection;
using Modules.LevelManaging.Editing.ThumbnailCreator;
using UnityEngine;
using CharacterController = Models.CharacterController;
using Event = Models.Event;
using PlayMode = Modules.LevelManaging.Editing.Players.EventPlaying.PlayMode;

namespace Modules.LevelManaging.Editing.LevelManagement
{
    internal sealed partial class LevelManager
    {
        private EventRecorder _eventRecorder;
        private IVoiceRecorder _voiceRecorder;
        private readonly EventThumbnailsCreatorManager _eventThumbnailsCreator;

        //---------------------------------------------------------------------
        // Events
        //---------------------------------------------------------------------
        
        public event Action EventForRecordingSetup;
        public event Action RecordingStarted;
        public event Action RecordingEnded;
        public event Action RecordingCancelled;
        public event Action FaceRecordingStateChanged;

        //---------------------------------------------------------------------
        // Properties
        //---------------------------------------------------------------------
        
        public bool IsFaceRecordingEnabled { get; private set; }
        public RecordingMode CurrentRecordingMode { get; private set; }

        private bool IsTargetEventSaved => TargetEvent.Id != 0;
        private bool _eventThumbnailCaptured;

        //---------------------------------------------------------------------
        // Public
        //---------------------------------------------------------------------
        
        public void SetFaceRecording(bool isEnabled)
        {
            IsFaceRecordingEnabled = isEnabled;
            SetFaceTracking(isEnabled);
            FaceRecordingStateChanged?.Invoke();
        }

        public void StartRecordingEvent()
        {
            ValidateTargetEventForRecording();
            SaveDayNightControllerValues();
            _activationCueManager.SetupActivationCues(TargetEvent);
            SetupRecordingMode();
            PlayEventForRecording();
            CaptureEventThumbnail();
        }

        public async Task StopRecordingEventAsync()
        {
            var result = await _eventRecorder.StopRecordingAsync();
            TargetEvent.SetCameraAnimation(result.CameraAnimation);
            TargetEvent.Length = (int) result.Length;

            var controllersWithFaceAndVoice = new List<CharacterController>();
            if (TargetEvent.TargetCharacterSequenceNumber >= 0)
            {
                var targetCharacterController = TargetEvent.GetTargetCharacterController();
                controllersWithFaceAndVoice.Add(targetCharacterController);
            }
            else
            {
                var allControllers = TargetEvent.CharacterController;
                controllersWithFaceAndVoice.AddRange(allControllers);
            }
            SetFaceAndVoice(result.FaceAnimation, result.VoiceTrack, controllersWithFaceAndVoice.ToArray());
            if (!_eventThumbnailCaptured)
            {
                await WaitThumbnailCapturing();
            }

            TargetEvent.HasActualThumbnail = true;
            RecordingEnded?.Invoke();
        }

        private async Task WaitThumbnailCapturing()
        {
            while (!_eventThumbnailCaptured)
            {
                await Task.Delay(20);
            }
        }

        public void CancelRecordingEvent()
        {
            _eventRecorder.CancelRecording();
            PlayEvent(PlayMode.PreRecording, TargetEvent);
            RecordingCancelled?.Invoke();
        }

        private void SetFaceAndVoice(FaceAnimationFullInfo faceAnimation, VoiceTrackFullInfo voiceTrack,
                                     params CharacterController[] targetControllers)
        {
            foreach (var characterController in targetControllers)
            {
                var faceVoice = characterController.GetCharacterControllerFaceVoiceController();
                faceVoice.FaceAnimation = faceAnimation;
                faceVoice.VoiceTrack = voiceTrack;
            }
        }

        //---------------------------------------------------------------------
        // Helpers
        //---------------------------------------------------------------------

        partial void InitializeRecorder(IVoiceRecorder voiceRecorder, EventRecorder eventRecorder)
        {
            _voiceRecorder = voiceRecorder;
            _eventRecorder = eventRecorder;
        }
        
        private void SetupRecordingMode()
        {
            ChangeRecordingMode(TargetEvent.HasMusic()? RecordingMode.LipSync: RecordingMode.Story);
        }

        private void PlayEventForRecording()
        {
            _previewManager.PlayEvent(PlayMode.Recording, TargetEvent, PreviewCleanMode.KeepLastEvent, StartRecording);
            
            void StartRecording()
            {
                EventForRecordingSetup?.Invoke();
                var songStartTime = TargetEvent.HasMusic()
                    ? TargetEvent.GetMusicController().ActivationCue
                    : 0;
                _eventRecorder.StartRecording(CurrentRecordingMode,
                                              songStartTime, IsFaceRecordingEnabled);
                RecordingStarted?.Invoke();
            }
        }

        private int GetSequenceNumberForNextEvent()
        {
            if (!CurrentLevel.Event.Any()) return 1;
            return CurrentLevel.Event.Max(x => x.LevelSequence) + 1;
        }
        
        private void ValidateTargetEventForRecording()
        {
            if (IsTargetEventSaved) throw new InvalidOperationException("Event must be not saved yet to be able record new one");
        }
        
        private void CaptureEventThumbnail()
        {
            var screenshotCamera = GetActiveCamera();
            CoroutineSource.Instance.StartCoroutine(TakeThumbnailAtEndOfFrame(screenshotCamera));
        }
        
        private IEnumerator TakeThumbnailAtEndOfFrame(Camera screenshotCamera)
        {
            _eventThumbnailCaptured = false;
            yield return new WaitForEndOfFrame();
            _eventThumbnailsCreator.CaptureThumbnails(TargetEvent.Id.ToString(), screenshotCamera, OnThumbnailsCaptured);
        }

        private void OnThumbnailsCaptured(FileInfo[] thumbnails)
        {
            TargetEvent.SetFiles(thumbnails);
            _eventThumbnailCaptured = true;
        }
    }
}