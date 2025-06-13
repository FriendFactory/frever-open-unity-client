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
    internal sealed class EventVoiceTrackLoader : DefaultLoader<VoiceTrackFullInfo, VoiceTrackLoadArgs>
    {
        public EventVoiceTrackLoader(IAssetManager assetManager) : base(assetManager)
        {
        }

        public override DbModelType Type => DbModelType.VoiceTrack;
        
        protected override VoiceTrackLoadArgs[] Args => new[] {new VoiceTrackLoadArgs {DeactivateOnLoad = true}};
        
        protected override ICollection<VoiceTrackFullInfo> ExtractAssetModels(Event @event)
        {
            return @event.CharacterController.Select(controller => controller.GetVoiceTrack()).Where(x=>x != null).ToArray();
        }
    }
}
