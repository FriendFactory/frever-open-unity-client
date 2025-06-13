using Bridge.Models.ClientServer.Assets;
using Bridge.Models.Common;
using Bridge.Services._7Digital.Models.TrackModels;

namespace Models
{
    public class MusicController : IEntity
    {
        public long Id { get; set; }
        public long EventId { get; set; }
        public long? SongId { get; set; }
        public long? ScoreId { get; set; }
        public long? UserSoundId { get; set; }
        public long? ExternalTrackId { get; set; }
        public int ActivationCue { get; set; }
        public int EndCue { get; set; }
        public int LevelSoundVolume { get; set; }

        public virtual SongInfo Song { get; set; }
        public virtual UserSoundFullInfo UserSound { get; set; }
        public virtual ExternalTrackInfo ExternalTrack { get; set; }
    }
}