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
    internal sealed class EventCameraAnimationLoader : DefaultLoader<CameraAnimationFullInfo, CameraAnimLoadArgs>
    {
        public EventCameraAnimationLoader(IAssetManager assetManager) : base(assetManager)
        {
        }

        public override DbModelType Type => DbModelType.CameraAnimation;
        
        protected override CameraAnimLoadArgs[] Args => new[]{new CameraAnimLoadArgs {DeactivateOnLoad = true}};

        protected override ICollection<CameraAnimationFullInfo> ExtractAssetModels(Event @event)
        {
            var cameraAnim = @event.GetCameraAnimation();  //DWC
            return cameraAnim == null ? Array.Empty<CameraAnimationFullInfo>() : new[] {cameraAnim};
        }
    }
}
