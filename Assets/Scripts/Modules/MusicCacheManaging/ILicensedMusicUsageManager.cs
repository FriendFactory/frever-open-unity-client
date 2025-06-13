using System.Linq;
using Common;
using Extensions;
using JetBrains.Annotations;
using Models;

namespace Modules.MusicCacheManaging
{
    /// <summary>
    /// Contains business rules and checks for using licensed songs, such as song count and length limitations per 1 video/level
    /// </summary>
    public interface ILicensedMusicUsageManager
    {
        int MusicCountPerLevelLimit { get; }
        
        bool CanUseForNewRecording(Level targetLevel, long externalSongId, ref string reason);
        float GetAllowedDurationForNextRecordingInSec(Level level, long externalSongId, int activationCue);
        bool CanUseForReplacing(Level level, long targetEventId, long externalSongId, int activationCue, long? licensedSongToReplaceId, ref string reason);
        bool CanUseForReplacing(Level level, long targetEventId, long externalSongId, int activationCue, long? licensedSongToReplaceId);
    }
    
    [UsedImplicitly]
    internal sealed class LicensedMusicUsageManager: ILicensedMusicUsageManager
    {
        private readonly float _allowedDurationLimitSec;
        private readonly float _licensedSongDuration;
        private readonly int _minEventDurationMS;

        public int MusicCountPerLevelLimit { get; }

        public LicensedMusicUsageManager(int usagePerLevelLimit, float allowedDurationLimitSec, float licensedSongDuration, int minEventDurationMS)
        {
            MusicCountPerLevelLimit = usagePerLevelLimit;
            _allowedDurationLimitSec = allowedDurationLimitSec;
            _licensedSongDuration = licensedSongDuration;
            _minEventDurationMS = minEventDurationMS;
        }

        public bool CanUseForNewRecording(Level targetLevel, long externalSongId, ref string reason)
        {
            var newTotalUsedCount = targetLevel.GetExternalSongIds().Append(externalSongId).Distinct().Count();
            var exceededLimit = newTotalUsedCount > MusicCountPerLevelLimit;
            if (exceededLimit)
            {
                reason = Constants.LicensedMusic.Messages.UNIQUE_MUSIC_PER_LEVEL_LIMIT_REACHED;
                return false;
            }
            
            var totalUsedLengthMs = CalculateTotalUsedLength(targetLevel, externalSongId);
            var hasEnoughSongLeft = _allowedDurationLimitSec - totalUsedLengthMs.ToSeconds() > _minEventDurationMS.ToSeconds();
            if (!hasEnoughSongLeft)
            {
                reason = Constants.LicensedMusic.Messages.MAX_DURATION_PER_LEVEL_LIMIT_REACHED;
            }

            return hasEnoughSongLeft;
        }

        public float GetAllowedDurationForNextRecordingInSec(Level level, long externalSongId, int activationCue)
        {
            string reason = null;
            if (!CanUseForNewRecording(level, externalSongId, ref reason))
            {
                return -1;
            }

            var alreadyUsedMs = CalculateTotalUsedLength(level, externalSongId);
            return (_allowedDurationLimitSec.ToMilli() - alreadyUsedMs).ToSeconds();
        }

        public bool CanUseForReplacing(Level level, long targetEventId, long externalSongId, int activationCue, long? licensedSongToReplaceId)
        {
            string reason = null;
            return CanUseForReplacing(level, targetEventId, externalSongId, activationCue, licensedSongToReplaceId, ref reason);
        }
        
        public bool CanUseForReplacing(Level level, long targetEventId, long externalSongId, int activationCue, long? licensedSongToReplaceId, ref string reason)
        {
            if (!licensedSongToReplaceId.HasValue && level.GetUsedLicensedSongsIds().Length == MusicCountPerLevelLimit)
            {
                reason = $"Can't change song. Video cannot contain more than {MusicCountPerLevelLimit} licensed songs";
                return false;
            }
            
            var targetEvent = level.GetEvent(targetEventId);
            if (targetEvent.GetMusicController()?.ExternalTrackId == externalSongId) return true;
            
            var endCue = activationCue + targetEvent.Length;
            if (endCue > _licensedSongDuration.ToMilli())
            {
                reason = $"Can't change song. Event is longer than {_allowedDurationLimitSec} seconds";
                return false;
            }
            var alreadyUsedMs = CalculateTotalUsedLength(level, externalSongId);
            var lengthAfterApplying = alreadyUsedMs + (endCue - activationCue);
            var canReplace = lengthAfterApplying <= _licensedSongDuration.ToMilli() && lengthAfterApplying <= _allowedDurationLimitSec.ToMilliseconds();
            if (!canReplace)
            {
                reason = $"Can't change song. Song length limit is {_allowedDurationLimitSec} seconds for level";
            }
            return canReplace;
        }

        private static int CalculateTotalUsedLength(Level level, long externalSongId)
        {
            return level.Event
                                           .Where(x => x.GetExternalTrackId() == externalSongId)
                                           .Select(x => x.GetMusicController())
                                           .Select(x => x.EndCue - x.ActivationCue).Sum();
        }
        
    }
}