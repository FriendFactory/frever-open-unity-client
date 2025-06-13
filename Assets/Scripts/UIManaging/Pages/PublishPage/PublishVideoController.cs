using Bridge;
using System;
using Common.ApplicationCore;
using Extensions;
using JetBrains.Annotations;
using Modules.LevelManaging.Editing;
using Modules.LevelManaging.Editing.LevelManagement;
using Modules.LevelManaging.Editing.Players;
using Modules.VideoRecording;
using Settings;
using UIManaging.PopupSystem.Configurations;
using UnityEngine;
using Zenject;

namespace UIManaging.Pages.PublishPage
{
    public interface IPublishVideoController
    {
        void SubscribeToEvents();
        void UnsubscribeFromEvents();
        void RenderVideo(bool saveToGallery, bool isLandscapeMode, bool isUltraHD, Action onCompleted);
        void Cancel();
        
        bool IsSavingInProgress { get; }
        string PortraitVideoFilePath { get; }
        IVideoRenderingState CurrentVideoRenderingState { get; }

        event Action OnRefocus;
        event Action RecordingCancelled;
    }
    
    internal sealed class PublishVideoController: IPublishVideoController
    {
        private IVideoRecorder _videoRecorder;
        private ILevelManager _levelManager;
        private LevelPreviewProgressCounter _previewProgressCounter;
        private IAppEventsSource _appEventsSource;

        private bool _saveToGallery;
        private bool _useLandscapeMode;
        private bool _useUltraHD;
        private Action _onVideoSaved;
        private Action _onCompleted;

        private float _progressMultiplier;
        private float _previousStepsProgress; // Portrait & Landscape rendering progresses are additive
        private int _renderedVideoCount;
        private bool _recordingInterruptedByBackgroundMode;
        private VideoRenderingState _videoRenderingState;

        [Inject] private IBridge _bridge;
        
        //---------------------------------------------------------------------
        // Properties
        //---------------------------------------------------------------------

        public IVideoRenderingState CurrentVideoRenderingState => _videoRenderingState;
        public bool IsSavingInProgress { get; private set; }
        public string PortraitVideoFilePath { get; private set; }
        private bool IsPortraitRenderingJustFinished => _renderedVideoCount == 1;
        private bool IsLandscapeRenderingJustFinished => _renderedVideoCount == 2;

        //---------------------------------------------------------------------
        // Events
        //---------------------------------------------------------------------

        public event Action OnRefocus;
        public event Action RecordingCancelled;

        //---------------------------------------------------------------------
        // Ctors
        //---------------------------------------------------------------------

        [Inject, UsedImplicitly]
        private void Construct(IVideoRecorder videoRecorder, ILevelManager levelManager,
            LevelPreviewProgressCounter progressCounter, IAppEventsSource appEventsSource)
        {
            _videoRecorder = videoRecorder;
            _levelManager = levelManager;
            _previewProgressCounter = progressCounter;
            _appEventsSource = appEventsSource;
            _videoRenderingState = new VideoRenderingState(videoRecorder);
        }

        //---------------------------------------------------------------------
        // Public
        //---------------------------------------------------------------------

        public void SubscribeToEvents()
        {
            _levelManager.LevelPreviewStarted += OnLevelPreviewStarted;
            _levelManager.LevelPiecePlayingCompleted += OnLevelPiecePlayingCompleted;
            _levelManager.NextLevelPiecePlayingStarted += OnNextLevelPiecePlayingStarted;
            _levelManager.LevelPreviewCompleted += OnLevelPreviewCompleted;

            _videoRecorder.VideoReady += OnVideoReady;
            _previewProgressCounter.PreviewElapsed += OnPreviewProgressChanged;
            _appEventsSource.ApplicationFocused += OnApplicationFocus;
            
            #if ENABLE_ONSCREEN_TUNING
            _appEventsSource.GUI += OnGUI;
            #endif
        }

