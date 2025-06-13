using System.Collections.Generic;
using System.Linq;
using Bridge.Models.ClientServer.Assets;
using Extensions;
using Models;
using Modules.AssetsManaging;
using Modules.LevelManaging.Assets;
using Modules.LevelManaging.Editing;
using Modules.LevelManaging.Editing.EventRecording.AssetCueManaging;
using Modules.LevelManaging.Editing.EventRecording.AssetCueManaging.CueProviders;
using Modules.LevelManaging.Editing.Templates;
using Moq;
using NUnit.Framework;

namespace Tests.EditMode.ActivationCueTests
{
    internal sealed class BodyAnimationCueTests: CueTestBase
    {
        [Test]
        public void SetActivationCue_WithOutTemplateAndPreviousEvent_ShouldUseAssetPlaybackTime()
        {
            //arrange
            const int playbackTimeSec = 5;
            
            var bodyAnimation = new BodyAnimationInfo
            {
                Id = 1
            };

            var ev = new Event();
            ev.CharacterController = new List<CharacterController>();
            var characterController = new CharacterController();
            var bodyAnimController = new CharacterControllerBodyAnimation();
            bodyAnimController.SetBodyAnimation(bodyAnimation);
            characterController.SetBodyAnimationController(bodyAnimController);
            characterController.CharacterId = 10;
            ev.CharacterController.Add(characterController);

            var templatesProviderMock = new Mock<ITemplateProvider>();
            var characterAssetMock = new Mock<ICharacterAsset>();
            characterAssetMock.Setup(x => x.Id).Returns(characterController.CharacterId);
            characterAssetMock.Setup(x => x.PlaybackTime).Returns(playbackTimeSec);
            var assetManagerMock = new Mock<IAssetManager>();
            assetManagerMock.Setup(x => x.GetActiveAssets<ICharacterAsset>()).Returns(new []{characterAssetMock.Object});
            var context = GetContextMockWithSetupLevel();
            var eventTemplateManager = new Mock<IEventTemplateManager>();
            var cueProvider = new BodyAnimationCueProvider(context, eventTemplateManager.Object, assetManagerMock.Object, templatesProviderMock.Object);

            //act
            var activationCue = cueProvider.GetActivationCue(ev, characterController);

            //assert
            var expectedResult = playbackTimeSec.ToMilliseconds();
            Assert.AreEqual(expectedResult, activationCue);
        }
        
        [Test]
        public void SetActivationCue_WithTemplateButWithoutPreviousEvent_ShouldUseTemplateActivationCue()
        {
            //arrange
            const int templateId = 1;
            var bodyAnimation = new BodyAnimationInfo
            {
                Id = 2
            };

            var templateEvent = new Event();
            var templateBodyAnimController = new CharacterControllerBodyAnimation();
            templateBodyAnimController.SetBodyAnimation(bodyAnimation);
            templateBodyAnimController.ActivationCue = 100;
            var templateCharacterController = new CharacterController();
            templateCharacterController.ControllerSequenceNumber = 1;
            templateCharacterController.SetBodyAnimationController(templateBodyAnimController);
            templateEvent.CharacterController = new List<CharacterController>();
            templateEvent.CharacterController.Add(templateCharacterController);

            var ev = new Event();
            ev.TemplateId = templateId;
            ev.CharacterController = new List<CharacterController>();
            var characterController = new CharacterController();
            characterController.ControllerSequenceNumber = templateCharacterController.ControllerSequenceNumber;
            var bodyAnimController = new CharacterControllerBodyAnimation();
            bodyAnimController.SetBodyAnimation(bodyAnimation);
            characterController.SetBodyAnimationController(bodyAnimController);
            characterController.CharacterId = 10;
            ev.CharacterController.Add(characterController);

            var templatesProviderMock = new Mock<ITemplateProvider>();
            templatesProviderMock.Setup(x => x.GetTemplateEventFromCache(templateId)).Returns(templateEvent);
            var assetManagerMock = new Mock<IAssetManager>();
            var context = GetContextMockWithSetupLevel();
            var eventTemplateManager = new Mock<IEventTemplateManager>();
            var cueProvider = new BodyAnimationCueProvider(context, eventTemplateManager.Object, assetManagerMock.Object, templatesProviderMock.Object);

            //act
            var activationCue = cueProvider.GetActivationCue(ev, characterController);

            //assert
    
            Assert.AreEqual(templateBodyAnimController.ActivationCue, activationCue);
        }
        
