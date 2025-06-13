using System;
using Bridge.Models.Common.Files;
using Cinemachine;
using JetBrains.Annotations;
using Modules.CameraCapturing;
using Modules.CameraSystem.CameraSystemCore;
using Unity.Collections;
using UnityEngine;
using Zenject;
using Resolution = Bridge.Models.Common.Files.Resolution;

namespace Modules.LevelManaging.Editing.ThumbnailCreator
{
    [UsedImplicitly]
    public sealed class EventThumbnailsCreatorManager
    {
        private readonly Vector2Int _previewThumbnailSize = new Vector2Int(337, 530);
        private readonly Resolution _previewThumbnailResolution = Resolution._512x512;
        
        private readonly Vector2Int _timelineThumbnailSize = new Vector2Int(128, 128);
        private readonly Resolution _timelineThumbnailResolution = Resolution._128x128;

        private readonly EventTimelineThumbnailCreator _eventTimelineThumbnailCreator;
        private readonly EventPreviewThumbnailsCreator _eventPreviewThumbnailsCreator;
        
        [Inject] private ICameraSystem _cameraSystem;
        
        //---------------------------------------------------------------------
        // Events
        //---------------------------------------------------------------------
        
        public event Action ThumbnailsCaptured;

        //---------------------------------------------------------------------
        // Properties
        //---------------------------------------------------------------------
        
        public Texture2D TimelineThumbnail => _eventTimelineThumbnailCreator.CapturedThumbnail;
        
        //---------------------------------------------------------------------
        // Ctors
        //---------------------------------------------------------------------
        
        public EventThumbnailsCreatorManager()
        {
            _eventTimelineThumbnailCreator = new EventTimelineThumbnailCreator(_timelineThumbnailSize, _timelineThumbnailResolution);
            _eventPreviewThumbnailsCreator = new EventPreviewThumbnailsCreator(_previewThumbnailSize, _previewThumbnailResolution);
        }

        //---------------------------------------------------------------------
        // Public
        //---------------------------------------------------------------------

        public void CaptureThumbnails(string fileName, Camera screenshotCamera = null, Action<FileInfo[]> onComplete = null)
        {
            var thumbnailCamera = screenshotCamera != null ? screenshotCamera : Camera.main;
            if (thumbnailCamera == null)
            {
                throw new InvalidOperationException("Failed thumbnail capturing. Reason: There is no available camera");
            }
            
            var composer = _cameraSystem.CinemachineComposer;
            
            var screenshotResolution =  new Vector2Int(_previewThumbnailSize.x, (int)(_previewThumbnailSize.x / thumbnailCamera.aspect));
            CameraCapture.CaptureFromCamera(thumbnailCamera, screenshotResolution,
                                            pixelData => {
                                                OnReceivedDataFromGpu(pixelData, fileName, onComplete, composer,
                                                                      screenshotResolution);
                                            });
        }

        //---------------------------------------------------------------------
        // Helpers
        //---------------------------------------------------------------------

        private void OnReceivedDataFromGpu(NativeArray<uint> pixelData, string fileName, Action<FileInfo[]> onComplete, CinemachineComposer composer, Vector2Int screenshotResolution)
        {
            var thumbnails = CreateThumbnails(fileName, composer, pixelData, screenshotResolution);
            _eventPreviewThumbnailsCreator.DestroyCapturedTexture();
            onComplete?.Invoke(thumbnails);
            ThumbnailsCaptured?.Invoke();
        }
        
        private FileInfo[] CreateThumbnails(string fileName, CinemachineComposer composer, NativeArray<uint> originalPixels, Vector2Int screenshotResolution)
        {
            var previewThumbnail = _eventPreviewThumbnailsCreator.CreateThumbnail(fileName, new Vector2(composer.m_ScreenX, composer.m_ScreenY), originalPixels, screenshotResolution);
            var timelineThumbnail = _eventTimelineThumbnailCreator.CreateThumbnail(fileName, new Vector2(composer.m_ScreenX, composer.m_ScreenY), originalPixels, screenshotResolution);
            
            return new[] { previewThumbnail, timelineThumbnail };
        }
    }
}