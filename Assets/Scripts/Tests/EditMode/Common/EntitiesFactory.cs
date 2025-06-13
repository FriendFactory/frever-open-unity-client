using System.Collections.Generic;
using Bridge.Models.ClientServer.Assets;
using Bridge.Models.ClientServer.Level.Full;
using Models;
using CharacterController = Models.CharacterController;
using Event = Models.Event;

namespace Tests.EditMode
{
    public class EntitiesFactory
    {
        private readonly IIdGenerator _generator;

        public EntitiesFactory(IIdGenerator generator)
        {
            _generator = generator;
        }

        public Level GetLevel()
        {
            var level = new Level() { Id = GetId() };
            var @event = GetEvent();

            @event.LevelId = level.Id;

            level.Event.Add(@event);

            return level;
        }

        public Event GetEvent()
        {
            var @event = new Event() { Id = GetId() };
            var characterController = GetCharacterController();
            var cameraController = GetCameraController();
            var setLocationController = GetSetLocationController();
            var cameraFilterController = GetCameraFilterController();
            var vfxController = GetVfxController();
            var musicController = GetMusicController();
            var caption = GetCaption();

            cameraController.EventId = @event.Id;
            characterController.EventId = @event.Id;
            setLocationController.EventId = @event.Id;
            vfxController.EventId = @event.Id;
            musicController.EventId = @event.Id;

            @event.CharacterController.Add(characterController);
            @event.CameraController.Add(cameraController);
            @event.SetLocationController.Add(setLocationController);
            @event.CameraFilterController.Add(cameraFilterController);
            @event.MusicController.Add(musicController);
            @event.Caption.Add(caption);

            return @event;
        }

        public CharacterController GetCharacterController()
        {
            var characterController = new CharacterController() { Id = GetId() };
            var faceAnimation = new FaceAnimationFullInfo() { Id = GetId() };
            var voiceTrack = new VoiceTrackFullInfo() { Id = GetId() };
            var faceVoice = new CharacterControllerFaceVoice()
            {
                Id = GetId(),
                CharacterControllerId = characterController.Id,
                FaceAnimationId = faceAnimation.Id,
                VoiceTrackId = voiceTrack.Id,
                FaceAnimation = faceAnimation,
                VoiceTrack = voiceTrack,
            };
            var bodyAnimationInfo = new BodyAnimationInfo() { Id = GetId() };
            var bodyAnimation = new CharacterControllerBodyAnimation()
            {
                Id = GetId(),
                CharacterControllerId = characterController.Id,
                BodyAnimationId = bodyAnimationInfo.Id,
                BodyAnimation = bodyAnimationInfo,
            };
            characterController.CharacterControllerFaceVoice.Add(faceVoice);
            characterController.CharacterControllerBodyAnimation.Add(bodyAnimation);

            return characterController;
        }

        public SetLocationController GetSetLocationController()
        {
            var videoClip = new VideoClipFullInfo() { Id = GetId() };
            var photo = new PhotoFullInfo() { Id = GetId() };
            var setLocationController = new SetLocationController()
            {
                Id = GetId(),
                PhotoId = photo.Id,
                VideoClipId = videoClip.Id,
                Photo = photo,
                VideoClip = videoClip,
            };

            return setLocationController;
        }

        public CameraController GetCameraController()
        {
            var cameraAnimation = new CameraAnimationFullInfo() { Id = GetId() };
            var cameraController = new CameraController()
            {
                Id = GetId(),
                CameraAnimation = cameraAnimation,
                CameraAnimationId = cameraAnimation.Id
            };

            return cameraController;
        }

        public CameraFilterController GetCameraFilterController()
        {
            var cameraFilterVariant = new CameraFilterVariantInfo() { Id = GetId() };
            var cameraFilter = new CameraFilterInfo()
            {
                Id = GetId(),
                CameraFilterVariants = new List<CameraFilterVariantInfo>() { cameraFilterVariant },
            };
            var controller = new CameraFilterController()
            {
                Id = GetId(),
                CameraFilter = cameraFilter,
                CameraFilterVariantId = cameraFilterVariant.Id,
            };

            return controller;
        }

        public MusicController GetMusicController()
        {
            var userSound = new UserSoundFullInfo() { Id = GetId() };
            var controller = new MusicController()
            {
                Id = GetId(),
                UserSound = userSound,
                UserSoundId = userSound.Id,
            };

            return controller;
        }

        public VfxController GetVfxController()
        {
            var vfx = new VfxInfo() { Id = GetId() };
            var controller = new VfxController()
            {
                Id = GetId(),
                Vfx = vfx,
                VfxId = vfx.Id,
            };

            return controller;
        }

        public CaptionFullInfo GetCaption()
        {
            var caption = new CaptionFullInfo() { Id = GetId() };

            return caption;
        }

        private long GetId() => _generator.Next();
    }
}