        [Test]
        public void SetActivationCue_WithTemplateAndFewCharacterButWithoutPreviousEvent_ShouldUseTemplateActivationCuePerCharacterController()
        {
            //arrange
            const int templateId = 1;
            const int charactersCount = 3;
            var bodyAnimation = new BodyAnimationInfo
            {
                Id = 2
            };

            var templateEvent = new Event();
            templateEvent.CharacterController = new List<CharacterController>();
            for (int i = 0; i < charactersCount; i++)
            {
                var templateBodyAnimController = new CharacterControllerBodyAnimation();
                templateBodyAnimController.SetBodyAnimation(bodyAnimation);
                templateBodyAnimController.ActivationCue = i * 100;
                
                var templateCharacterController = new CharacterController();
                templateCharacterController.ControllerSequenceNumber = i;
                templateCharacterController.CharacterId = i * 1000;
                templateCharacterController.SetBodyAnimationController(templateBodyAnimController);
                templateEvent.CharacterController.Add(templateCharacterController);
            }
            
            var ev = new Event();
            ev.TemplateId = templateId;
            ev.CharacterController = new List<CharacterController>();
            for (int i = 0; i < charactersCount; i++)
            {
                var characterController = new CharacterController();
                characterController.ControllerSequenceNumber = templateEvent.CharacterController.ElementAt(i).ControllerSequenceNumber;
                var bodyAnimController = new CharacterControllerBodyAnimation();
                bodyAnimController.SetBodyAnimation(bodyAnimation);
                characterController.SetBodyAnimationController(bodyAnimController);
                characterController.CharacterId = i * 10;
                ev.CharacterController.Add(characterController);
            }
            
            var templatesProviderMock = new Mock<ITemplateProvider>();
            templatesProviderMock.Setup(x => x.GetTemplateEventFromCache(templateId)).Returns(templateEvent);
            var assetManagerMock = new Mock<IAssetManager>();
            var context = GetContextMockWithSetupLevel();
            var eventTemplateManager = new Mock<IEventTemplateManager>();
            var cueProvider = new BodyAnimationCueProvider(context, eventTemplateManager.Object, assetManagerMock.Object, templatesProviderMock.Object);
            var eventEditorMock = new Mock<IEventEditor>();
            eventEditorMock.Setup(x => x.UseSameBodyAnimation).Returns(false);
            var bodyAnimCueManager = new BodyAnimationCueManager(eventEditorMock.Object, cueProvider);

            //act
            bodyAnimCueManager.SetupActivationCues(ev);

            //assert
            for (var i = 0; i < charactersCount; i++)
            {
                var recordingCharacterController = ev.GetCharacterController(i);
                var templateCharacterController = templateEvent.GetCharacterController(i);
                var recordingActivationCue = recordingCharacterController.GetBodyAnimationController().ActivationCue;
                var templateActivationCue = templateCharacterController.GetBodyAnimationController().ActivationCue;
                Assert.AreEqual(recordingActivationCue, templateActivationCue);
            }
        }
        
        [Test]
        public void SetActivationCue_WithTemplateAndFewCharacterAndWithForceUsingSameBodyAnimation_EachCharacterShouldHaveSameActivationCue()
        {
            //arrange
            const int templateId = 1;
            const int charactersCount = 3;
            var bodyAnimation = new BodyAnimationInfo
            {
                Id = 2
            };

            var templateEvent = new Event();
            templateEvent.CharacterController = new List<CharacterController>();
            for (int i = 0; i < charactersCount; i++)
            {
                var templateBodyAnimController = new CharacterControllerBodyAnimation();
                templateBodyAnimController.SetBodyAnimation(bodyAnimation);
                templateBodyAnimController.ActivationCue = i * 100;
                
                var templateCharacterController = new CharacterController();
                templateCharacterController.ControllerSequenceNumber = i;
                templateCharacterController.CharacterId = i * 1000;
                templateCharacterController.SetBodyAnimationController(templateBodyAnimController);
                templateEvent.CharacterController.Add(templateCharacterController);
            }
            
            var ev = new Event();
            ev.TemplateId = templateId;
            ev.CharacterController = new List<CharacterController>();
            for (int i = 0; i < charactersCount; i++)
            {
                var characterController = new CharacterController();
                characterController.ControllerSequenceNumber = templateEvent.CharacterController.ElementAt(i).ControllerSequenceNumber;
                var bodyAnimController = new CharacterControllerBodyAnimation();
                bodyAnimController.SetBodyAnimation(bodyAnimation);
                characterController.SetBodyAnimationController(bodyAnimController);
                characterController.CharacterId = i * 10;
                ev.CharacterController.Add(characterController);
            }
            
            var templatesProviderMock = new Mock<ITemplateProvider>();
            templatesProviderMock.Setup(x => x.GetTemplateEventFromCache(templateId)).Returns(templateEvent);
            var assetManagerMock = new Mock<IAssetManager>();
            var context = GetContextMockWithSetupLevel();
            var eventTemplateManager = new Mock<IEventTemplateManager>();
            var cueProvider = new BodyAnimationCueProvider(context, eventTemplateManager.Object, assetManagerMock.Object, templatesProviderMock.Object);
            var eventEditorMock = new Mock<IEventEditor>();
            eventEditorMock.Setup(x => x.UseSameBodyAnimation).Returns(true);
            var bodyAnimCueManager = new BodyAnimationCueManager(eventEditorMock.Object, cueProvider);

            //act
            bodyAnimCueManager.SetupActivationCues(ev);

            //assert
            var allBodyControllers = ev.GetCharacterBodyAnimationControllers();
            Assert.IsTrue(allBodyControllers.Select(x=>x.ActivationCue).Distinct().Count() == 1);
        }
        
