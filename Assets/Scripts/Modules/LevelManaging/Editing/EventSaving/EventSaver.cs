using System.Collections.Generic;
using System.Linq;
using Bridge.Models.Common.Files;
using Extensions;
using JetBrains.Annotations;
using Modules.LevelManaging.Editing.RecordingModeSelection;
using Modules.LocalStorage;
using UnityEngine;
using Utils;
using System.IO;
using Models;
using Modules.LevelManaging.Editing.LevelSaving;
using Event = Models.Event;
using FileInfo = Bridge.Models.Common.Files.FileInfo;
using static Common.Constants.FileDefaultNames;
using static Common.Constants.FileDefaultPaths;

namespace Modules.LevelManaging.Editing.EventSaving
{
    [UsedImplicitly]
    internal sealed class EventSaver
    {
        private readonly IDefaultThumbnailService _defaultThumbnailService;
        private readonly IAudioRecordingStateHolder _audioRecordingStateHolder;

        //---------------------------------------------------------------------
        // Properties
        //---------------------------------------------------------------------

        public Level CurrentLevel { get; private set; }

        //---------------------------------------------------------------------
        // Ctors
        //---------------------------------------------------------------------
        
        public EventSaver(IDefaultThumbnailService defaultThumbnailService, IAudioRecordingStateHolder audioRecordingStateHolder)
        {
            _defaultThumbnailService = defaultThumbnailService;
            _audioRecordingStateHolder = audioRecordingStateHolder;
        }
        
        //---------------------------------------------------------------------
        // Public
        //---------------------------------------------------------------------

        public void SaveEvent(Level currentLevel, Event currentEvent, RecordingMode recordingMode)
        {
            CurrentLevel = currentLevel;

            if (currentEvent.Id == 0)
            {
                ConfirmNewEvent(currentEvent, recordingMode);
            }
            else
            {
                Debug.LogError("New event should not have an ID.");
            }
        }

        //---------------------------------------------------------------------
        // Helpers
        //---------------------------------------------------------------------

        private void ConfirmNewEvent(Event currentEvent, RecordingMode recordingMode)
        {
            currentEvent.LevelId = CurrentLevel.Id;
            ConfirmLocalEvent(CurrentLevel, currentEvent, recordingMode);
            UpdateTargetEventIds(currentEvent, CurrentLevel.Event.Last());
        }

        private static void UpdateTargetEventIds(Event targetEvent, Event savedEvent)
        {
            var targetSetLocation = targetEvent.GetSetLocationController();
            var savedSetLocation = savedEvent.GetSetLocationController();

            if (targetSetLocation?.Photo != null && savedSetLocation?.Photo != null)
            {
                targetSetLocation.PhotoId = savedSetLocation.PhotoId;
                targetSetLocation.Photo = savedSetLocation.Photo;
            }

            if (targetSetLocation?.VideoClip != null && savedSetLocation?.VideoClip != null)
            {
                targetSetLocation.VideoClipId = savedSetLocation.VideoClipId;
                targetSetLocation.VideoClip = savedSetLocation.VideoClip;
            }
        }
        
        private void ConfirmLocalEvent(Level level, Event eventData, RecordingMode recordingMode)
        {
            level.AddEvent(eventData);
            level.ReplaceEmptyIds(LocalStorageManager.GetNextLocalId);

            var eventFolder = LocalStorageManager.GetEventPath(level.Id, eventData.Id);

            ConfirmCameraAnimation(eventData, eventFolder);
            ConfirmFaceAnimation(eventData, eventFolder);
            var recordVoice = recordingMode == RecordingMode.Story && _audioRecordingStateHolder.State == AudioRecordingState.Voice;
            if (recordVoice)
            {
                ConfirmVoiceTrack(eventData, eventFolder);
            }
            _defaultThumbnailService.FillMissedEventThumbnailsByDefault(eventData);
            ConfirmEventThumbnails(eventData, eventFolder);
        }

        private static void ConfirmCameraAnimation(Event eventData, string eventFolder)
        {
            var cameraAnimPath = $"{Application.persistentDataPath}/{CAMERA_ANIMATION_PATH}";
            var cameraAnimPathLocal = $"{eventFolder}/{CAMERA_ANIMATION_FILE}";

            FileUtil.CopyFile(cameraAnimPath, cameraAnimPathLocal, true);

            var cameraAnimationFiles = new List<FileInfo>
            {
                new FileInfo(cameraAnimPathLocal, FileType.MainFile)
            };

            foreach (var cameraController in eventData.CameraController)
            {
                cameraController.CameraAnimation.Files = cameraAnimationFiles;
            }
        }

        private static void ConfirmFaceAnimation(Event eventData, string eventFolder)
        {
            var faceAnimPath = $"{Application.persistentDataPath}/{FACE_ANIMATION_PATH}";
            if (!File.Exists(faceAnimPath)) return;

            var faceAnimPathLocal = $"{eventFolder}/{FACE_ANIMATION_FILE}";
            FileUtil.CopyFile(faceAnimPath, faceAnimPathLocal, true);

            var faceAnimationFiles = new List<FileInfo>
            {
                new FileInfo(faceAnimPathLocal, FileType.MainFile)
            };

            foreach (var characterController in eventData.CharacterController)
            {
                foreach (var faceVoice in characterController.CharacterControllerFaceVoice)
                {
                    if (faceVoice.FaceAnimation == null) continue;
                    faceVoice.FaceAnimation.Files = faceAnimationFiles;
                }
            }
        }

        private void ConfirmVoiceTrack(Event eventData, string eventFolder)
        {
            var voicePath = $"{Application.persistentDataPath}/{VOICE_TRACK_PATH}";
            var voicePathLocal = $"{eventFolder}/{VOICE_TRACK_FILE}";

            FileUtil.CopyFile(voicePath, voicePathLocal, true);

            var voiceTrackFiles = new List<FileInfo>
            {
                new FileInfo(voicePathLocal, FileType.MainFile)
            };

            foreach (var characterController in eventData.CharacterController)
            {
                foreach (var faceVoice in characterController.CharacterControllerFaceVoice)
                {
                    if (faceVoice.VoiceTrack == null) continue;
                    faceVoice.VoiceTrack.Files = voiceTrackFiles;
                }
            }
        }

        private static void ConfirmEventThumbnails(Event eventData, string eventFolder)
        {
            var thumbnails = eventData.Files.Where(info => info.FileType == FileType.Thumbnail).ToList();

            foreach (var thumbnail in thumbnails)
            {
                var thumbnailPath = thumbnail?.FilePath;
                var thumbnailResolution = thumbnail?.Resolution;
                var thumbnailPathLocal = $"{eventFolder}/thumbnail{thumbnailResolution}.png";

                FileUtil.CopyFile(thumbnailPath, thumbnailPathLocal, true);

                var thumbnailLocal = new FileInfo(thumbnailPathLocal, FileType.Thumbnail, thumbnail.Resolution.Value);

                eventData.Files.Remove(thumbnail);
                eventData.Files.Add(thumbnailLocal);
            }
        }
    }
}