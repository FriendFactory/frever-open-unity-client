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
    internal sealed class SetLocationCueTests: CueTestBase
    {
        [Test]
        public void SetActivationCue_WithoutTemplateOrPreviousEvent_ShouldUseAssetPlaybackTime()
        {
            //arrange
            const int playbackTime = 500;

            var ev = new Event();
            var setLocationController = new SetLocationController();
            setLocationController.ActivationCue = 100;
            ev.SetSetLocationController(setLocationController);
            var setLocation = new SetLocationFullInfo {Id = 20};
            ev.SetSetLocation(setLocation);

            var setLocationMock = new Mock<ISetLocationAsset>();
            setLocationMock.Setup(x => x.Id).Returns(setLocation.Id);
            setLocationMock.Setup(x => x.PlaybackTimeMs).Returns(playbackTime);

            var assetManagerMock = new Mock<IAssetManager>();
            assetManagerMock.Setup(x => x.GetActiveAssets<ISetLocationAsset>()).Returns(new[] {setLocationMock.Object});

            var templateProviderMock = new Mock<ITemplateProvider>();
            var context = GetContextMockWithSetupLevel();
            var eventTemplateManager = new Mock<IEventTemplateManager>();
            var cueProvider = new SetLocationCueProvider(context, eventTemplateManager.Object, assetManagerMock.Object, templateProviderMock.Object);

            //act
            var activationCue = cueProvider.GetActivationCue(ev);

            //assert
            Assert.AreEqual(playbackTime, activationCue);
        }

        [Test]
        public void SetActivationCue_WithTemplateButChangedSetLocation_ShouldUseAssetPlaybackTime()
        {
            //arrange
            const int playbackTime = 500;
            const int templateId = 1;
            var templateEvent = new Event();
            var templateController = new SetLocationController();
            templateController.ActivationCue = 100;
            templateEvent.SetSetLocationController(templateController);
            var templateSetLocation = new SetLocationFullInfo {Id = 20};
            templateEvent.SetSetLocation(templateSetLocation);

            var ev = new Event();
            ev.TemplateId = templateId;
            var setLocationController = new SetLocationController();
            setLocationController.ActivationCue = 100;
            ev.SetSetLocationController(setLocationController);
            var setLocation = new SetLocationFullInfo
            {
                Id = templateSetLocation.Id + 1
            };
            ev.SetSetLocation(setLocation);

            var setLocationMock = new Mock<ISetLocationAsset>();
            setLocationMock.Setup(x => x.Id).Returns(setLocation.Id);
            setLocationMock.Setup(x => x.PlaybackTimeMs).Returns(playbackTime);

            var assetManagerMock = new Mock<IAssetManager>();
            assetManagerMock.Setup(x => x.GetActiveAssets<ISetLocationAsset>()).Returns(new[] {setLocationMock.Object});

            var templateProviderMock = new Mock<ITemplateProvider>();
            templateProviderMock.Setup(x => x.GetTemplateEventFromCache(templateId)).Returns(templateEvent);
            var context = GetContextMockWithSetupLevel();
            var eventTemplateManager = new Mock<IEventTemplateManager>();
            
            var cueProvider = new SetLocationCueProvider(context, eventTemplateManager.Object, assetManagerMock.Object, templateProviderMock.Object);

            //act
            var activationCue = cueProvider.GetActivationCue(ev);

            //assert
            Assert.AreEqual(playbackTime, activationCue);
        }
    }
}