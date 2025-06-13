using System.Linq;
using Extensions;
using Models;
using Modules.AssetsManaging;
using Modules.LevelManaging.Assets;

namespace Modules.LevelManaging.Editing.Players.EventAssetsProviders
{
    internal sealed class VoiceTrackAssetsProvider: EventAssetsProviderBase
    {
        public override DbModelType TargetType => DbModelType.VoiceTrack;
        
        public VoiceTrackAssetsProvider(IAssetManager assetManager) : base(assetManager)
        {
        }
        
        public override IAsset[] GetLoadedAssets(Event ev)
        {
            if (ev.CharacterController == null || ev.CharacterController.Count == 0)
                return Empty;

            var voiceControllers = ev.GetCharacterFaceVoiceControllers();

            if (!voiceControllers.Any())
                return Empty;

            var voiceIds = voiceControllers.Where(x=>x.VoiceTrack != null).Select(x => x.VoiceTrack.Id);
            
            var loadedVoiceTracks = AssetManager.GetAllLoadedAssets(DbModelType.VoiceTrack);
            return loadedVoiceTracks.Where(x => voiceIds.Contains(x.Id)).ToArray();
        }
    }
}