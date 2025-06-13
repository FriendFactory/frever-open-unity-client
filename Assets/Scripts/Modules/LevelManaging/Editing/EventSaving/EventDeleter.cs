using System;
using System.Collections.Generic;
using System.Linq;
using Bridge.Models.Common;
using Extensions;
using Modules.Amplitude;
using Models;
using Modules.AssetsManaging;
using Modules.LevelManaging.Editing.Players;
using UnityEngine;
using CharacterController = Models.CharacterController;
using Event = Models.Event;
using PlayMode = Modules.LevelManaging.Editing.Players.EventPlaying.PlayMode;

namespace Modules.LevelManaging.Editing.EventSaving
{
    internal sealed class EventDeleter
    {
        private readonly IPreviewManager _previewManager;
        private readonly IAssetManager _assetManager;
        private readonly AmplitudeManager _amplitudeManager;
        
        private bool _levelIsEmpty;
        

        //---------------------------------------------------------------------
        // Ctors
        //---------------------------------------------------------------------

        public EventDeleter(IPreviewManager previewManager, IAssetManager assetManager, AmplitudeManager amplitudeManager)
        {
            _previewManager = previewManager;
            _assetManager = assetManager;
            _amplitudeManager = amplitudeManager;
        }

        //---------------------------------------------------------------------
        // Public
        //---------------------------------------------------------------------

        public Event DeleteLastEvent(ICollection<Event> events)
        {
            var eventToRemove = events.OrderBy(x => x.LevelSequence).Last();
            
            var previousEvent = DeleteEventByIdInternal(events, eventToRemove.Id);
            previousEvent.LevelSequence = 0;
            return previousEvent;
        }

        public void DeleteEvent(long eventId, Level currentLevel)
        {
            var eventModel = currentLevel.GetEvent(eventId);

            if (eventModel == null)
            {
                throw new InvalidOperationException($"Failed deletion of event {eventId}. Reason: Level does not have this event");
            }
            currentLevel.Event.Remove(eventModel);

            for (var i = 0; i < currentLevel.Event.Count; i++)
            {
                currentLevel.Event.ElementAt(i).LevelSequence = i + 1;
            }

            DeleteEventAmplitudeEvent(eventId, eventModel.LevelSequence);
        }
        
        //---------------------------------------------------------------------
        // Helpers
        //---------------------------------------------------------------------
        
        private Event DeleteEventByIdInternal(ICollection<Event> events, long eventId)
        {
            var removedEvent = events.FirstOrDefault(x => x.Id == eventId);
            var indexOfRemovedEvent = events.ToList().IndexOf(removedEvent);
            events.Remove(removedEvent);
            
            DeleteEventAmplitudeEvent(eventId, removedEvent.LevelSequence);

            var newTargetEventIndex = 0;

            if(events.Count > 0)
            {
                newTargetEventIndex = Mathf.Clamp(indexOfRemovedEvent, 0, events.Count - 1);
            }
            
            ResetControllerIds(removedEvent);

            if (_previewManager.PlayMode == PlayMode.PreRecording)
            {
                return removedEvent;
            }

            var lastEventAfterDeleting = events.Count > 0 ? events.ElementAt(newTargetEventIndex) : removedEvent;
            return lastEventAfterDeleting;
        }

        private static void ResetControllerIds(Event @event)
        {
            ResetIds(@event.SetLocationController);
            ResetIds(@event.CharacterController);
            ResetIds(@event.VfxController);
            ResetIds(@event.CameraController);
            ResetIds(@event.MusicController);
            ResetIds(@event.CameraFilterController);
            ResetCharacterControllerFaceVoice(@event.CharacterController);
        }

        private static void ResetIds<T>(ICollection<T> controller) where T : IEntity
        {
            for (var i = 0; i < controller.Count; i++)
            {
                controller.ElementAt(i).Id = 0;
            }
        }

        private static void ResetCharacterControllerFaceVoice(ICollection<CharacterController> controller)
        {
            for (var i = 0; i < controller.Count; i++)
            {
                var characterControllerFaceVoice =
                    controller.ElementAt(i).CharacterControllerFaceVoice.FirstOrDefault();
                
                if (characterControllerFaceVoice == null) continue;
                
                characterControllerFaceVoice.Id = 0;
                characterControllerFaceVoice.FaceAnimation = null;
                characterControllerFaceVoice.FaceAnimationId = null;
                characterControllerFaceVoice.VoiceTrack = null;
                characterControllerFaceVoice.VoiceTrackId = null;
            }
        }
        
        private void DeleteEventAmplitudeEvent(long eventId, int levelSequence)
        {
            var deleteEventMetaData = new Dictionary<string, object>
            {
                [AmplitudeEventConstants.EventProperties.EVENT_ID] = eventId,
                [AmplitudeEventConstants.EventProperties.LEVEL_SEQUENCE] = levelSequence
            };
            _amplitudeManager.LogEventWithEventProperties(AmplitudeEventConstants.EventNames.DELETE_EVENT, deleteEventMetaData);
        }
    }
}