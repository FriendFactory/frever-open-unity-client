using System.Collections.Generic;
using System.Linq;
using Bridge.Models.ClientServer.Assets;
using Bridge.Models.Common;
using Bridge.Services._7Digital.Models.TrackModels;
using Common;
using Extensions;
using Models;
using Modules.LevelManaging.Editing.LevelManagement;
using Modules.MusicCacheManaging;
using Moq;
using NUnit.Framework;

namespace Tests.EditMode
{
    internal sealed class MusicApplierTests
    {
        private static float LicensedSongDurationUsageMaxSec => Constants.LicensedMusic.Constraints.LICENSED_SONG_DURATION_USAGE_MAX_SEC;

        [Test]
        [TestCase(1, 4)]
        [TestCase(2, 4)]
        [TestCase(3, 4)]
        [TestCase(4, 4)]
        public void ApplyNewSong_ToLevelWithoutMusic_ShouldSetSongForAllEventsAfterTarget(int targetEventSequence, int eventCount)
        {
            //arrange
            var level = PrepareLevelWithoutMusic(eventCount);

            var licensedMusicManagerMock = GetLicensedMusicManagerMock();
            var applier = new MusicApplier(licensedMusicManagerMock.Object);

            var music = new SongInfo {Id = 10};

            var targetEvent = level.Event.First(x => x.LevelSequence == targetEventSequence);
            
            //act
            applier.ApplyMusicToLevel(level, music, targetEvent.Id, 0);
            
            //assert
            Assert.True(targetEvent.HasMusic(music));

            var eventsAfterTarget = level.GetEventsAfter(targetEvent);
            Assert.True(eventsAfterTarget.All(x=>x.HasMusic(music)));

            var eventsBeforeTarget = level.GetEventsBefore(targetEvent);
            Assert.True(eventsBeforeTarget.All(x=>!x.HasMusic()));
        }

        [Test]
        [TestCase(1, 4)]
        [TestCase(2, 4)]
        [TestCase(3, 4)]
        [TestCase(4, 4)]
        public void ApplySong_ToLevelWithTheSameSong_ShouldReplaceSongForAllEventsAfterTarget(int targetEventSequence, int eventCount)
        {
            //arrange
            var original = new SongInfo {Id = 10};
            var level = PrepareLevelWithMusic(eventCount, original);

            var targetEvent = level.Event.First(x => x.LevelSequence == targetEventSequence);
            var licensedMusicManagerMock = GetLicensedMusicManagerMock();
            var applier = new MusicApplier(licensedMusicManagerMock.Object);

            var newSong = new SongInfo {Id = original.Id + 1};
            //act
            applier.ApplyMusicToLevel(level, newSong, targetEvent.Id, 0);
            
            //assert
            Assert.True(targetEvent.HasMusic(newSong));
            
            var eventsBefore = level.GetEventsBefore(targetEvent);
            Assert.True(eventsBefore.All(x=>x.HasMusic(original)));

            var eventsAfter = level.GetEventsAfter(targetEvent);
            Assert.True(eventsAfter.All(x=>x.HasMusic(newSong)));
        }

        [Test]
        [TestCase(2,4)]
        [TestCase(3,4)]
        [TestCase(4,4)]
        public void ApplySong_InLevelWithDifferentSongs_ShouldReplaceMusicForNextEventsWhichUsesTheSameOriginSong(int eventWithSwitchedSongSequence, int eventCount)
        {
            //arrange
            var song1 = new SongInfo {Id = 1};

            var lvl = PrepareLevelWithMusic(eventCount, song1);
            
            var eventsToHaveAnotherSong = lvl.GetOrderedEvents()
                .TakeWhile(x => x.LevelSequence >= eventWithSwitchedSongSequence).ToArray();
            var song2 = new SongInfo {Id = 2};
            foreach (var ev in eventsToHaveAnotherSong)
            {
               ev.GetMusicController().SetMusic(song2); 
            }

            var newSong = new SongInfo {Id = 3};

            var targetEvent = lvl.GetOrderedEvents().First();
            var licensedMusicManagerMock = GetLicensedMusicManagerMock();
            var applier = new MusicApplier(licensedMusicManagerMock.Object);
            
            //act
            applier.ApplyMusicToLevel(lvl, newSong, targetEvent.Id, 0);
            
            //assert
            Assert.True(targetEvent.HasMusic(newSong));
            
            Assert.True(eventsToHaveAnotherSong.All(x=>x.HasMusic(song2)));

            var eventsBeforeSong2 = lvl.GetEventsBefore(eventWithSwitchedSongSequence);
            Assert.True(eventsBeforeSong2.All(x=>x.HasMusic(newSong)));
        }

