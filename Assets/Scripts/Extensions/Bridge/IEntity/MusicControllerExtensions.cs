using  System;
using Bridge.Models.ClientServer.Assets;
using Bridge.Models.Common;
using Bridge.Services._7Digital.Models.TrackModels;
using Models;
using UnityEngine;

namespace Extensions
{
    public static class MusicControllerExtensions
    {
        public static void SetSong(this MusicController controller, SongInfo song)
        {
            if (controller == null)
            {
                Debug.LogException(new Exception($"{nameof(controller)} is null!"));
                return;
            }

            controller.Song = song;

            if (song == null) controller.SongId = null;
            else controller.SongId = song.Id;
        }
        
        public static void SetUserSound(this MusicController controller, UserSoundFullInfo userSound)
        {
            if (controller == null)
            {
                Debug.LogException(new Exception($"{nameof(controller)} is null!"));
                return;
            }

            controller.UserSound = userSound;

            if (userSound == null) controller.UserSoundId = null;
            else controller.UserSoundId = userSound.Id;
        }
        
        public static void SetExternalTrack(this MusicController controller, ExternalTrackInfo trackInfo)
        {
            if (controller == null)
            {
                Debug.LogException(new Exception($"{nameof(controller)} is null!"));
                return;
            }
            controller.ExternalTrack = trackInfo;
            if (trackInfo == null) controller.ExternalTrackId = null;
            else controller.ExternalTrackId = trackInfo.Id;
        }

        public static void SetMusic(this MusicController controller, IPlayableMusic music)
        {
            controller.RemoveMusic();
            switch (music.GetModelType())
            {
                case DbModelType.UserSound:
                    controller.SetUserSound(music as UserSoundFullInfo);
                    return;
                case DbModelType.Song:
                    controller.SetSong(music as SongInfo);
                    return;
                case DbModelType.ExternalTrack:
                    controller.SetExternalTrack(music as ExternalTrackInfo);
                    return;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private static void RemoveMusic(this MusicController controller)
        {
            controller.SetSong(null);
            controller.SetUserSound(null);
            controller.SetExternalTrack(null);
        }
    }
}
