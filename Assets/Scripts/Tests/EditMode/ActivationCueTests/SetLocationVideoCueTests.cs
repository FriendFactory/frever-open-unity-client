using Bridge.Models.ClientServer.Assets;
using Extensions;
using Models;
using Modules.AssetsManaging;
using Modules.LevelManaging.Assets;
using Modules.LevelManaging.Editing.EventRecording.AssetCueManaging.CueProviders;
using Modules.LevelManaging.Editing.LevelManagement;
using Modules.LevelManaging.Editing.Templates;
using Moq;
using NUnit.Framework;

namespace Tests.EditMode.ActivationCueTests
{
    internal sealed class SetLocationVideoCueTests: CueTestBase
    {
        [Test]
        public void SetActivationCue_WithoutTemplateOrPreviousEvent_ShouldUseAssetPlaybackTime()
        {
            //arrange
            const int playbackTime = 5;

            var ev = new Event();
            var setLocationController = new SetLocationController();
            setLocationController.VideoActivationCue = 100;
            ev.SetSetLocationController(setLocationController);
            var setLocation = new SetLocationFullInfo
            {
                Id = 120,
                AllowVideo = true
            };
            ev.SetSetLocation(setLocation);
            var setLocationVideo = new VideoClipFullInfo {Id = 140};
            ev.SetVideo(setLocationVideo);

            var setLocationMock = new Mock<ISetLocationAsset>();
            setLocationMock.Setup(x => x.Id).Returns(setLocation.Id);
            setLocationMock.Setup(x => x.VideoPlaybackTime).Returns(playbackTime);

            var assetManagerMock = new Mock<IAssetManager>();
            assetManagerMock.Setup(x => x.GetActiveAssets<ISetLocationAsset>()).Returns(new[] {setLocationMock.Object});

            var templateProviderMock = new Mock<ITemplateProvider>();
            var context = GetContextMockWithSetupLevel();
            var eventTemplateManager = new Mock<IEventTemplateManager>();

            var cueProvider = new VideoCueProvider(context, eventTemplateManager.Object, assetManagerMock.Object, templateProviderMock.Object);

            //act
            var activationCue = cueProvider.GetActivationCue(ev);

            //assert
            Assert.AreEqual(playbackTime.ToMilliseconds(), activationCue);
        }

        [Test]
        public void SetActivationCue_WithTemplate_ShouldUseCueFromTemplate()
        {
            //arrange
            const int templateId = 15;

            var template = new Event();
            var templateController = new SetLocationController();
            templateController.VideoActivationCue = 100;
            template.SetSetLocationController(templateController);
            var setLocation = new SetLocationFullInfo
            {
                Id = 120,
                AllowVideo = true
            };
            template.SetSetLocation(setLocation);
            var setLocationVideo = new VideoClipFullInfo {Id = 140};
            template.SetVideo(setLocationVideo);

            var ev = new Event();
            ev.TemplateId = templateId;
            var setLocationController = new SetLocationController();
            ev.SetSetLocationController(setLocationController);
            ev.SetSetLocation(setLocation);
            ev.SetVideo(setLocationVideo);

            var assetManagerMock = new Mock<IAssetManager>();

            var templateProviderMock = new Mock<ITemplateProvider>();
            templateProviderMock.Setup(x => x.GetTemplateEventFromCache(templateId)).Returns(template);
            var context = GetContextMockWithSetupLevel();
            var eventTemplateManager = new Mock<IEventTemplateManager>();
            
            var cueProvider = new VideoCueProvider(context, eventTemplateManager.Object, assetManagerMock.Object, templateProviderMock.Object);

            //act
            var activationCue = cueProvider.GetActivationCue(ev);

            //assert
            var expected = template.GetSetLocationController().VideoActivationCue;
            Assert.AreEqual(expected, activationCue);
        }

        [Test]
        public void SetActivationCue_WithTemplateAndPreviousEvent_ShouldUseEndCueFromPreviousEvent()
        {
            //arrange
            const int templateId = 15;

            var setLocation = new SetLocationFullInfo
            {
                Id = 120,
                AllowVideo = true
            };

            var setLocationVideo = new VideoClipFullInfo {Id = 140};

            var previousEvent = new Event();
            previousEvent.TemplateId = templateId;
            var previousController = new SetLocationController();
            previousController.VideoActivationCue = 100;
            previousController.VideoEndCue = 200;
            previousEvent.SetSetLocationController(previousController);
            previousEvent.SetSetLocation(setLocation);
            previousEvent.SetVideo(setLocationVideo);

            var ev = new Event();
            ev.TemplateId = templateId;
            var setLocationController = new SetLocationController();
            setLocationController.VideoEndCue = previousController.VideoEndCue;
            ev.SetSetLocationController(setLocationController);
            ev.SetSetLocation(setLocation);
            ev.SetVideo(setLocationVideo);

            var assetManagerMock = new Mock<IAssetManager>();

            var templateProviderMock = new Mock<ITemplateProvider>();
            var context = GetContextMockWithSetupLevel(previousEvent);
            var eventTemplateManager = new Mock<IEventTemplateManager>();
            
            var cueProvider = new VideoCueProvider(context, eventTemplateManager.Object, assetManagerMock.Object, templateProviderMock.Object);

            //act
            var activationCue = cueProvider.GetActivationCue(ev);

            //assert
            Assert.AreEqual(previousController.VideoEndCue, activationCue);
        }

        [Test]
        public void SetActivationCue_WithOutTemplateButWithPreviousEvent_ShouldUseEndCueFromPreviousEvent()
        {
            //arrange
            var setLocation = new SetLocationFullInfo
            {
                Id = 120,
                AllowVideo = true
            };

            var setLocationVideo = new VideoClipFullInfo {Id = 140};

            var previousEvent = new Event();
            var previousController = new SetLocationController();
            previousController.VideoActivationCue = 100;
            previousController.VideoEndCue = 200;
            previousEvent.SetSetLocationController(previousController);
            previousEvent.SetSetLocation(setLocation);
            previousEvent.SetVideo(setLocationVideo);

            var ev = new Event();
            var setLocationController = new SetLocationController();
            setLocationController.VideoEndCue = previousController.VideoEndCue;
            ev.SetSetLocationController(setLocationController);
            ev.SetSetLocation(setLocation);
            ev.SetVideo(setLocationVideo);

            var assetManagerMock = new Mock<IAssetManager>();

            var templateProviderMock = new Mock<ITemplateProvider>();
            var context = GetContextMockWithSetupLevel(previousEvent);
            var eventTemplateManager = new Mock<IEventTemplateManager>();
            var cueProvider = new VideoCueProvider(context, eventTemplateManager.Object, assetManagerMock.Object, templateProviderMock.Object);

            //act
            var activationCue = cueProvider.GetActivationCue(ev);

            //assert
            Assert.AreEqual(previousController.VideoEndCue, activationCue);
        }
    }
}