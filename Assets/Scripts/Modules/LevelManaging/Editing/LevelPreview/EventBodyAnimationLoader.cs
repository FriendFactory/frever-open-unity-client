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
    internal sealed class EventBodyAnimationLoader : DefaultLoader<BodyAnimationInfo, BodyAnimationLoadArgs>
    {
        public EventBodyAnimationLoader(IAssetManager assetManager) : base(assetManager)
        {
        }

        public override DbModelType Type => DbModelType.BodyAnimation;
        
        protected override BodyAnimationLoadArgs[] Args => new[]{new BodyAnimationLoadArgs {DeactivateOnLoad = true}};

        protected override ICollection<BodyAnimationInfo> ExtractAssetModels(Event @event)
        {
            return @event.CharacterController.Select(controller => controller.GetBodyAnimation()).Where(x=>x!=null).ToArray();
        }
    }
}
