using System;
using System.Collections.Generic;
using System.Linq;
using Extensions;
using JetBrains.Annotations;
using Modules.LevelManaging.Assets;
using Modules.LevelManaging.Editing.Players.AssetPlayerProfiles;
using Modules.LevelManaging.Editing.Players.EventAssetsProviders;
using Modules.LevelManaging.Editing.Players.PlayersSetup;

namespace Modules.LevelManaging.Editing.Players.AssetPlayers
{
    [UsedImplicitly]
    internal sealed class PlayersManager: IPlayersManager
    {
        private readonly IAssetPlayerProfile[] _profiles;
        
        public IReadOnlyCollection<DbModelType> PlayableTypes { get; }
        public IReadOnlyCollection<DbModelType> AudioTypes { get; }

        public PlayersManager(IAssetPlayerProfile[] profiles)
        {
            _profiles = profiles;

            PlayableTypes = new HashSet<DbModelType>(_profiles.Select(x => x.AssetType));
            AudioTypes = PlayableTypes.Where(x => x.IsAudioType()).ToArray();
        }
        
        public bool IsSupported(DbModelType assetType)
        {
            return PlayableTypes.Contains(assetType);
        }

        public IAssetPlayer CreateAssetPlayer(IAsset targetAsset)
        {
            ThrowExceptionIfTypeIsNotSupported(targetAsset.AssetType);

            var profile = GetProfile(targetAsset.AssetType);
            var player = profile.GetPlayer();
            player.SetTarget(targetAsset);
            return player;
        }
        
        public IEventAssetsProvider GetAssetsProvider(DbModelType targetType)
        {
            ThrowExceptionIfTypeIsNotSupported(targetType);
            
            var profile = GetProfile(targetType);
            return profile.GetAssetsProvider();
        }

        public IPlayerSetup GetSetup(DbModelType type)
        {
            ThrowExceptionIfTypeIsNotSupported(type);

            var profile = GetProfile(type);
            return profile.GetPlayerSetup();
        }

        private void ThrowExceptionIfTypeIsNotSupported(DbModelType type)
        {
            if (!PlayableTypes.Contains(type))
                throw new InvalidOperationException(
                    $"Type is not supported: {type}");
        }

        private IAssetPlayerProfile GetProfile(DbModelType dbModelType)
        {
            return _profiles.First(x => x.AssetType == dbModelType);
        }
    }
}