        public void UnsubscribeFromEvents()
        {
            _levelManager.LevelPreviewStarted -= OnLevelPreviewStarted;
            _levelManager.LevelPiecePlayingCompleted -= OnLevelPiecePlayingCompleted;
            _levelManager.NextLevelPiecePlayingStarted -= OnNextLevelPiecePlayingStarted;
            _levelManager.LevelPreviewCompleted -= OnLevelPreviewCompleted;

            _videoRecorder.VideoReady -= OnVideoReady;
            _previewProgressCounter.PreviewElapsed -= OnPreviewProgressChanged;
            _appEventsSource.ApplicationFocused -= OnApplicationFocus;
            
            #if ENABLE_ONSCREEN_TUNING
            _appEventsSource.GUI -= OnGUI;
            #endif
        }
        
        public void RenderVideo(bool saveToGallery, bool isLandscapeMode, bool isUltraHD, Action onCompleted)
        {
            _saveToGallery = saveToGallery;
            _useLandscapeMode = isLandscapeMode;
            _useUltraHD = isUltraHD;
            _onCompleted = onCompleted;

            PortraitVideoFilePath = null;
            _renderedVideoCount = 0;
            _videoRenderingState.Reset();
            _previousStepsProgress = 0;
            _progressMultiplier = _useLandscapeMode
                ? .5f // Portrait = 50%, Landscape = 50%
                : 1f; // Portrait = 100%

            //agreed to set frame delays not per event, but only based on 1st as lon gas it does not cause any problem
            var isVoiceRecordingBasedLevel = _levelManager.CurrentLevel.GetFirstEvent().HasVoiceTrack();  
            _videoRecorder.AudioDelayFrames = isVoiceRecordingBasedLevel
                ? AppSettings.VoiceBasedEventAudioRenderingFramesDelay
                : AppSettings.LipSyncAudioRenderingFramesDelay;

            IsSavingInProgress = true;
            StartVideoRendering(false);
            _levelManager.LevelPiecePlayingCompleted += OnLevelPiecePlayingCompleted;
        }

        public void Cancel()
        {
            var camera = _levelManager.GetActiveCamera();
            if (camera)
            {
                camera.targetTexture = null;
            }

            _levelManager.CancelPreview(PreviewCleanMode.KeepFirstEvent);
            _levelManager.DeactivateAllAssets();

            _renderedVideoCount = 0;
            _previousStepsProgress = 0;

            _levelManager.CleanUp();

            _videoRecorder.CancelCapture();
            _videoRenderingState.IsCanceled = true;

            RecordingCancelled?.Invoke();

            OnRecordingStopped();
            IsSavingInProgress = false;
        }
        
        //---------------------------------------------------------------------
        // Events
        //---------------------------------------------------------------------
        
        private void OnApplicationFocus(bool hasFocus)
        {
            var enterBackgroundModeDuringRecording = IsSavingInProgress && !hasFocus;
            if (enterBackgroundModeDuringRecording)
            {
                //There is no way to continue or postpone video capturing in background mode on iOS, we can only cancel
                Cancel();
                _recordingInterruptedByBackgroundMode = true;
                return;
            }

            var appFocusingAfterCanceledRecording = hasFocus && _recordingInterruptedByBackgroundMode;
            if (appFocusingAfterCanceledRecording)
            {
                OnRefocus?.Invoke();
                _recordingInterruptedByBackgroundMode = false;
            }
        }
        
        private void OnLevelPreviewStarted()
        {
            if (_videoRecorder.IsCapturing) _videoRecorder.CancelCapture();
            
            if (!IsPortraitRenderingJustFinished)
            {
                StartPortraitVideoCapturing();
            }
            else
            {
                StartLandscapeVideoCapturing();
            }

            _previewProgressCounter.Run();
        }

        private void OnLevelPiecePlayingCompleted()
        {
            _videoRecorder.PauseCapture();
            GC.Collect();
            Resources.UnloadUnusedAssets();
        }
        
        private void OnNextLevelPiecePlayingStarted()
        {
            _videoRecorder.ResumeCapture();
        }

