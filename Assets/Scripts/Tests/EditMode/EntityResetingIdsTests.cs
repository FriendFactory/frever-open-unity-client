using System.Linq;
using Extensions;
using Extensions.ResetEntity;
using NUnit.Framework;

namespace Tests.EditMode
{
    internal sealed class EntityResetingIdsTests
    {
        [Test]
        public void ResetLevel()
        {
            var factory = new EntitiesFactory(new RandomIdGenerator());
            var level = factory.GetLevel();
            
            //Act
            level.ResetIds();
            
            //Assert
            var targetEvent = level.Event.First();
            var characterController = targetEvent.GetFirstCharacterController();
            var characterControllerFaceVoice = characterController.GetCharacterControllerFaceVoiceController();
            
            Assert.True(level.Id == 0);
            Assert.True(targetEvent.Id == 0);
            Assert.True(characterController.Id == 0);
            Assert.True(targetEvent.LevelId == 0);
            Assert.True(characterController.EventId == 0);
            Assert.True(characterControllerFaceVoice.FaceAnimation.Id == 0);
            Assert.True(characterControllerFaceVoice.FaceAnimationId == null);
            Assert.True(characterControllerFaceVoice.CharacterControllerId == 0);
        }
    }
}
