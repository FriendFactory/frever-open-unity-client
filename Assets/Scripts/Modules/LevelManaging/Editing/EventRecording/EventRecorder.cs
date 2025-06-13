using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Bridge;
using Bridge.Models.ClientServer.Assets;
using Bridge.Models.Common.Files;
using Common;
using Extensions;
using JetBrains.Annotations;
using Modules.CameraSystem.CameraSystemCore;
using Modules.FaceAndVoice.Face.Playing.Core;
using Modules.FaceAndVoice.Face.Recording.Core;
using Modules.FaceAndVoice.Voice.Recording.Core;
using Modules.LevelManaging.Assets.AssetDependencies;
using Modules.LevelManaging.Editing.EventCreation;
using Modules.LevelManaging.Editing.Players;
using Modules.LevelManaging.Editing.RecordingModeSelection;
using UnityEngine;
using FileInfo = Bridge.Models.Common.Files.FileInfo;

namespace Modules.LevelManaging.Editing.EventRecording
{
    [UsedImplicitly]
    internal sealed class EventRecorder
    {
        private readonly IPreviewManager _previewManager;
        private readonly IFaceAnimRecorder _faceAnimRecorder;
        private readonly IVoiceRecorder _voiceRecorder;
        private readonly ICameraSystem _cameraSystem;
        private readonly IStopWatch _stopWatch;
        private readonly AudioSourceManager _audioSourceManager;
        private readonly IBridge _bridge;
        private readonly IAudioRecordingStateHolder _audioRecordingStateHolder;
        
        private int _soundActivationCue;

        //---------------------------------------------------------------------
        // Properties
        //---------------------------------------------------------------------

        private bool IsRecording { get; set; }

        //---------------------------------------------------------------------
        // Ctors
        //---------------------------------------------------------------------

        public EventRecorder(IPreviewManager previewManager, IFaceAnimRecorder faceAnimRecorder, 
            IVoiceRecorder voiceRecorder, AudioSourceManager audioSourceManager, ICameraSystem cameraSystem,
            IStopWatch stopWatch, IBridge bridge, IAudioRecordingStateHolder audioRecordingStateHolder)
        {
            _previewManager = previewManager;
            _faceAnimRecorder = faceAnimRecorder;
            _voiceRecorder = voiceRecorder;
            _audioSourceManager = audioSourceManager;
            _cameraSystem = cameraSystem;
            _stopWatch = stopWatch;
            _bridge = bridge;
            _audioRecordingStateHolder = audioRecordingStateHolder;
        }

        //---------------------------------------------------------------------
        // Public
        //---------------------------------------------------------------------

        public void StartRecording(
            RecordingMode mode,
            int soundActivationCue,
            bool recordFaceAnimation)
        {
            if (IsRecording)
            {
                throw new InvalidOperationException("Event recording have been running already");
            }

            _soundActivationCue = soundActivationCue;
            
            _stopWatch.Reset();
            _stopWatch.Start();
            
            IsRecording = true;

            if (recordFaceAnimation)
            {
                var source = GetAudioSourceForRecording(mode);
                _faceAnimRecorder.StartRecording(_stopWatch, source);   
            }

            var recordVoice = mode == RecordingMode.Story && _audioRecordingStateHolder.State == AudioRecordingState.Voice;
            if (recordVoice)
            {
                _voiceRecorder.StartRecording();
            }
            
            _cameraSystem.StartRecording(_stopWatch);
        }

        public void CancelRecording()
        {
            IsRecording = false;

            _faceAnimRecorder.CancelRecording();

            if (_voiceRecorder.IsRecording)
            {
                _voiceRecorder.CancelRecording();
            }

            _cameraSystem.StopAnimation();
            _cameraSystem.CancelRecording();
            _stopWatch.Stop();
            PauseSong();
        }
        
        public async Task<EventRecorderOutput> StopRecordingAsync()
        {
            IsRecording = false;
            _stopWatch.Stop();
            
            var output = new EventRecorderOutput
            {
                CameraAnimation = StopCameraAnimationRecording(),
                FaceAnimation = await StopFaceRecording(),
                VoiceTrack = StopVoiceRecording(),
                Length = _stopWatch.PreviousFrameMs //current frame is not being captured
            };

            PauseSong();
           
            return output;
        }

        private void PauseSong()
        { 
            _previewManager.PauseAudio();
        }

        private static FaceAnimationFullInfo CreateFaceAnimData(FaceAnimationClip clip, int soundActivationCue)
        {            
            var faceAnim = new FaceAnimationFullInfo
            {
                Duration = (int) clip.Duration.ToMilliseconds(),
                // important to save the music activation cue used during recording, because the first face animation frame
                // depends on ARKit face tracking. In case of bad face tracking session, we won't be able to restore relative to the audio
                // animation start time(due to missed first few frames), which is important for adapting face animations for other songs/activation cues
                MusicStartCue = soundActivationCue
            };

            var fileInfo = new FileInfo(clip.FullSavePath, FileType.MainFile);
            faceAnim.Files = new List<FileInfo> {fileInfo};
            return faceAnim;
        }

        private void SetupVoiceTrackFiles(VoiceTrackFullInfo voiceTrack)
        {
            var path = Path.Combine(Application.persistentDataPath, Constants.FileDefaultPaths.VOICE_TRACK_PATH);
            AudioSaver.Save(path, _voiceRecorder.AudioClip);
            var fileInfo = new FileInfo(path, FileType.MainFile);
            voiceTrack.Files = new List<FileInfo> {fileInfo};
        }

        private VoiceTrackFullInfo StopVoiceRecording()
        {
            if (!_voiceRecorder.IsRecording) return null;
            
            _voiceRecorder.StopRecording();
            var voiceTrack = new VoiceTrackFullInfo();
            SetupVoiceTrackFiles(voiceTrack);
            voiceTrack.VoiceOwnerGroupId = _bridge.Profile.GroupId;
            return voiceTrack;
        }

        private async Task<FaceAnimationFullInfo> StopFaceRecording()
        {
            if (!_faceAnimRecorder.IsRecording) return null;
            
            await _faceAnimRecorder.StopRecordingAsync();
            return CreateFaceAnimData(_faceAnimRecorder.AnimationClip, _soundActivationCue);;
        }

        private CameraAnimationFullInfo StopCameraAnimationRecording()
        {
            _cameraSystem.StopRecording();
            _cameraSystem.PauseAnimation();
            
            var cameraAnim = new CameraAnimationFullInfo();
            var file = new FileInfo(Path.Combine(Application.persistentDataPath,Constants.FileDefaultPaths.CAMERA_ANIMATION_PATH), FileType.MainFile);
            cameraAnim.Files = new List<FileInfo> {file};
            return cameraAnim;
        }

        private AudioSource GetAudioSourceForRecording(RecordingMode targetMode)
        {
            switch (targetMode)
            {
                case RecordingMode.LipSync:
                    return _audioSourceManager.SongAudioSource;
                case RecordingMode.Story:
                    return null;
                default:
                    throw new ArgumentOutOfRangeException(nameof(targetMode), targetMode, null);
            }
        }
    }
}