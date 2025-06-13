using System;
using System.Collections;
using Bridge.Models.Common.Files;
using Common;
using Extensions;
using JetBrains.Annotations;
using Modules.LevelManaging.Editing.LevelManagement;
using Modules.LevelManaging.Editing.ThumbnailCreator;
using UnityEngine;
using Event = Models.Event;

namespace UIManaging.Pages.LevelEditor.Ui.PostRecordEditor
{
    [UsedImplicitly]
    internal sealed class EventThumbnailCapture
    {
        private readonly EventThumbnailsCreatorManager _eventThumbnailsCreatorManager;
        private readonly ILevelManager _levelManager;

        private Event TargetEvent => _levelManager.TargetEvent;
        
        public EventThumbnailCapture(EventThumbnailsCreatorManager eventThumbnailsCreatorManager, ILevelManager levelManager)
        {
            _eventThumbnailsCreatorManager = eventThumbnailsCreatorManager;
            _levelManager = levelManager;
        }
        
        public void RefreshTargetEventThumbnail(Action<Event> onThumbnailTaken = null)
        {
            if (TargetEvent.HasActualThumbnail) return;
            CoroutineSource.Instance.StartCoroutine(RetakeThumbnail(onThumbnailTaken));
        }
        
        public void RefreshThumbnailsDuringNextLevelPreview(Action<Event> onThumbnailTaken = null)
        {
            _levelManager.LevelPreviewCompleted += StopCapturingThumbnails;
            _levelManager.PlayingEventSwitched += OnPlayingEventSwitched;
            _levelManager.PreviewCancelled += StopCapturingThumbnails;

            void StopCapturingThumbnails()
            {
                _levelManager.PlayingEventSwitched -= OnPlayingEventSwitched;
                _levelManager.LevelPreviewCompleted -= StopCapturingThumbnails;
                _levelManager.PreviewCancelled -= StopCapturingThumbnails;
            }
            
            void OnPlayingEventSwitched()
            {
                RefreshTargetEventThumbnail(onThumbnailTaken);
            }
        }
        
        private IEnumerator RetakeThumbnail(Action<Event> onThumbnailTaken = null)
        {
            yield return new WaitForEndOfFrame();

            var targetEvent = TargetEvent;
            var camera = _levelManager.GetActiveCamera();
            _eventThumbnailsCreatorManager.CaptureThumbnails(targetEvent.Id.ToString(), camera, files=> OnThumbnailCreated(files, targetEvent, onThumbnailTaken));
        }
        
        private void OnThumbnailCreated(FileInfo[] thumbnails, Event targetEvent, Action<Event> onThumbnailTaken)
        {
            if (targetEvent == null) return;
            
            targetEvent.SetFiles(thumbnails);
            targetEvent.HasActualThumbnail = true;
            onThumbnailTaken?.Invoke(targetEvent);
        }
    }
}