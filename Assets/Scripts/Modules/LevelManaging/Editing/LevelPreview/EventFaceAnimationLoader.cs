using System.Collections.Generic;
using System.Linq;
using Bridge.Models.ClientServer.Assets;
using Extensions;
using JetBrains.Annotations;
using Modules.AssetsManaging;
using Modules.AssetsManaging.LoadArgs;
using Event = Models.Event;

namespace Modules.LevelManaging.Editing.LevelPreview
{
    [UsedImplicitly]
    internal sealed class EventFaceAnimationLoader : DefaultLoader<FaceAnimationFullInfo, FaceAnimationLoadArgs>
    {
        public EventFaceAnimationLoader(IAssetManager assetManager) : base(assetManager)
        {
        }

        public override DbModelType Type => DbModelType.FaceAnimation;
        
        protected override FaceAnimationLoadArgs[] Args => new[] {new FaceAnimationLoadArgs {DeactivateOnLoad = true}};
        
        protected override ICollection<FaceAnimationFullInfo> ExtractAssetModels(Event @event)
        {
            return @event.CharacterController.Select(controller => controller.GetFaceAnimation()).Where(x=>x != null).ToArray();
        }
    }
}