         [Test]
        public void SetActivationCue_WithTemplateAndFewCharacterButWithoutPreviousEvent_ShouldUseTemplateActivationCuePerCharacterControllerExpectCharacterWithChangedAsset()
        {
            //arrange
            const int templateId = 1;
            const int charactersCount = 3;
            const int characterWithChangedBodyAnimNumber = 1;
            const int animPlayBackTime = 5;
            var templateBodyAnimation = new BodyAnimationInfo
            {
                Id = 2
            };
            var newBodyAnim = new BodyAnimationInfo
            {
                Id = templateBodyAnimation.Id + 1
            };

            var templateEvent = new Event();
            templateEvent.CharacterController = new List<CharacterController>();
            for (int i = 0; i < charactersCount; i++)
            {
                var templateBodyAnimController = new CharacterControllerBodyAnimation();
                templateBodyAnimController.SetBodyAnimation(templateBodyAnimation);
                templateBodyAnimController.ActivationCue = i * 100;
                
                var templateCharacterController = new CharacterController();
                templateCharacterController.ControllerSequenceNumber = i;
                templateCharacterController.CharacterId = i * 1000;
                templateCharacterController.SetBodyAnimationController(templateBodyAnimController);
                templateEvent.CharacterController.Add(templateCharacterController);
            }
            
            var ev = new Event();
            ev.TemplateId = templateId;
            ev.CharacterController = new List<CharacterController>();
            for (int i = 0; i < charactersCount; i++)
            {
                var characterController = new CharacterController();
                characterController.ControllerSequenceNumber = templateEvent.CharacterController.ElementAt(i).ControllerSequenceNumber;
                var bodyAnimController = new CharacterControllerBodyAnimation();

                var bodyAnim = characterWithChangedBodyAnimNumber == i ? newBodyAnim : templateBodyAnimation; 
                bodyAnimController.SetBodyAnimation(bodyAnim);
                characterController.SetBodyAnimationController(bodyAnimController);
                characterController.CharacterId = i * 10;
                ev.CharacterController.Add(characterController);
            }
            
            var characterWithNewBodyAnimId = ev.GetCharacterController(characterWithChangedBodyAnimNumber).CharacterId;
            
            var templatesProviderMock = new Mock<ITemplateProvider>();
            templatesProviderMock.Setup(x => x.GetTemplateEventFromCache(templateId)).Returns(templateEvent);
            var characterMock = new Mock<ICharacterAsset>();
            characterMock.Setup(x => x.Id).Returns(characterWithNewBodyAnimId);
            characterMock.Setup(x => x.PlaybackTime).Returns(animPlayBackTime);
            var assetManagerMock = new Mock<IAssetManager>();
            assetManagerMock.Setup(x => x.GetActiveAssets<ICharacterAsset>()).Returns(new[] {characterMock.Object});
            var context = GetContextMockWithSetupLevel();
            var eventTemplateManager = new Mock<IEventTemplateManager>();
            var cueProvider = new BodyAnimationCueProvider(context, eventTemplateManager.Object, assetManagerMock.Object, templatesProviderMock.Object);
            var eventEditorMock = new Mock<IEventEditor>();
            eventEditorMock.Setup(x => x.UseSameBodyAnimation).Returns(false);
            var bodyAnimCueManager = new BodyAnimationCueManager(eventEditorMock.Object, cueProvider);

            //act
            bodyAnimCueManager.SetupActivationCues(ev);

            //assert
            for (var i = 0; i < charactersCount; i++)
            {
                var recordingCharacterController = ev.GetCharacterController(i);
                var templateCharacterController = templateEvent.GetCharacterController(i);
                var recordingActivationCue = recordingCharacterController.GetBodyAnimationController().ActivationCue;
                var templateActivationCue = templateCharacterController.GetBodyAnimationController().ActivationCue;
                if (recordingCharacterController.ControllerSequenceNumber == characterWithChangedBodyAnimNumber)
                {
                    Assert.AreEqual(recordingActivationCue, animPlayBackTime.ToMilliseconds());
                }
                else
                {
                    Assert.AreEqual(recordingActivationCue, templateActivationCue);
                }
            }
        }
    }
}
