using System.Linq;
using Extensions;
using Models;
using Modules.LocalStorage;
using NUnit.Framework;

namespace Tests.EditMode
{
    [TestFixture]
    internal sealed class EntityReplacingIdsTests
    {
        private EntitiesFactory _factory;

        [SetUp]
        public void Init()
        {
            _factory = new EntitiesFactory(new DefaultIdGenerator());
        }

        [Test]
        public void ReplaceLevelIds()
        {
            var level = _factory.GetLevel();
            var targetId = -1;
            long ProvideId(string x) => targetId;

            level.ReplaceEmptyIds(ProvideId);

            ValidateLevelIds(level, targetId);
        }

        [Test]
        public void ResetLevelLocalIds()
        {
            var level = _factory.GetLevel();

            level.ReplaceEmptyIds(LocalStorageManager.GetNextLocalId);
            level.ResetLocalIds();
            
            ValidateLevelIds(level, 0);
        }

        private void ValidateLevelIds(Level level, long expectedId)
        {
            var @event = level.Event.First();
            var cameraController = @event.GetCameraController();
            var cameraAnimation = cameraController.CameraAnimation;
            var cameraFilterController = @event.GetCameraFilterController();
            var characterController = @event.GetCharacterController(0);
            var faceVoice = characterController.GetCharacterControllerFaceVoiceController();
            var faceAnimation = faceVoice.FaceAnimation;
            var voiceTrack = faceVoice.VoiceTrack;
            var bodyAnimation = characterController.GetBodyAnimationController();
            var musicController = @event.GetMusicController();
            var setLocationController = @event.GetSetLocationController();
            var videoClip = setLocationController.VideoClip;
            var photo = setLocationController.Photo;
            var caption = @event.GetCaption(expectedId);

            Assert.True(level.Id == expectedId);
            Assert.True(@event.Id == expectedId);
            Assert.True(cameraController.Id == expectedId);
            Assert.True(cameraAnimation.Id == expectedId);
            Assert.True(cameraController.CameraAnimationId  == cameraAnimation.Id);
            Assert.True(cameraFilterController.Id == expectedId);
            Assert.True(characterController.Id == expectedId);
            Assert.True(faceVoice.Id == expectedId);
            Assert.True(faceAnimation.Id == expectedId);
            Assert.True(voiceTrack.Id == expectedId);
            Assert.True(faceVoice.FaceAnimationId == faceAnimation.Id);
            Assert.True(faceVoice.VoiceTrackId == voiceTrack.Id);
            Assert.True(bodyAnimation.Id == expectedId);
            Assert.True(musicController.Id == expectedId);
            Assert.True(setLocationController.Id == expectedId);
            Assert.True(videoClip.Id == expectedId);
            Assert.True(photo.Id == expectedId);
            Assert.True(setLocationController.VideoClipId == videoClip.Id);
            // reflection based replacing algorithm fails here,
            // but this type is included in list of child types that must be replaced
            Assert.True(setLocationController.PhotoId == photo.Id);
            Assert.True(caption.Id == expectedId);
        }
    }
}