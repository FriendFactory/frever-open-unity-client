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
    internal sealed class VfxCueTests: CueTestBase
    {
        [Test]
        public void SetActivationCue_PreviousEventHasTheSameVfx_ShouldInheritEndCueFromPreviousEvent()
        {
            //arrange
            var sharedVfx = new VfxInfo {Id = 1};

            var previousEvent = new Event();
            previousEvent.SetVfxController(new VfxController
            {
                Vfx = sharedVfx,
                VfxId = sharedVfx.Id,
                ActivationCue = 10,
                EndCue = 100
            });

            var recordingEvent = new Event();
            recordingEvent.SetVfxController(new VfxController
            {
                Vfx = sharedVfx,
                VfxId = sharedVfx.Id,
                ActivationCue = 11,
                EndCue = 111
            });

            var assetManagerMock = new Mock<IAssetManager>();
            var templateProviderMock = new Mock<ITemplateProvider>();
            var context = GetContextMockWithSetupLevel(previousEvent);
            var eventTemplateManager = new Mock<IEventTemplateManager>();
            var cueProvider = new VfxCueProvider(context, eventTemplateManager.Object, assetManagerMock.Object, templateProviderMock.Object);

            //act
            var activationCue = cueProvider.GetActivationCue(recordingEvent);

            //assert
            Assert.AreEqual(previousEvent.GetVfxController().EndCue, activationCue);
        }

        [Test]
        public void SetActivationCue_PreviousEventHasDifferentVfx_ShouldUsePlaybackTimeFromAsset()
        {
            //arrange
            var previousVfx = new VfxInfo {Id = 1};

            var previousEvent = new Event();
            previousEvent.SetVfxController(new VfxController
            {
                Vfx = previousVfx,
                VfxId = previousVfx.Id,
                ActivationCue = 10,
                EndCue = 100
            });

            var nextVfx = new VfxInfo {Id = 2};
            var recordingEvent = new Event();
            recordingEvent.SetVfxController(new VfxController
            {
                Vfx = nextVfx,
                VfxId = nextVfx.Id,
                ActivationCue = 11,
                EndCue = 111
            });

            var vfxAssetMock = new Mock<IVfxAsset>();
            const int vfxAssetPlaybackTime = 500;
            vfxAssetMock.Setup(x => x.PlaybackTime).Returns(vfxAssetPlaybackTime);

            var assetManagerMock = new Mock<IAssetManager>();
            assetManagerMock.Setup(x => x.GetActiveAssets<IVfxAsset>()).Returns(new[] {vfxAssetMock.Object});
            var templateProviderMock = new Mock<ITemplateProvider>();
            var context = GetContextMockWithSetupLevel(previousEvent);
            var eventTemplateManager = new Mock<IEventTemplateManager>();
            var cueProvider = new VfxCueProvider(context, eventTemplateManager.Object, assetManagerMock.Object, templateProviderMock.Object);

            //act
            var activationCue = cueProvider.GetActivationCue(recordingEvent);

            //assert
            Assert.AreEqual(vfxAssetPlaybackTime, activationCue);
        }

        [Test]
        public void SetActivationCue_PreviousEventIsNull_ShouldUsePlaybackTimeFromAsset()
        {
            //arrange
            var nextVfx = new VfxInfo {Id = 2};
            var recordingEvent = new Event();
            recordingEvent.SetVfxController(new VfxController
            {
                Vfx = nextVfx,
                VfxId = nextVfx.Id,
                ActivationCue = 11,
                EndCue = 111
            });

            var vfxAssetMock = new Mock<IVfxAsset>();
            const int vfxAssetPlaybackTime = 500;
            vfxAssetMock.Setup(x => x.PlaybackTime).Returns(vfxAssetPlaybackTime);

            var assetManagerMock = new Mock<IAssetManager>();
            assetManagerMock.Setup(x => x.GetActiveAssets<IVfxAsset>()).Returns(new[] {vfxAssetMock.Object});
            var templateProviderMock = new Mock<ITemplateProvider>();
            var context = GetContextMockWithSetupLevel();
            var eventTemplateManager = new Mock<IEventTemplateManager>();
            var cueProvider = new VfxCueProvider(context, eventTemplateManager.Object, assetManagerMock.Object, templateProviderMock.Object);

            //act
            var activationCue = cueProvider.GetActivationCue(recordingEvent);

            //assert
            Assert.AreEqual(vfxAssetPlaybackTime, activationCue);
        }

        [Test]
        public void SetActivationCue_HasTemplateAndPreviousEventIsNull_ShouldUseActivationCueFromTemplate()
        {
            //arrange
            const int templateId = 1;

            var sharedVfx = new VfxInfo {Id = 2};

            var templateEvent = new Event();
            templateEvent.SetVfxController(new VfxController
            {
                Vfx = sharedVfx,
                VfxId = sharedVfx.Id,
                ActivationCue = 10,
                EndCue = 100
            });

            var recordingEvent = new Event();
            recordingEvent.TemplateId = templateId;
            recordingEvent.SetVfxController(new VfxController
            {
                Vfx = sharedVfx,
                VfxId = sharedVfx.Id,
                ActivationCue = 11,
                EndCue = 111
            });

            var assetManagerMock = new Mock<IAssetManager>();
            var templateProviderMock = new Mock<ITemplateProvider>();
            templateProviderMock.Setup(x => x.GetTemplateEventFromCache(templateId)).Returns(templateEvent);
            var context = GetContextMockWithSetupLevel();
            var eventTemplateManager = new Mock<IEventTemplateManager>();
            var cueProvider = new VfxCueProvider(context, eventTemplateManager.Object, assetManagerMock.Object, templateProviderMock.Object);

            //act
            var activationCue = cueProvider.GetActivationCue(recordingEvent);

            //assert
            Assert.AreEqual(templateEvent.GetVfxController().ActivationCue, activationCue);
        }

        [Test]
        public void SetActivationCue_HasTemplateAndRecordingEventHasChangedVfx_ShouldUsePlaybackTimeFromAsset()
        {
            //arrange
            const int templateId = 1;

            var templateVfx = new VfxInfo {Id = 2};

            var templateEvent = new Event();
            templateEvent.SetVfxController(new VfxController
            {
                Vfx = templateVfx,
                VfxId = templateVfx.Id,
                ActivationCue = 10,
                EndCue = 100
            });

            var recordingEvent = new Event();
            var newVfx = new VfxInfo
            {
                Id = templateVfx.Id + 1
            };
            recordingEvent.TemplateId = templateId;
            recordingEvent.SetVfxController(new VfxController
            {
                Vfx = newVfx,
                VfxId = newVfx.Id,
                ActivationCue = 11,
                EndCue = 111
            });

            var vfxAssetMock = new Mock<IVfxAsset>();
            const int vfxAssetPlaybackTime = 500;
            vfxAssetMock.Setup(x => x.PlaybackTime).Returns(vfxAssetPlaybackTime);

            var assetManagerMock = new Mock<IAssetManager>();
            assetManagerMock.Setup(x => x.GetActiveAssets<IVfxAsset>()).Returns(new[] {vfxAssetMock.Object});
            var templateProviderMock = new Mock<ITemplateProvider>();
            templateProviderMock.Setup(x => x.GetTemplateEventFromCache(templateId)).Returns(templateEvent);
            var context = GetContextMockWithSetupLevel();
            var eventTemplateManager = new Mock<IEventTemplateManager>();
            var cueProvider = new VfxCueProvider(context, eventTemplateManager.Object, assetManagerMock.Object, templateProviderMock.Object);

            //act
            var activationCue = cueProvider.GetActivationCue(recordingEvent);

            //assert
            Assert.AreEqual(vfxAssetPlaybackTime, activationCue);
        }

        [Test]
        public void SetActivationCue_PreviousEventHasTheSameTemplateButNewVfx_ShouldUsePlaybackTimeFromAsset()
        {
            //arrange
            const int templateId = 1;

            var templateVfx = new VfxInfo {Id = 2};

            var previousEvent = new Event();
            previousEvent.TemplateId = templateId;
            previousEvent.SetVfxController(new VfxController
            {
                Vfx = templateVfx,
                VfxId = templateVfx.Id,
                ActivationCue = 10,
                EndCue = 100
            });

            var templateEvent = new Event();
            templateEvent.SetVfxController(new VfxController
            {
                Vfx = templateVfx,
                VfxId = templateVfx.Id,
                ActivationCue = 10,
                EndCue = 50
            });

            var recordingEvent = new Event();
            var newVfx = new VfxInfo
            {
                Id = templateVfx.Id + 1
            };
            recordingEvent.TemplateId = templateId;
            recordingEvent.SetVfxController(new VfxController
            {
                Vfx = newVfx,
                VfxId = newVfx.Id,
                ActivationCue = 11,
                EndCue = 111
            });

            var vfxAssetMock = new Mock<IVfxAsset>();
            const int vfxAssetPlaybackTime = 500;
            vfxAssetMock.Setup(x => x.PlaybackTime).Returns(vfxAssetPlaybackTime);

            var assetManagerMock = new Mock<IAssetManager>();
            assetManagerMock.Setup(x => x.GetActiveAssets<IVfxAsset>()).Returns(new[] {vfxAssetMock.Object});
            var templateProviderMock = new Mock<ITemplateProvider>();
            templateProviderMock.Setup(x => x.GetTemplateEventFromCache(templateId)).Returns(templateEvent);
            var context = GetContextMockWithSetupLevel();
            var eventTemplateManager = new Mock<IEventTemplateManager>();
            var cueProvider = new VfxCueProvider(context, eventTemplateManager.Object, assetManagerMock.Object, templateProviderMock.Object);

            //act
            var activationCue = cueProvider.GetActivationCue(recordingEvent);

            //assert
            Assert.AreEqual(vfxAssetPlaybackTime, activationCue);
        }

        [Test]
        public void SetActivationCue_PreviousEventHasDifferentTemplate_ShouldUseActivationCueFromTemplate()
        {
            //arrange
            const int recordingEventTemplateId = 1;
            const int previousEventTemplateId = 2;

            var previousEventVfx = new VfxInfo {Id = 3};
            var previousEvent = new Event();
            previousEvent.TemplateId = previousEventTemplateId;
            previousEvent.SetVfxController(new VfxController
            {
                Vfx = previousEventVfx,
                VfxId = previousEventVfx.Id,
                ActivationCue = 10,
                EndCue = 100
            });

            var recordingEventTemplateVfx = new VfxInfo
            {
                Id = previousEventVfx.Id + 1
            };

            var templateEvent = new Event();
            templateEvent.SetVfxController(new VfxController
            {
                Vfx = recordingEventTemplateVfx,
                VfxId = recordingEventTemplateVfx.Id,
                ActivationCue = 10,
                EndCue = 50
            });

            var recordingEvent = new Event();
            recordingEvent.TemplateId = recordingEventTemplateId;
            recordingEvent.SetVfxController(new VfxController
            {
                Vfx = recordingEventTemplateVfx,
                VfxId = recordingEventTemplateVfx.Id,
                ActivationCue = 11,
                EndCue = 111
            });

            var assetManagerMock = new Mock<IAssetManager>();
            var templateProviderMock = new Mock<ITemplateProvider>();
            templateProviderMock.Setup(x => x.GetTemplateEventFromCache(recordingEventTemplateId)).Returns(templateEvent);

            var context = GetContextMockWithSetupLevel();
            var eventTemplateManager = new Mock<IEventTemplateManager>();
            var cueProvider = new VfxCueProvider(context, eventTemplateManager.Object, assetManagerMock.Object, templateProviderMock.Object);

            //act
            var activationCue = cueProvider.GetActivationCue(recordingEvent);

            //assert
            Assert.AreEqual(templateEvent.GetVfxController().ActivationCue, activationCue);
        }

        [Test]
        public void SetActivationCue_SecondEventHasChangedActivationCue_ShouldInheritFromTemplate()
        {
            //arrange
            const int templateId = 1;

            var templateVfx = new VfxInfo {Id = 2};

            var previousEvent = new Event();
            previousEvent.TemplateId = templateId;
            previousEvent.SetVfxController(new VfxController
            {
                Vfx = templateVfx,
                VfxId = templateVfx.Id,
                ActivationCue = 10,
                EndCue = 100
            });

            var templateEvent = new Event();
            templateEvent.SetVfxController(new VfxController
            {
                Vfx = templateVfx,
                VfxId = templateVfx.Id,
                ActivationCue = 10,
                EndCue = 50
            });

            var recordingEvent = new Event();
            recordingEvent.TemplateId = templateId;
            recordingEvent.SetVfxController(new VfxController
            {
                Vfx = templateVfx,
                VfxId = templateVfx.Id,
                ActivationCue = previousEvent.GetVfxController().EndCue + 1
            });

            var assetManagerMock = new Mock<IAssetManager>();
            var templateProviderMock = new Mock<ITemplateProvider>();
            templateProviderMock.Setup(x => x.GetTemplateEventFromCache(templateId)).Returns(templateEvent);
            var context = GetContextMockWithSetupLevel();
            var eventTemplateManager = new Mock<IEventTemplateManager>();
            var cueProvider = new VfxCueProvider(context, eventTemplateManager.Object, assetManagerMock.Object, templateProviderMock.Object);

            //act
            var activationCue = cueProvider.GetActivationCue(recordingEvent);

            //assert
            Assert.AreEqual(templateEvent.GetVfxController().ActivationCue, activationCue);
        }

        [Test]
        public void SetActivationCue_SecondEventActivationCueAsFirstEventEndCue_ShouldUseEndCue()
        {
            //arrange
            const int templateId = 1;

            var templateVfx = new VfxInfo {Id = 2};

            var previousEvent = new Event();
            previousEvent.TemplateId = templateId;
            previousEvent.SetVfxController(new VfxController
            {
                Vfx = templateVfx,
                VfxId = templateVfx.Id,
                ActivationCue = 10,
                EndCue = 100
            });

            var templateEvent = new Event();
            templateEvent.SetVfxController(new VfxController
            {
                Vfx = templateVfx,
                VfxId = templateVfx.Id,
                ActivationCue = 10,
                EndCue = 50
            });

            var recordingEvent = new Event();
            recordingEvent.TemplateId = templateId;
            recordingEvent.SetVfxController(new VfxController
            {
                Vfx = templateVfx,
                VfxId = templateVfx.Id,
                ActivationCue = previousEvent.GetVfxController().ActivationCue,
                EndCue = previousEvent.GetVfxController().EndCue
            });

            var assetManagerMock = new Mock<IAssetManager>();
            var templateProviderMock = new Mock<ITemplateProvider>();
            templateProviderMock.Setup(x => x.GetTemplateEventFromCache(templateId)).Returns(templateEvent);
            var context = GetContextMockWithSetupLevel(previousEvent);
            var eventTemplateManager = new Mock<IEventTemplateManager>();
            var cueProvider = new VfxCueProvider(context, eventTemplateManager.Object, assetManagerMock.Object, templateProviderMock.Object);

            //act
            var activationCue = cueProvider.GetActivationCue(recordingEvent);

            //assert
            Assert.AreEqual(previousEvent.GetVfxController().EndCue, activationCue);
        }

        [Test]
        public void SetEndCue_PreviousEventIsNull_ShouldUseSumOfActivationCueAndEventLength()
        {
            //arrange
            var recordingEvent = new Event();
            var vfx = new VfxInfo {Id = 1};

            recordingEvent.SetVfxController(new VfxController
            {
                Vfx = vfx,
                VfxId = vfx.Id,
                ActivationCue = 11,
                EndCue = 111
            });

            recordingEvent.Length = 1000;

            var assetManagerMock = new Mock<IAssetManager>();
            var templateProviderMock = new Mock<ITemplateProvider>();
            var context = GetContextMockWithSetupLevel();
            var eventTemplateManager = new Mock<IEventTemplateManager>();
            var cueProvider = new VfxCueProvider(context, eventTemplateManager.Object, assetManagerMock.Object, templateProviderMock.Object);

            //act
            var endCue = cueProvider.GetEndCue(recordingEvent);

            //assert
            var expected = recordingEvent.Length + recordingEvent.GetVfxController().ActivationCue;
            Assert.AreEqual(expected, endCue);
        }
    }
}