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
    internal sealed class EventVfxLoader : DefaultLoader<VfxInfo, VfxLoadArgs>
    {
        public EventVfxLoader(IAssetManager assetManager) : base(assetManager)
        {
        }

        public override DbModelType Type => DbModelType.Vfx;
        
        protected override VfxLoadArgs[] Args => new[] {new VfxLoadArgs {DeactivateOnLoad = true}};

        protected override ICollection<VfxInfo> ExtractAssetModels(Event @event)
        {
            var vfx = @event.GetVfx();
            return vfx == null ? Array.Empty<VfxInfo>() : new[] {vfx};
        }
    }
}
