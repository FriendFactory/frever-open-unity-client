using System;
using Bridge.Models.ClientServer.Assets;
using Bridge.Models.Common;
using Bridge.Services._7Digital.Models.TrackModels;

namespace Extensions
{
    public static class MusicExtensions
    {
        public static bool IsLicensed(this IPlayableMusic music)
        {
            return music.GetModelType() == DbModelType.ExternalTrack;
        }
        
        public static string GetName(this IPlayableMusic sound)
        {
            var name = sound.GetSoundName();
            var artistName = sound.GetArtistName();

            return string.IsNullOrEmpty(artistName) ? name : $"{artistName} - {name}";
        }
        
        public static string GetSoundName(this IPlayableMusic sound)
        {
            switch (sound)
            {
                case SongInfo song:
                    return song.Name;
                case UserSoundFullInfo userSong:
                    return userSong.Name;
                case ExternalTrackInfo track:
                    return track.Title;
                case FavouriteMusicInfo favoriteSound:
                    return favoriteSound.SongName;
                default:
                    throw new InvalidOperationException($"Song panel does not support audio type: {sound.GetType().Name}");
            }
        }
        
        public static string GetArtistName(this IPlayableMusic sound)
        {
            switch (sound)
            {
                case SongInfo song:
                    return song.Artist?.Name;
                case UserSoundFullInfo userSong:
                    return userSong.Owner?.Nickname;
                case ExternalTrackInfo track:
                    return track.ArtistName;
                case FavouriteMusicInfo favoriteSound:
                    return favoriteSound.Type == SoundType.UserSound ? favoriteSound.Owner?.Nickname : favoriteSound.ArtistName;
                default:
                    throw new InvalidOperationException($"Song panel does not support audio type: {sound.GetType().Name}");
            }
        }

        public static SoundType GetFavoriteSoundType(this IPlayableMusic sound)
        {
            switch (sound)
            {
                case SongInfo _:
                    return SoundType.Song;
                case UserSoundFullInfo _:
                    return SoundType.UserSound;
                case ExternalTrackInfo _:
                    return SoundType.ExternalSong;
                case FavouriteMusicInfo favouriteSound:
                    return favouriteSound.Type;
                default:
                    throw new InvalidOperationException($"Failed to determine SoundType for {sound.GetType().Name}");
            }
        }

        public static IPlayableMusic GetSoundAsset(this FavouriteMusicInfo favoriteSound)
        {
            switch (favoriteSound.Type)
            {
                case SoundType.Song:
                    return ConvertToSoundAsset(new SongInfo());
                case SoundType.UserSound:
                    return new UserSoundFullInfo
                    {
                        Id = favoriteSound.Id,
                        Files = favoriteSound.Files,
                        Duration = favoriteSound.Duration,
                        Name = favoriteSound.GetName(),
                        IsFavorite = favoriteSound.IsFavoriteSound(),
                        Owner = favoriteSound.Owner,
                        UsageCount = favoriteSound.UsageCount
                    };
                case SoundType.ExternalSong:
                    throw new ArgumentException("External song conversion is not supported");
                default:
                    throw new ArgumentOutOfRangeException();
            }

            IPlayableMusic ConvertToSoundAsset(IPlayableMusic target)
            {
                target.Id = favoriteSound.Id;
                target.Files = favoriteSound.Files;
                target.Duration = favoriteSound.Duration;

                return target;
            }
        }

        public static long GetSoundId(this IPlayableMusic sound)
        {
            switch (sound)
            {
                case SongInfo song:
                    return song.Id;
                case UserSoundFullInfo userSong:
                    return userSong.Id;
                case ExternalTrackInfo track:
                    return track.Id;
                case FavouriteMusicInfo favoriteSound:
                    return favoriteSound.Id;
                default:
                    throw new InvalidOperationException($"Failed to determine id for {sound.GetType().Name}");
                
            }
        }

        public static bool IsFavoriteSound(this IPlayableMusic sound) => sound is FavouriteMusicInfo;

        public static string GetDurationFormatted(this IPlayableMusic sound)
        {
            var timeSpan = TimeSpan.FromMilliseconds(sound.Duration);
            
            return timeSpan.ToString(@"mm\:ss");
        }
    }
}