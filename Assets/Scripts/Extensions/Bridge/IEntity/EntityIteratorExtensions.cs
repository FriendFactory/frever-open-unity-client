using System;
using Bridge.Models.Common;
using Models;
using CharacterController = Models.CharacterController;
using Event = Models.Event;

namespace Extensions
{
    internal static class EntityIteratorExtensions
    {
        internal static void ReplaceIds(this Level level, Func<long, bool> condition, Func<string, long> generator)
        {
            level.ReplaceIds<Level>(condition, generator);

            level.Event.ForEach(@event => @event.ReplaceIds(condition, generator));
        }

        private static void ReplaceIds(this Event @event, Func<long, bool> condition, Func<string, long> generator)
        {
            @event.ReplaceIds<Event>(condition, generator);
            
            @event.CameraController.ForEach(cameraController => cameraController.ReplaceIds(condition, generator));
            @event.CameraFilterController.ForEach(cameraFilterController =>
                                                       cameraFilterController.ReplaceIds(condition, generator));
            @event.CharacterController.ForEach(characterController => characterController.ReplaceIds(condition, generator));
            @event.SetLocationController.ForEach(setLocationController => setLocationController.ReplaceIds(condition, generator));
            @event.VfxController.ForEach(vfxController => vfxController.ReplaceIds(condition, generator));
            @event.MusicController.ForEach(musicController => musicController.ReplaceIds(condition, generator));
            @event.Caption.ForEach(caption => caption.ReplaceIds(condition, generator));
        }

        private static void ReplaceIds(this CameraController controller, Func<long, bool> condition,
                                       Func<string, long> generator)
        {
            controller.ReplaceIds<CameraController>(condition, generator);
            controller.CameraAnimation.ReplaceIds(condition, generator);
            
            controller.CameraAnimationId = controller.CameraAnimation.Id;
        }

        private static void ReplaceIds(this CharacterController controller, Func<long, bool> condition,
                                       Func<string, long> generator)
        {
            controller.ReplaceIds<CharacterController>(condition, generator);
            controller.CharacterControllerBodyAnimation.ForEach(bodyAnimation =>
                                                                    bodyAnimation.ReplaceIds(condition, generator));
            controller.CharacterControllerFaceVoice.ForEach(faceVoice =>
            {
                faceVoice.ReplaceIds(condition, generator);
            });
        }

        private static void ReplaceIds(this CharacterControllerBodyAnimation bodyAnimation, Func<long, bool> condition,
                                       Func<string, long> generator)
        {
            bodyAnimation.ReplaceIds<CharacterControllerBodyAnimation>(condition, generator);
            bodyAnimation.BodyAnimation.ReplaceIds(condition, generator);

            bodyAnimation.BodyAnimationId = bodyAnimation.BodyAnimation.Id;
        }

        private static void ReplaceIds(this CharacterControllerFaceVoice faceVoice, Func<long, bool> condition,
                                       Func<string, long> generator)
        {
            faceVoice.ReplaceIds<CharacterControllerFaceVoice>(condition, generator);
            faceVoice.FaceAnimation.ReplaceIds(condition, generator);
            faceVoice.VoiceFilter.ReplaceIds(condition, generator);
            faceVoice.VoiceTrack.ReplaceIds(condition, generator);
            
            faceVoice.FaceAnimationId = faceVoice.FaceAnimation?.Id;
            faceVoice.VoiceFilterId = faceVoice.VoiceFilter?.Id;
            faceVoice.VoiceTrackId = faceVoice.VoiceTrack?.Id;
        }

        private static void ReplaceIds(this SetLocationController controller, Func<long, bool> condition,
                                       Func<string, long> generator)
        {
            controller.ReplaceIds<SetLocationController>(condition, generator);
            controller.VideoClip.ReplaceIds(condition, generator);
            controller.Photo.ReplaceIds(condition, generator);

            controller.VideoClipId = controller.VideoClip?.Id;
            controller.PhotoId = controller.Photo?.Id;
        }

        private static void ReplaceIds<T>(this T entity, Func<long, bool> condition,
                                          Func<string, long> generator) where T: IEntity
        {
            if (entity == null || !condition.Invoke(entity.Id)) return;
          
            entity.Id = GetEntityId(entity, generator);
        }
        
        private static long GetEntityId(IEntity entity, Func<string, long> generator)
        {
            var typeName = entity.GetType().Name;

            return generator.Invoke(typeName);
        }
    }
}