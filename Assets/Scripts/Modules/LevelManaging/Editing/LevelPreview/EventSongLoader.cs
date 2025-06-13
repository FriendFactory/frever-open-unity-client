using System;
using System.Collections.Generic;
using Bridge.Models.ClientServer.Assets;
using Extensions;
using JetBrains.Annotations;
using Modules.AssetsManaging;
using Modules.AssetsManaging.LoadArgs;
using Event = Models.Event;

namespace Modules.LevelManaging.Editing.LevelPreview
{
    [UsedImplicitly]
    internal sealed class EventSongLoader : DefaultLoader<SongInfo, SongLoadArgs>
    {
        public EventSongLoader(IAssetManager assetManager) : base(assetManager)
        {
        }

        public override DbModelType Type => DbModelType.Song;
        
        protected override SongLoadArgs[] Args => new[] {new SongLoadArgs {DeactivateOnLoad = true}};

        protected override ICollection<SongInfo> ExtractAssetModels(Event @event)
        {
            var song = @event.GetSong();
            return song == null ? Array.Empty<SongInfo>() : new[] {song};
        }
    }
}
