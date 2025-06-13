using System.Collections.Generic;
using System.Linq;
using Bridge.Models.Common;
using Bridge.Services._7Digital.Models.TrackModels;
using Common;
using Extensions;
using Models;
using Modules.MusicCacheManaging;

namespace Modules.LevelManaging.Editing.LevelManagement
{
    /// <summary>
    ///  Applies music to single event or to full level
    /// </summary>
    internal sealed class MusicApplier
    {
        private readonly ILicensedMusicUsageManager _licensedMusicUsageManager;

        //---------------------------------------------------------------------
        // Ctors
        //---------------------------------------------------------------------
        
        public MusicApplier(ILicensedMusicUsageManager licensedMusicUsageManager)
        {
            _licensedMusicUsageManager = licensedMusicUsageManager;
        }

        //---------------------------------------------------------------------
        // Public
        //---------------------------------------------------------------------
        
        public void ApplyMusicToEvent(Event targetEvent, IPlayableMusic music, int activationCue)
        {
            if (music == null)
            {
                targetEvent.RemoveMusic();
            }
            else
            {
                SetupMusicController(targetEvent, music, activationCue);
            }
        }
        
        public void ApplyMusicToLevel(Level level, IPlayableMusic music, long startFromEventId, int activationCue)
        {
            var startEvent = level.Event.First(x => x.Id == startFromEventId);

            if (music == null)
            {
                startEvent.RemoveMusic();
                return;
            }

            var replacedMusic = startEvent.GetMusic();

            ApplyMusicToEvent(startEvent, music, activationCue);
            startEvent.SetMusicEndCue(CalculateMusicEndCue(startEvent));

            ApplyMusicForNextEventsIfNeeded(level, music, startEvent, replacedMusic);
        }

        public int GetExternalTracksCountAfterReplacing(Level level, ExternalTrackInfo externalTrack, long startFromEventId, int activationCue)
        {
            var lightLevelModel = CreateLevelLightCopy(level);
            ApplyMusicToLevel(lightLevelModel, externalTrack, startFromEventId, activationCue);
            return lightLevelModel.GetExternalSongIds().Count();
        }

        //---------------------------------------------------------------------
        // Helpers
        //---------------------------------------------------------------------

        private void ApplyMusicForNextEventsIfNeeded(Level level, IPlayableMusic music, Event startEvent,
            IPlayableMusic musicBeforeReplace)
        {
            var nextEvents = level.Event
                .OrderBy(ev => ev.LevelSequence)
                .SkipWhile(x => x.LevelSequence <= startEvent.LevelSequence);

            var previousEvent = startEvent;
            foreach (var nextEvent in nextEvents)
            {
                if (!ShouldReplaceMusic(level, nextEvent, musicBeforeReplace, music)) break;

                var musicController = nextEvent.HasMusicController()
                    ? nextEvent.GetMusicController()
                    : SetupNewMusicController(nextEvent);

                musicController.SetMusic(music);

                musicController.ActivationCue = previousEvent.GetMusicController().EndCue;
                musicController.EndCue = CalculateMusicEndCue(nextEvent);
                musicController.LevelSoundVolume = startEvent.GetMusicController().LevelSoundVolume;

                previousEvent = nextEvent;
            }
        }

        private void SetupMusicController(Event ev, IPlayableMusic audio, int activationCue)
        {
            var musicController = ev.HasMusicController() 
                ? PrepareExistingMusicController(ev)
                : SetupNewMusicController(ev);
            musicController.ActivationCue = activationCue;
            musicController.LevelSoundVolume = (int) (GetMusicVolume(ev) * 100);
            musicController.SetMusic(audio);
        }
        
        private MusicController SetupNewMusicController(Event ev)
        {
            var musicController = new MusicController
            {
                EventId = ev.Id
            };
            ev.SetMusicController(musicController);
            return musicController;
        }

        private float GetMusicVolume(Event ev)
        {
            return ev.HasVoiceTrack()
                ? Constants.LevelDefaults.BACKGROUND_MUSIC_VOLUME
                : Constants.LevelDefaults.AUDIO_SOURCE_VOLUME_DEFAULT;
        }
        
        private MusicController PrepareExistingMusicController(Event ev)
        {
            var musicController = ev.GetMusicController();
            musicController.EventId = ev.Id;
            return musicController;
        }

        private int CalculateMusicEndCue(Event ev)
        {
            return ev.GetMusicController().ActivationCue + ev.Length;
        }
        
        private bool ShouldReplaceMusic(Level level, Event nextEvent, IPlayableMusic replacedMusic, IPlayableMusic newMusic)
        {
            var usesDifferentMusicCompareToReplaced =
                replacedMusic != null && nextEvent.HasMusic() && !nextEvent.HasMusic(replacedMusic);
            if (usesDifferentMusicCompareToReplaced) return false;
            
            if (replacedMusic == null && nextEvent.HasMusic())
                return false;

            if (!newMusic.IsLicensed()) return true;
            
            var activationCue = level.GetEventBefore(nextEvent).GetMusicController().EndCue;
            var replacedLicensedSongId = replacedMusic != null && replacedMusic.IsLicensed() ? new long?(replacedMusic.Id) : null;
            var canReplaceForNextEvent = _licensedMusicUsageManager.CanUseForReplacing(level, nextEvent.Id, newMusic.Id, activationCue, replacedLicensedSongId);
            return canReplaceForNextEvent;
        }
        
        private static Level CreateLevelLightCopy(Level level)
        {
            var lightLevelModel = new Level();
            for (var i = 0; i < level.Event.Count; i++)
            {
                var sourceEvent = level.Event.ElementAt(i);
                var ev = new Event();
                lightLevelModel.Event.Add(ev);
                
                ev.Id = sourceEvent.Id;
                ev.LevelSequence = ev.LevelSequence;
                var sourceMusicController = sourceEvent.GetMusicController();
                if (sourceMusicController == null) continue;

                ev.SetMusicController(new MusicController
                {
                    ActivationCue = sourceMusicController.ActivationCue,
                    EndCue = sourceMusicController.EndCue,
                    Song = sourceMusicController.Song,
                    SongId = sourceMusicController.SongId,
                    UserSound = sourceMusicController.UserSound,
                    UserSoundId = sourceMusicController.UserSoundId,
                    ExternalTrack = sourceMusicController.ExternalTrack,
                    ExternalTrackId = sourceMusicController.ExternalTrackId
                });
            }

            return lightLevelModel;
        }
    }
}