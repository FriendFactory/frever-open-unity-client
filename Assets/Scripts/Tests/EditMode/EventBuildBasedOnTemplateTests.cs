using System;
using System.Collections.Generic;
using System.Linq;
using Bridge.Models.ClientServer.Assets;
using Bridge.Models.Common.Files;
using Extensions;
using Models;
using Modules.LevelManaging.Editing.Templates;
using NUnit.Framework;

namespace Tests.EditMode
{
    internal sealed class EventBuildBasedOnTemplateTests 
    {
        [Test]
        public void SetLocationControllerBuildStep_MustInheritSetLocationControllersFields()
        {
            //arrange
            var freshEvent = new Event();
            
            var template = new Event();
            var templateController = new SetLocationController
            {
                SetLocation = new SetLocationFullInfo {Id = 1000},
                Photo = CreatePhoto(1001),
                VideoClip = CreateVideo(1002),
                WeatherId = 10,
                ActivationCue = 20,
                EndCue = 30,
                VideoActivationCue = 40,
                VideoEndCue = 50,
                TimeOfDay = 60
            };
            template.SetLocationController = new List<SetLocationController> {templateController};

            var buildStepArgs = new BuildStepArgs
            {
                Template = template,
                ReplaceCharactersData = Array.Empty<ReplaceCharacterData>(),
                TargetEvent = freshEvent
            };
            var buildStep = new SetLocationControllerBuildStep();
            
            //act
            buildStep.Run(buildStepArgs);
            
            //assert

            var freshEventController = freshEvent.GetSetLocationController();
            Assert.AreEqual(templateController.WeatherId, freshEventController.WeatherId);
            Assert.AreEqual(templateController.ActivationCue, freshEventController.ActivationCue);
            Assert.AreEqual(templateController.EndCue, freshEventController.EndCue);
            Assert.AreEqual(templateController.VideoActivationCue, freshEventController.VideoActivationCue);
            Assert.AreEqual(templateController.VideoEndCue, freshEventController.VideoEndCue);
            Assert.AreEqual(templateController.TimeOfDay, freshEventController.TimeOfDay);
            Assert.AreEqual(freshEventController.Photo.Id, 0);
            Assert.AreEqual(freshEventController.PhotoId, 0);
            Assert.AreEqual(freshEventController.VideoClip.Id, 0 );
            Assert.AreEqual(freshEventController.VideoClipId, 0);
        }
        
        [Test]
        public void MusicControllerBuildStep_MustInheritMusicControllerFields()
        {
            //arrange
            var freshEvent = new Event();

            var template = new Event();
            var templateController = new MusicController()
            {
                Song = new SongInfo {Id = 10},
                ActivationCue = 20,
                EndCue = 30,
                LevelSoundVolume = 40
            };
            template.MusicController = new List<MusicController> {templateController};

            var buildArgs = new BuildStepArgs
            {
                Template = template,
                ReplaceCharactersData = Array.Empty<ReplaceCharacterData>(),
                TargetEvent = freshEvent
            };
            var buildStep = new MusicControllerBuildStep();
            
            //act
            buildStep.Run(buildArgs);
            
            //assert
            var freshEventController = freshEvent.GetMusicController();
            Assert.AreEqual(templateController.Song.Id, freshEventController.Song.Id);
            Assert.AreEqual(templateController.Song.Id, freshEventController.SongId);
            Assert.AreEqual(templateController.ActivationCue, freshEventController.ActivationCue);
            Assert.AreEqual(templateController.EndCue, freshEventController.EndCue);
            Assert.AreEqual(templateController.LevelSoundVolume, freshEventController.LevelSoundVolume);
            Assert.IsNull(freshEventController.UserSoundId);
            Assert.IsNull(freshEventController.UserSound);
        }
        
        [Test]
        public void VfxControllerStep_MustInheritVfxController()
        {
            //arrange
            var freshEvent = new Event();

            var template = new Event();
            var templateController = new VfxController()
            {
                Vfx = new VfxInfo {Id = 10},
                ActivationCue = 20,
                EndCue = 30
            };
            template.VfxController = new List<VfxController> {templateController};

            var buildArgs = new BuildStepArgs
            {
                Template = template,
                ReplaceCharactersData = Array.Empty<ReplaceCharacterData>(),
                TargetEvent = freshEvent
            };
            var buildStep = new VfxControllerBuildStep();
            
            //act
            buildStep.Run(buildArgs);
            
            //assert

            var freshEventController = freshEvent.GetVfxController();
            Assert.AreEqual(templateController.Vfx.Id, freshEventController.Vfx.Id);
            Assert.AreEqual(templateController.Vfx.Id, freshEventController.VfxId);
            Assert.AreEqual(templateController.ActivationCue, freshEventController.ActivationCue);
            Assert.AreEqual(templateController.EndCue, freshEventController.EndCue);
        }
        
