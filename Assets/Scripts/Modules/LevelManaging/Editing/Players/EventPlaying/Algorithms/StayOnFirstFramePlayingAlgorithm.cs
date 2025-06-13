using System.Collections.Generic;
using Extensions;
using JetBrains.Annotations;
using Modules.LevelManaging.Editing.Players.AssetPlayers;

namespace Modules.LevelManaging.Editing.Players.EventPlaying.Algorithms
{
    /// <summary>
    /// Keeps event on first frame
    /// </summary>
    [UsedImplicitly]
    internal sealed class StayOnFirstFramePlayingAlgorithm: EventPlayingAlgorithm
    {
        public StayOnFirstFramePlayingAlgorithm(IPlayersManager playerManager, EventAssetsProvider eventAssetsProvider) : base(playerManager, eventAssetsProvider)
        {
        }

        protected override void OnPlayStarted()
        {
            Simulate(0, TargetPlayingTypes);
        }

        protected override IReadOnlyCollection<DbModelType> GetAssetTypesToIgnore()
        {
            return PlayerManager.AudioTypes;
        }
    }
}