using System;
using System.Collections;
using System.IO;
using Common;
using Modules.LevelManaging.Editing.LevelManagement;
using Modules.RenderingPipelineManagement;
using Settings;
using UIManaging.Pages.Common.NativeGalleryManagement;
using UnityEngine;
using Zenject;

namespace Modules.VideoRecording
{
    public abstract class VideoRecorderBase : MonoBehaviour, IVideoRecorder
    {
        [Inject] private ILevelManager _levelManager;
        [Inject] private IRenderingPipelineManager _renderingPipelineManager;
        [Inject] private INativeGallery _nativeGallery;

        private IEnumerator _fileWritingWatcher;

        //---------------------------------------------------------------------
        // Events
        //---------------------------------------------------------------------

        public event Action RecordingStarted;
        public event Action RecordingEnded;
        public event Action RecordingCanceled;
        public event Action VideoReady;

        //---------------------------------------------------------------------
        // Properties
        //---------------------------------------------------------------------

        public bool IsCapturing => IsRecording;
        public bool IsRendering => IsRecording && !IsPaused;
        public abstract string FilePath { get; }
        public abstract int AudioDelayFrames { get; set; }

        protected bool IsRecording { get; private set; }
        protected bool IsPaused { get; private set; }
        protected VideoRecorderSettings Settings { get; private set; }

        //---------------------------------------------------------------------
        // Messages
        //---------------------------------------------------------------------

        protected virtual void OnDestroy()
        {
            if (_fileWritingWatcher != null)
            {
                StopCoroutine(_fileWritingWatcher);
                _fileWritingWatcher = null;
            }
        }

        //---------------------------------------------------------------------
        // Public
        //---------------------------------------------------------------------

        public virtual void StartCapture(bool saveToGallery, bool isLandscapeMode, bool isUltraHD)
        {
            if (IsRecording) return;
            
            Settings = new VideoRecorderSettings(isLandscapeMode, isUltraHD, saveToGallery);
            
            if (_fileWritingWatcher != null)
            {
                StopCoroutine(_fileWritingWatcher);
                _fileWritingWatcher = null;
            }

            SetupRecorder();
            
            _renderingPipelineManager.SetHighQualityPipeline();

            IsPaused = false;
            IsRecording = true;

            StartRecording();

            RecordingStarted?.Invoke();
        }

        public virtual void PauseCapture()
        {
            if (IsPaused) return;

            IsPaused = true;

            PauseRecording();
        }

        public void ResumeCapture()
        {
            if (!IsPaused) return;

            IsPaused = false;

            ResumeRecording();
        }

        public virtual void StopCapture()
        {
            if (!IsRecording) return;

            IsRecording = false;

            StopRecording();

            if (AppSettings.UseOptimizedRenderingScale)
            {
                _renderingPipelineManager.SetDefaultPipeline();
            }

            RecordingEnded?.Invoke();

            if (_fileWritingWatcher != null)
            {
                StopCoroutine(_fileWritingWatcher);
                _fileWritingWatcher = null;
            }

            StartCoroutine(FileWritingWatcher());
        }

        public virtual void CancelCapture()
        {
            if (!IsRecording) return;

            IsRecording = false;
            CancelRecording();

            if (AppSettings.UseOptimizedRenderingScale)
            {
                _renderingPipelineManager.SetDefaultPipeline();
            }

            RecordingCanceled?.Invoke();
        }

        protected abstract void SetupRecorder();
        protected abstract void StartRecording();
        protected abstract void PauseRecording();
        protected abstract void ResumeRecording();
        protected abstract void StopRecording();
        protected abstract void CancelRecording();
        protected abstract bool IsFileWritingFinished();

        //---------------------------------------------------------------------
        // Helpers 
        //---------------------------------------------------------------------

        private IEnumerator FileWritingWatcher()
        {
            yield return new WaitUntil(IsFileWritingFinished);

            // Wait until writing handler will close the file
            yield return new WaitForEndOfFrame();

            if (Settings.SaveToGallery)
            {
                SaveVideoToGallery();
            }

            VideoReady?.Invoke();
        }

        private void SaveVideoToGallery()
        {
            var fileName = Path.GetFileName(FilePath);

            _nativeGallery.SaveVideoToGallery(FilePath, Constants.FileDefaultPaths.RECORDED_VIDEOS_FOLDER, fileName);
        }
    }
}