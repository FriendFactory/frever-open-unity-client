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
    internal sealed class EventCameraFilterVariantLoader : DefaultLoader<CameraFilterVariantInfo, CameraFilterVariantLoadArgs>
    {
        public EventCameraFilterVariantLoader(IAssetManager assetManager) : base(assetManager)
        {
        }

        public override DbModelType Type => DbModelType.CameraFilterVariant;

        protected override CameraFilterVariantLoadArgs[] Args => new[]{new CameraFilterVariantLoadArgs {DeactivateOnLoad = true}};
        
        protected override ICollection<CameraFilterVariantInfo> ExtractAssetModels(Event @event)
        {
            var cameraFilter = @event.GetCameraFilterVariant();
            return cameraFilter == null ? Array.Empty<CameraFilterVariantInfo>() : new []{cameraFilter};
        }
    }
}