        [Test]
        [TestCase(1,4, 1000)]
        [TestCase(2,4, 1000)]
        [TestCase(3,4, 1000)]
        [TestCase(4,4, 1000)]
        public void ApplySong_ToLevelWithMusic_ShouldHaveCorrectCues(int targetLevelSequence, int eventCount, int activationCue)
        {
            //arrange
            var originSong = new SongInfo {Id = 1};
            var lvl = PrepareLevelWithMusic(eventCount, originSong);

            var targetEvent = lvl.Event.First(x => x.LevelSequence == targetLevelSequence);

            var newSong = new SongInfo {Id = 2};
            var licensedMusicManagerMock = GetLicensedMusicManagerMock();
            var applier = new MusicApplier(licensedMusicManagerMock.Object);

            //act
            applier.ApplyMusicToLevel(lvl, newSong, targetEvent.Id, activationCue);

            //assert
            Assert.True(targetEvent.GetMusicController().ActivationCue == activationCue);

            var eventsAfter = lvl.GetEventsAfter(targetEvent);
            var previousEvent = targetEvent;
            foreach (var nextEvent in eventsAfter)
            {
                Assert.True(nextEvent.GetMusicController().ActivationCue == previousEvent.GetMusicController().EndCue);
                previousEvent = nextEvent;
            }
        }
        
        [Test]
        [TestCase(1, 10, 5000, 3)]
        [TestCase(2, 4, 3000, 3)]
        [TestCase(3, 20, 1000, 15)]
        [TestCase(4, 5, 3000, 2)]
        public void ApplyLicensedSong_ToLevelWithTheSingleSong_ShouldReplaceSongForAllEventsAfterTargetUntilReachedLimit(int targetEventSequence, int eventCount, int eventLengthMs, int expectedReplacedEventsCountResult)
        {
            //arrange
            var original = new SongInfo {Id = 10};
            var level = PrepareLevelWithMusic(eventCount, original);
            for (var i = 0; i < eventCount; i++)
            {
                var ev = level.Event.ElementAt(i);
                ev.Length = eventLengthMs;
                var musicController = ev.GetMusicController();
                musicController.ActivationCue = i * eventLengthMs;
                musicController.EndCue = musicController.ActivationCue + eventLengthMs;
            }

            var newSong = new ExternalTrackInfo {Id = original.Id + 1};

            var targetEvent = level.Event.First(x => x.LevelSequence == targetEventSequence);
            var licensedMusicManagerMock = new Mock<ILicensedMusicUsageManager>();
            licensedMusicManagerMock
               .Setup(x => x.CanUseForReplacing(
                          It.IsAny<Level>(),
                          It.IsAny<long>(),
                          It.IsAny<long>(),
                          It.IsAny<int>(),
                          It.IsAny<long?>()))
               .Returns((Level lvl, long nextEventId, long newSongId, int cue, long? replacedSong) =>
                            HasEnoughLicensedSongDurationForNextEvent(lvl, nextEventId, newSongId));
            var applier = new MusicApplier(licensedMusicManagerMock.Object);

            //act
            applier.ApplyMusicToLevel(level, newSong, targetEvent.Id, 0);
            
            //assert
            Assert.True(targetEvent.HasMusic(newSong));
            
            var eventsBefore = level.GetEventsBefore(targetEvent);
            Assert.True(eventsBefore.All(x=> x.HasMusic(original)));
            
            Assert.IsTrue(level.Event.Count(x => x.GetMusicController().ExternalTrackId == newSong.Id) == expectedReplacedEventsCountResult);
        }
        
        private Level PrepareLevelWithMusic(int eventCount, IPlayableMusic music)
        {
            var level = PrepareLevelWithoutMusic(eventCount);
            var activationCue = 0;
            foreach (var ev in level.Event)
            {
                var musicController = new MusicController();
                musicController.EventId = ev.Id;
                musicController.SetMusic(music);
                musicController.ActivationCue = activationCue;
                ev.SetMusicController(musicController);

                musicController.EndCue = activationCue + ev.Length;
                
                activationCue = musicController.EndCue;
            }

            return level;
        }
        
        private Level PrepareLevelWithoutMusic(int eventCount)
        {
            var level = new Level();
            level.Event = new List<Event>();
            for (var i = 0; i < eventCount; i++)
            {
                var ev = new Event();
                ev.Id = i + 100;
                ev.Length = i + 1000;
                ev.LevelSequence = i + 1;
                level.Event.Add(ev);
            }
            return level;
        }
        
        private static Mock<ILicensedMusicUsageManager> GetLicensedMusicManagerMock()
        {
            var licensedMusicManagerMock = new Mock<ILicensedMusicUsageManager>();
            licensedMusicManagerMock
               .Setup(x => x.CanUseForReplacing(It.IsAny<Level>(), It.IsAny<long>(), It.IsAny<long>(), It.IsAny<int>(), It.IsAny<long?>()))
               .Returns(false);
            return licensedMusicManagerMock;
        }

        private static bool HasEnoughLicensedSongDurationForNextEvent(Level level, long nextEvent, long licensedSongId)
        {
            var alreadyUsed = level.Event.Where(x => x.GetMusicController().ExternalTrackId == licensedSongId)
                                   .Sum(x => x.Length);
            var nextEventLength = level.GetEvent(nextEvent).Length;
            return alreadyUsed + nextEventLength <= LicensedSongDurationUsageMaxSec.ToMilliseconds();
        }
    }
}