using System;
using System.Collections.Generic;
using Bridge.Services._7Digital.Models.TrackModels;
using Extensions;
using JetBrains.Annotations;
using Models;
using Modules.AssetsManaging;
using Modules.AssetsManaging.LoadArgs;

namespace Modules.LevelManaging.Editing.LevelPreview
{
    [UsedImplicitly]
    internal sealed class EventExternalTrackLoader : DefaultLoader<ExternalTrackInfo, ExternalTrackLoadArgs>
    {
        public EventExternalTrackLoader(IAssetManager assetManager) : base(assetManager)
        {
        }

        public override DbModelType Type => DbModelType.ExternalTrack;
        protected override ExternalTrackLoadArgs[] Args => new[] {new ExternalTrackLoadArgs {DeactivateOnLoad = true}};
        protected override ICollection<ExternalTrackInfo> ExtractAssetModels(Event @event)
        {
            var externalTrack = @event.GetExternalTrack();
            return externalTrack == null ? Array.Empty<ExternalTrackInfo>() : new []{externalTrack}; 
        }
    }
}