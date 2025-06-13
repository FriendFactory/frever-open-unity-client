using System.Collections.Generic;
using Extensions;
using System.Linq;
using Models;
using Modules.LevelManaging.Editing.Players.AssetPlayers;

namespace Modules.LevelManaging.Editing.Players
{
    /// <summary>
    ///  In some cases we should not postpone asset playing between events.
    /// For instance, we should not pause a song, if the song are similar for 2 near events,
    /// and if previous event song end cue is the same as song activation of next one.
    /// </summary>
    
    internal sealed class AssetPlayStateOnEventSwitchingManager
    {
        private readonly HashSet<DbModelType> _continuouslyPlayingSupportAsset;
        private readonly List<ShouldStopAssetBetweenEventsCheck> _checkAlgorithms;

        public AssetPlayStateOnEventSwitchingManager()
        {
            _checkAlgorithms = new List<ShouldStopAssetBetweenEventsCheck>()
            {
                new ShouldSongBetweenEventsCheck(),
                new ShouldStopBodyAnimationBetweenEventsCheck(),
                new ShouldStopUserSoundBetweenEventsCheck(),
                new ShouldStopVideoClipBetweenEventsCheck()
            };

            _continuouslyPlayingSupportAsset = new HashSet<DbModelType>(_checkAlgorithms.Select(x => x.TargetType));
        }

        public bool ShouldStopBetweenEvents(Event currentEvent, Event nextEvent, IAssetPlayer currentPlayer)
        {
            if (!_continuouslyPlayingSupportAsset.Contains(currentPlayer.TargetType))
                return true;

            var check = _checkAlgorithms.First(x => x.TargetType == currentPlayer.TargetType);
            return check.ShouldStop(currentEvent, nextEvent, currentPlayer);
        }
    }
    
    internal abstract class ShouldStopAssetBetweenEventsCheck
    {
        public abstract DbModelType TargetType { get; }

        public abstract bool ShouldStop(Event currentEvent, Event nextEvent, IAssetPlayer currentPlayer);
    }

    internal sealed class ShouldSongBetweenEventsCheck : ShouldStopAssetBetweenEventsCheck
    {
        public override DbModelType TargetType => DbModelType.Song;

        public override bool ShouldStop(Event currentEvent, Event nextEvent, IAssetPlayer currentPlayer)
        {
            var nextMusicController = nextEvent.GetMusicController();
            if (nextMusicController?.SongId == null) return true;

            var currentMusicController = currentEvent.GetMusicController();
            
            var nextSongId = nextMusicController.SongId.Value;
            var currentSongId = currentMusicController.SongId.Value;
            if (nextSongId != currentSongId) return true;
           
            var currentEndCue = currentMusicController.EndCue;
            var nextActivationCue = nextMusicController.ActivationCue;
            return currentEndCue != nextActivationCue;
        }
    }

    internal sealed class ShouldStopUserSoundBetweenEventsCheck : ShouldStopAssetBetweenEventsCheck
    {
        public override DbModelType TargetType => DbModelType.UserSound;

        public override bool ShouldStop(Event currentEvent, Event nextEvent, IAssetPlayer currentPlayer)
        {
            var nextMusicController = nextEvent.GetMusicController();
            if (nextMusicController?.UserSoundId == null) return true;

            var currentMusicController = currentEvent.GetMusicController();
            
            var nextUserSoundId = nextMusicController.UserSoundId.Value;
            var currentUserSoundId = currentMusicController.UserSoundId.Value;
            if (nextUserSoundId != currentUserSoundId) return true;
            
            var currentEndCue = currentMusicController.EndCue;
            var nextActivationCue = nextMusicController.ActivationCue;
            return currentEndCue != nextActivationCue;
        }
    }

    internal sealed class ShouldStopBodyAnimationBetweenEventsCheck : ShouldStopAssetBetweenEventsCheck
    {
        public override DbModelType TargetType => DbModelType.BodyAnimation;

        public override bool ShouldStop(Event currentEvent, Event next, IAssetPlayer assetPlayer)
        {
            var currentBodyAnims = currentEvent.GetCharacterBodyAnimationControllers();
            var nextBodyAnims = next.GetCharacterBodyAnimationControllers();

            var currentBodyAnimController =
                currentBodyAnims.First(x => x.BodyAnimationId == assetPlayer.AssetId);

            return !nextBodyAnims.Any(x =>
                x.BodyAnimationId == currentBodyAnimController.BodyAnimationId &&
                x.ActivationCue == currentBodyAnimController.EndCue);
        }
    }

    internal sealed class ShouldStopVideoClipBetweenEventsCheck : ShouldStopAssetBetweenEventsCheck
    {
        public override DbModelType TargetType => DbModelType.VideoClip;

        public override bool ShouldStop(Event currentEvent, Event nextEvent, IAssetPlayer currentPlayer)
        {
            var nextController = nextEvent.GetSetLocationController();
            if (nextController.VideoClipId == null) return true;

            var currentController = currentEvent.GetSetLocationController();
            return currentController.VideoClipId != nextController.VideoClipId;
        }
    }
}