        [Test]
        public void VfxControllerStepWithoutVfx_MustIgnoreInheritingVfxController()
        {
            //arrange
            var freshEvent = new Event();

            var template = new Event();
            template.VfxController = new List<VfxController>();

            var buildArgs = new BuildStepArgs
            {
                Template = template,
                ReplaceCharactersData = Array.Empty<ReplaceCharacterData>(),
                TargetEvent = freshEvent
            };
            var buildStep = new VfxControllerBuildStep();
            
            //act
            buildStep.Run(buildArgs);
            
            //assert

            Assert.IsNull(freshEvent.GetVfxController());
        }
        
        [Test]
        public void TemplateWithMultipleCharacter_MustOverrideAllCharactersInDestination()
        {
            //arrange
            const int charactersCount = 5;

            var characters = new ReplaceCharacterData[charactersCount];
            for (var i = 0; i < charactersCount; i++)
            {
                var originId = GetOriginCharacterId(i);
                var replaceId = GetReplaceCharacterId(originId);
                var character = new CharacterFullInfo {Id = replaceId};
                characters[i] = new ReplaceCharacterData(originId, character);
            }
            
            var freshEvent = new Event();
            
            var template = new Event();
            template.CharacterController = new List<CharacterController>(charactersCount);

            for (var i = 0; i < charactersCount; i++)
            {
                var cc = new CharacterController
                {
                    CharacterId = GetOriginCharacterId(i),
                    CharacterControllerBodyAnimation = new List<CharacterControllerBodyAnimation>
                    {
                        new CharacterControllerBodyAnimation {BodyAnimation = new BodyAnimationInfo {Id = i*1000}}
                    }
                };
                template.CharacterController.Add(cc);
            }

            long GetOriginCharacterId(int index)
            {
                return index;
            }
            
            long GetReplaceCharacterId(long originCharacterId)
            {
                return originCharacterId * 100;
            }

            var buildArgs = new BuildStepArgs()
            {
                Template = template,
                ReplaceCharactersData = characters,
                TargetEvent = freshEvent
            };
            
            var buildStep = new CharacterControllerBuildStep();
            
            //act
            buildStep.Run(buildArgs);
            
            //assert
            for (var i = 0; i < charactersCount; i++)
            {
                var destCharacterController = freshEvent.CharacterController.ElementAt(i);
                var destBodyAnim = destCharacterController.GetBodyAnimationController().GetBodyAnimation();
                var templateCharacterController = template.CharacterController.ElementAt(i);
                var templateBodyAnim = templateCharacterController.GetBodyAnimationController().GetBodyAnimation();
                Assert.AreEqual(templateBodyAnim.Id, destBodyAnim.Id);   
                Assert.AreEqual(destCharacterController.CharacterId, GetReplaceCharacterId(templateCharacterController.CharacterId));
            }
        }

        [Test]
        public void TryBuildFreshEventWithWrongCharactersCount_MustThrowException()
        {
            //arrange
            const int replaceCharacterCount = 2;
            const int templateCharactersCount = 3;

            var characters = new ReplaceCharacterData[replaceCharacterCount];
            for (var i = 0; i < replaceCharacterCount; i++)
            {
                characters[i] = new ReplaceCharacterData(i, new CharacterFullInfo {Id = i + 1});
            }
            
            var template = new Event();
            template.CharacterController = new List<CharacterController>();
            for (var i = 0; i < templateCharactersCount; i++)
            {
                template.CharacterController.Add(new CharacterController
                {
                    Character = new CharacterFullInfo {Id = i}
                });
            }

            var templateManager = new EventTemplateManager(Array.Empty<IEventBuildStep>());
            
            //act & assert
            Assert.Catch<UniqueCharacterMismatchTemplateValidationException>(() =>
            {
                templateManager.CreateFreshEventBasedOnTemplate(template, characters);
            });
        }
        
        [Test]
        public void TryBuildFreshEventWithTheCorrectCharactersCount_MustPassValidation()
        {
            //arrange
            const int uniqueCharactersCount = 2;
            const int templateCharacterControllersCount = 4;

            var characters = new ReplaceCharacterData[uniqueCharactersCount];
            for (var i = 0; i < uniqueCharactersCount; i++)
            {
                characters[i] = new ReplaceCharacterData(i, new CharacterFullInfo {Id = i + 1});
            }
            
            var template = new Event();
            template.CharacterController = new List<CharacterController>();
            for (var i = 0; i < templateCharacterControllersCount; i++)
            {
                var nextId = i % uniqueCharactersCount * 100;
                template.CharacterController.Add(new CharacterController
                {
                    Character = new CharacterFullInfo {Id = nextId},
                    CharacterId = nextId
                });
            }
            
            var eventTemplateManager = new EventTemplateManager(Array.Empty<IEventBuildStep>());
            
            //act
            eventTemplateManager.CreateFreshEventBasedOnTemplate(template, characters);
            
            //assert
            Assert.Pass();
        }

