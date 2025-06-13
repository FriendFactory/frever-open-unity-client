using System;
using System.Collections.Generic;
using System.Linq;
using Bridge.Models.ClientServer.Assets;
using Bridge.Models.Common;
using Bridge.Services._7Digital.Models.TrackModels;
using Models;

namespace Extensions
{
    /// <summary>
    ///     Music related
    /// </summary>
    public static partial class EventExtensions
    {
        public static bool HasMusic(this Event ev)
        {
            return ev.HasMusicController();
        }

        public static void SetMusicEndCue(this Event ev, int endCue)
        {
            if (!ev.HasMusic()) return;
            ev.GetMusicController().EndCue = endCue;
        }
        
        public static void SetMusicActivationCue(this Event ev, int activationCue)
        {
            if (!ev.HasMusic()) return;
            ev.GetMusicController().ActivationCue = activationCue;
        }

        public static MusicController GetMusicController(this Event ev)
        {
            ThrowExceptionIfEventIsNull(ev);
            return ev.MusicController?.FirstOrDefault();
        }
        
        public static void SetMusicController(this Event ev, MusicController musicController)
        {
            ThrowExceptionIfEventIsNull(ev);

            if (ev.MusicController == null)
            {
                ev.MusicController = new List<MusicController>();
            }
            else
            {
                ev.MusicController.Clear();
            }
            
            if (musicController == null) return;
            ev.MusicController.Add(musicController);
        }

        public static SongInfo GetSong(this Event ev)
        {
            ThrowExceptionIfEventIsNull(ev);
            return ev.GetMusicController()?.Song;
        }

        public static void SetSong(this Event ev, SongInfo song)
        {
            ThrowExceptionIfEventIsNull(ev);
            var musicController = ev.GetMusicController();
            musicController.Song = song;
            musicController.SongId = song?.Id;
        }
        public static void SetExternalTrack(this Event ev, ExternalTrackInfo info)
        {
            ThrowExceptionIfEventIsNull(ev);
            var musicController = ev.GetMusicController();
            musicController.ExternalTrack = info;
            musicController.ExternalTrackId = info?.Id;
        }

        public static UserSoundFullInfo GetUserSound(this Event ev)
        {
            ThrowExceptionIfEventIsNull(ev);
            return ev.GetMusicController()?.UserSound;
        }
        
        public static ExternalTrackInfo GetExternalTrack(this Event ev)
        {
            ThrowExceptionIfEventIsNull(ev);
            return ev.GetMusicController()?.ExternalTrack;
        }

        public static void SetUserSound(this Event ev, UserSoundFullInfo userSound)
        {
            ThrowExceptionIfEventIsNull(ev);
            var musicController = ev.GetMusicController();
            musicController.UserSound = userSound;
            musicController.UserSoundId = userSound?.Id;
        }

        public static bool HasTheSameMusic(this Event target, Event compare)
        {
            ThrowExceptionIfEventIsNull(target);
            
            var targetController = target.GetMusicController();
            var compareController = compare?.GetMusicController();

            if (compareController == null || targetController == null)
                return false;

            return targetController.SongId != null && compareController.SongId.Equals(targetController.SongId) || 
                   targetController.UserSoundId != null && compareController.UserSoundId.Equals(targetController.UserSoundId) || 
                   targetController.ExternalTrackId != null && compareController.ExternalTrackId.Equals(targetController.ExternalTrackId);
        }

        public static bool HasMusicController(this Event ev)
        {
            return ev.MusicController?.Any() ?? false;
        }

        public static void RemoveMusic(this Event ev)
        {
            if(!ev.HasMusicController()) return;
            ev.MusicController.Clear();
        }

        public static IPlayableMusic GetMusic(this Event ev)
        {
            var song = ev.GetSong();
            if (song != null) return song;
            
            var userSound = ev.GetUserSound();
            if (userSound != null) return userSound;
            
            return ev.GetExternalTrack();
        }

        public static bool HasMusic(this Event ev, IPlayableMusic music)
        {
            if (music == null) throw new ArgumentNullException(nameof(music));
            
            var currentMusic = ev.GetMusic();
            if (currentMusic == null) return false;
            return currentMusic.GetModelType() == music.GetModelType()
                   && currentMusic.Id == music.Id;
        }

        public static bool HasVoiceTrack(this Event ev)
        {
            return ev.GetVoiceTracks().Any();
        }

        public static long? GetExternalTrackId(this Event ev)
        {
            if (!ev.HasMusic()) return null;
            return ev.GetMusicController().ExternalTrackId;
        }
        
        public static bool HasExternalTrack(this Event ev)
        {
            return ev.GetExternalTrackId().HasValue;
        }

        /// <summary>
        ///   Check whether event has music or voice track
        /// </summary>
        public static bool HasAnyAudio(this Event ev)
        {
            return ev.HasMusic() || ev.HasVoiceTrack();
        }
    }
}