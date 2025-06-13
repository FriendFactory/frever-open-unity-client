using System;
using System.Collections.Generic;
using Bridge.Models.ClientServer.Assets;
using Extensions;
using JetBrains.Annotations;
using Models;
using Modules.AssetsManaging;
using Modules.AssetsManaging.LoadArgs;

namespace Modules.LevelManaging.Editing.LevelPreview
{
    [UsedImplicitly]
    internal sealed class EventSetLocationBackgroundLoader : DefaultLoader<SetLocationBackground, SetLocationBackgroundLoadArgs>
    {
        public EventSetLocationBackgroundLoader(IAssetManager assetManager) : base(assetManager)
        {
        }

        public override DbModelType Type => DbModelType.UserPhoto;
        
        protected override SetLocationBackgroundLoadArgs[] Args => new[] {new SetLocationBackgroundLoadArgs {DeactivateOnLoad = true}}; 

        protected override ICollection<SetLocationBackground> ExtractAssetModels(Event @event)
        {
            var background = @event.GetSetLocationBackground();
            return background == null ? Array.Empty<SetLocationBackground>() : new[] {background};
        }
    }
}