        [Test]
        public void EventBuildStep_MustInheritEventMetaData()
        {
            //arrange
            var freshEvent = new Event();

            var template = new Event
            {
                CharacterSpawnPositionId = 10,
                TargetCharacterSequenceNumber = 20
            };

            var buildArgs = new BuildStepArgs
            {
                Template = template,
                TargetEvent = freshEvent
            };
            
            var buildStep = new EventDataTemplateBuildStep();
            
            //act
            buildStep.Run(buildArgs);
            
            //assert
            Assert.AreEqual(template.CharacterSpawnPositionId, freshEvent.CharacterSpawnPositionId);
            Assert.AreEqual(template.CharacterSpawnPositionFormationId, freshEvent.CharacterSpawnPositionFormationId);
        }

        [Test]
        public void CameraControllerBuildStep_MustInheritCameraController()
        {
            //arrange

            var freshEvent = new Event();

            var template = new Event();
            template.CameraController = new List<CameraController>();
            var templateController = new CameraController();
            template.SetCameraController(templateController);
            
            templateController.ActivationCue = 1;
            templateController.EndCue = 2;
            templateController.TemplateSpeed = 3;
            templateController.CameraAnimationTemplateId = 4;
            templateController.StartFocusDistance = 5;
            templateController.EndFocusDistance = 6;
            templateController.EndDepthOfFieldOffset = 7;
            templateController.CameraNoiseSettingsIndex = 8;
            templateController.LookAtIndex = 9;
            templateController.FollowSpawnPositionIndex = 10;
            templateController.FollowAll = true;
            templateController.FollowZoom = true;
            templateController.CameraAnimation = new CameraAnimationFullInfo() {Id = 10};
            
            var buildStep = new CameraControllerBuildStep();
            var buildArgs = new BuildStepArgs()
            {
                Template = template,
                ReplaceCharactersData = Array.Empty<ReplaceCharacterData>(),
                TargetEvent = freshEvent
            };
            
            //act
            buildStep.Run(buildArgs);
            
            //assert
            var controller = freshEvent.GetCameraController();
            Assert.IsNotNull(controller);
            Assert.AreEqual(controller.ActivationCue, templateController.ActivationCue);
            Assert.AreEqual(controller.EndCue, templateController.EndCue);
            Assert.AreEqual(controller.TemplateSpeed, templateController.TemplateSpeed);
            Assert.AreEqual(controller.CameraAnimationTemplateId, templateController.CameraAnimationTemplateId);
            Assert.AreEqual(controller.StartFocusDistance, templateController.StartFocusDistance);
            Assert.AreEqual(controller.EndFocusDistance, templateController.EndFocusDistance);
            Assert.AreEqual(controller.EndDepthOfFieldOffset, templateController.EndDepthOfFieldOffset);
            Assert.AreEqual(controller.CameraNoiseSettingsIndex, templateController.CameraNoiseSettingsIndex);
            Assert.AreEqual(controller.LookAtIndex, templateController.LookAtIndex);
            Assert.AreEqual(controller.FollowAll, templateController.FollowAll);
            Assert.AreEqual(controller.FollowZoom, templateController.FollowZoom);
            Assert.AreEqual(controller.FollowSpawnPositionIndex, templateController.FollowSpawnPositionIndex);
        }
        
        [Test]
        public void CameraControllerBuildStep_MustIgnoreCameraAnimationAsset()
        {
            //arrange

            var freshEvent = new Event();

            var template = new Event();
            template.CameraController = new List<CameraController>();
            var templateController = new CameraController();
            template.SetCameraController(templateController);
            templateController.CameraAnimation = new CameraAnimationFullInfo() {Id = 10};
            
            var buildStep = new CameraControllerBuildStep();
            var buildArgs = new BuildStepArgs()
            {
                Template = template,
                ReplaceCharactersData = Array.Empty<ReplaceCharacterData>(),
                TargetEvent = freshEvent
            };
            
            //act
            buildStep.Run(buildArgs);
            
            //assert
            var controller = freshEvent.GetCameraController();
            Assert.IsNotNull(controller);
            Assert.AreNotEqual(controller.CameraAnimation, templateController.CameraAnimation);
        }

        private PhotoFullInfo CreatePhoto(long id)
        {
            var photo = new PhotoFullInfo {Id = id, Files = new List<FileInfo>()};
            var fileInfo = new FileInfo(FileType.MainFile);
            photo.Files.Add(fileInfo);
            return photo;
        }
        
        private VideoClipFullInfo CreateVideo(long id)
        {
            var video = new VideoClipFullInfo {Id = id, Files = new List<FileInfo>()};
            var fileInfo = new FileInfo(FileType.MainFile);
            video.Files.Add(fileInfo);
            return video;
        }
    }
}
