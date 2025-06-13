using System;
using System.Collections.Generic;
using Bridge.Models.ClientServer.Level.Full;
using Extensions;
using JetBrains.Annotations;
using Modules.AssetsManaging;
using Modules.AssetsManaging.LoadArgs;
using Event = Models.Event;

namespace Modules.LevelManaging.Editing.LevelPreview
{
    [UsedImplicitly]
    internal sealed class EventCaptionLoader : DefaultLoader<CaptionFullInfo, CaptionLoadArgs>
    {
        public EventCaptionLoader(IAssetManager assetManager) : base(assetManager)
        {
        }

        public override DbModelType Type => DbModelType.Caption;
        
        protected override CaptionLoadArgs[] Args => new[] {new CaptionLoadArgs {DeactivateOnLoad = true}};

        protected override ICollection<CaptionFullInfo> ExtractAssetModels(Event @event)
        {
            return @event.Caption ?? Array.Empty<CaptionFullInfo>();
        }
    }
}