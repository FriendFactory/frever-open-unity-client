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
    internal sealed class EventUserPhotoLoader : DefaultLoader<PhotoFullInfo, PhotoLoadArgs>
    {
        public EventUserPhotoLoader(IAssetManager assetManager) : base(assetManager)
        {
        }

        public override DbModelType Type => DbModelType.UserPhoto;
        
        protected override PhotoLoadArgs[] Args => new[] {new PhotoLoadArgs {DeactivateOnLoad = true}}; 

        protected override ICollection<PhotoFullInfo> ExtractAssetModels(Event @event)
        {
            var photo = @event.GetPhoto();
            return photo == null ? Array.Empty<PhotoFullInfo>() : new[] {photo};
        }
    }
}
