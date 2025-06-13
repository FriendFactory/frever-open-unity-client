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
    internal sealed class EventVideoClipLoader : DefaultLoader<VideoClipFullInfo, VideoClipLoadArgs>
    {
        public EventVideoClipLoader(IAssetManager assetManager) : base(assetManager)
        {
        }

        public override DbModelType Type => DbModelType.VideoClip;
        
        protected override VideoClipLoadArgs[] Args => new[] {new VideoClipLoadArgs {DeactivateOnLoad = true}};

        protected override ICollection<VideoClipFullInfo> ExtractAssetModels(Event @event)
        {
            var videoClip = @event.GetVideo();
            return videoClip == null ? Array.Empty<VideoClipFullInfo>() : new[] {videoClip};
        }
    }
}
