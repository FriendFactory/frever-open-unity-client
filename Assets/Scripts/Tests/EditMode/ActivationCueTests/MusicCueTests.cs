using Bridge.Models.ClientServer.Assets;
using Extensions;
using Models;
using Modules.LevelManaging.Editing.EventRecording.AssetCueManaging.CueProviders;
using Modules.LevelManaging.Editing.Templates;
using Moq;
using NUnit.Framework;

namespace Tests.EditMode.ActivationCueTests
{
    internal sealed class MusicCueTests: CueTestBase
    {
        [Test]
        public void SetActivationCue_WithoutTemplateOrPreviousEvent_ShouldUseActivationCueFromInputModel()
        {
            //arrange
            const int activationCue = 150;
            
            var song = new SongInfo {Id = 10};

            var ev = new Event();
            var musicController = new MusicController();
            musicController.SetSong(song);
            musicController.ActivationCue = activationCue;
            ev.SetMusicController(musicController);

            var templateProviderMock = new Mock<ITemplateProvider>();
            var context = GetContextMockWithSetupLevel();
            var eventTemplateManager = new Mock<IEventTemplateManager>();
            var cueProvider = new MusicCueProvider(context, eventTemplateManager.Object, templateProviderMock.Object);

            //act
            var result = cueProvider.GetActivationCue(ev);

            //assert
            Assert.AreEqual(activationCue, result);
        }
        
        [Test]
        public void SetActivationCue_WithTemplate_ShouldUseActivationCueFromTemplate()
        {
            //arrange
            const int templateId = 1;
            
            var song = new SongInfo {Id = 10};

            var template = new Event();
            var templateController = new MusicController();
            templateController.SetSong(song);
            templateController.ActivationCue = 100;
            template.SetMusicController(templateController);
            
            var ev = new Event();
            ev.TemplateId = templateId;
            var musicController = new MusicController();
            musicController.SetSong(song);
            musicController.ActivationCue = 10;
            ev.SetMusicController(musicController);

            var templateProviderMock = new Mock<ITemplateProvider>();
            templateProviderMock.Setup(x => x.GetTemplateEventFromCache(templateId)).Returns(template);
            var context = GetContextMockWithSetupLevel();
            var eventTemplateManager = new Mock<IEventTemplateManager>();
            var cueProvider = new MusicCueProvider(context, eventTemplateManager.Object, templateProviderMock.Object);

            //act
            var result = cueProvider.GetActivationCue(ev);

            //assert
            Assert.AreEqual(templateController.ActivationCue, result);
        }
        
        [Test]
        public void SetActivationCue_WithTemplateAndEventBeforeMadeBasedOnTemplate_ShouldUseActivationCueFromTemplate()
        {
            //arrange
            const int templateId = 1;
            
            var song = new SongInfo {Id = 10};

            var template = new Event();
            var templateController = new MusicController();
            templateController.SetSong(song);
            templateController.ActivationCue = 100;
            template.SetMusicController(templateController);
            
            var previous = new Event {Id = 2};
            previous.TemplateId = templateId;
            var previousController = new MusicController();
            previousController.SetSong(song);
            previousController.ActivationCue = 50;
            previous.SetMusicController(previousController);
            
            var ev = new Event();
            ev.TemplateId = templateId;
            var musicController = new MusicController();
            musicController.SetSong(song);
            musicController.ActivationCue = 10;
            ev.SetMusicController(musicController);

            var templateProviderMock = new Mock<ITemplateProvider>();
            templateProviderMock.Setup(x => x.GetTemplateEventFromCache(templateId)).Returns(template);
            var context = GetContextMockWithSetupLevel(previous);
            var eventTemplateManager = new Mock<IEventTemplateManager>();
            eventTemplateManager.Setup(x => x.LastMadeEvent).Returns(ev);
            var cueProvider = new MusicCueProvider(context, eventTemplateManager.Object, templateProviderMock.Object);

            //act
            var result = cueProvider.GetActivationCue(ev);

            //assert
            Assert.AreEqual(templateController.ActivationCue, result);
        }
        
        [Test]
        public void SetActivationCue_NewEventInheritsTemplateFromPrevious_ShouldUseActivationCueFromPreviousEvent()
        {
            //arrange
            const int templateId = 1;
            
            var song = new SongInfo {Id = 10};

            var template = new Event();
            var templateController = new MusicController();
            templateController.SetSong(song);
            templateController.ActivationCue = 100;
            template.SetMusicController(templateController);
            
            var previous = new Event {Id = 2};
            previous.TemplateId = templateId;
            var previousController = new MusicController();
            previousController.SetSong(song);
            previousController.ActivationCue = 50;
            previousController.EndCue = 80;
            previous.SetMusicController(previousController);
            
            var ev = new Event();
            ev.TemplateId = templateId;
            var musicController = new MusicController();
            musicController.SetSong(song);
            musicController.ActivationCue = 10;
            musicController.EndCue = 15;
            ev.SetMusicController(musicController);

            var templateProviderMock = new Mock<ITemplateProvider>();
            templateProviderMock.Setup(x => x.GetTemplateEventFromCache(templateId)).Returns(template);
            var context = GetContextMockWithSetupLevel(previous);
            var eventTemplateManager = new Mock<IEventTemplateManager>();
            eventTemplateManager.Setup(x => x.LastMadeEvent).Returns(previous);
            var cueProvider = new MusicCueProvider(context, eventTemplateManager.Object, templateProviderMock.Object);

            //act
            var result = cueProvider.GetActivationCue(ev);

            //assert
            Assert.AreEqual(previousController.EndCue, result);
        }
    }
}