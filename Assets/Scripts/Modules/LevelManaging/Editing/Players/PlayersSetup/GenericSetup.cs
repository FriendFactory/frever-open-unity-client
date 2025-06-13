using System;
using Extensions;
using Modules.LevelManaging.Assets;
using Modules.LevelManaging.Assets.AssetDependencies;
using Modules.LevelManaging.Editing.Players.AssetPlayers;
using UnityEngine;
using Event = Models.Event;

namespace Modules.LevelManaging.Editing.Players.PlayersSetup
{
    internal abstract class GenericSetup<TAsset, TPlayer>: IPlayerSetup<TPlayer> where TAsset: IAsset where TPlayer: AssetPlayerBase<TAsset>
    {
        public void Setup(IAssetPlayer assetPlayer, Event ev)
        {
            if (assetPlayer == null) throw new ArgumentNullException(nameof(assetPlayer));
            
            if (assetPlayer is TPlayer player)
            {
                SetupPlayer(player, ev);
            }
            else
            {
                throw new InvalidOperationException(
                    $"Wrong player is passed to player setup. Player type: {assetPlayer.GetType().Name}. Supposed type: {typeof(TPlayer).Name}");
            }
        }

        protected abstract void SetupPlayer(TPlayer player, Event ev);

        protected AudioSource GetAudioSource(Event ev, AudioSourceManager sourceManager)
        {
            return ev.HasMusic() ? sourceManager.SongAudioSource : sourceManager.CharacterAudioSource;
        }
    }
}