        private void OnLevelPreviewCompleted()
        {
            _videoRecorder.StopCapture();
        }

        private void OnVideoReady()
        {
            _renderedVideoCount++;
            if (IsPortraitRenderingJustFinished)
            {
                _previousStepsProgress = _videoRenderingState.Progress;
                PortraitVideoFilePath = _videoRecorder.FilePath;
            }

            if (_useLandscapeMode && !IsLandscapeRenderingJustFinished)
            {
                RenderLandscapeVideo();
                return;
            }

            FinishRendering();
        }

        //---------------------------------------------------------------------
        // Helpers
        //---------------------------------------------------------------------

        #if ENABLE_ONSCREEN_TUNING
        private void OnGUI()
        {
            int midFont = (int) ((Screen.height / 1334f * 3) * 8 * 0.7f);
            GUI.skin.label.fontSize = midFont;
            GUI.skin.button.fontSize = midFont;
            GUI.skin.textField.fontSize = midFont;

            int width = (int)(Screen.width * 0.20f);
            int y = 260;
            int height = (int)(Screen.height * 0.05f);
            int nextSpot = height + (int)(Screen.height * 0.003f);
            y += 9 * nextSpot;
            if(GUI.Button(new Rect(10,y,width,height), "Video +"))
            {
                _lipSyncAudioDelay= _lipSyncAudioDelay + 1;
            }
            y += nextSpot;
            if(GUI.Button(new Rect(10,y,width,height), "Video -"))
            {
                _lipSyncAudioDelay= _lipSyncAudioDelay - 1;
            }
            y += nextSpot;  
            GUI.contentColor = Color.black;
            GUI.Label(new Rect(10, y, Screen.width * 0.9f, Screen.height * 0.4f),"Video Delay: " + _lipSyncAudioDelay);
        }
        #endif
        
        private void FinishRendering()
        {
            CleanupResources();
            OnRecordingStopped();
            _videoRenderingState.IsFinished = true;

            IsSavingInProgress = false;

            _onCompleted?.Invoke();
        }
        
        private void StartVideoRendering(bool isLandscape)
        {
            _levelManager.PlayLevelPreviewForRendering(isLandscape);
        }

        private void StartPortraitVideoCapturing()
        {
            _videoRecorder.StartCapture(_saveToGallery, false, false);
        }

        private void StartLandscapeVideoCapturing()
        {
            _videoRecorder.StartCapture(true, true, _useUltraHD);
        }
        
        private void RenderLandscapeVideo()
        {
            if (!_useLandscapeMode) throw new InvalidOperationException("Landscape video was not requested");
            StartVideoRendering(true);
        }
        
        private void CleanupResources()
        {
            _levelManager.CleanUp();
            _levelManager.UnloadAllAssets();
            _bridge.DeleteTempFiles();
        }

        private void OnPreviewProgressChanged(float value)
        {
            var currentStepProgress = value * _progressMultiplier;
            var totalProgress = _previousStepsProgress + currentStepProgress;
            _videoRenderingState.SetProgress(totalProgress);
        }

        private void OnRecordingStopped()
        {
            _levelManager.LevelPiecePlayingCompleted -= OnLevelPiecePlayingCompleted;
        }
        
        private sealed class VideoRenderingState: IVideoRenderingState
        {
            private readonly IVideoRecorder _videoRecorder;
            
            public event Action<float> ProgressUpdated;
            public float Progress { get; private set; }
            public bool IsRendering => _videoRecorder.IsRendering;
            public bool IsFinished { get; set; }
            public bool IsCanceled { get; set; }
            
            public VideoRenderingState(IVideoRecorder videoRecorder)
            {
                _videoRecorder = videoRecorder;
            }

            public void SetProgress(float value)
            {
                Progress = value;
                ProgressUpdated?.Invoke(Progress);
            }

            public void Reset()
            {
                Progress = 0;
                IsFinished = false;
                IsCanceled = false;
            }
        }
    }
}