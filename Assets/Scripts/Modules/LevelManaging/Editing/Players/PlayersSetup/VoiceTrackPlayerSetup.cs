using System.Linq;
using Models;
using Modules.LevelManaging.Assets;
using Modules.LevelManaging.Assets.AssetDependencies;
using Modules.LevelManaging.Editing.Players.AssetPlayers;

namespace Modules.LevelManaging.Editing.Players.PlayersSetup
{
    internal sealed class VoiceTrackPlayerSetup: GenericSetup<IVoiceTrackAsset, VoiceTrackAssetPlayer>
    {
        private readonly AudioSourceManager _audioSourceManager;

        public VoiceTrackPlayerSetup(AudioSourceManager audioSourceManager)
        {
            _audioSourceManager = audioSourceManager;
        }

        protected override void SetupPlayer(VoiceTrackAssetPlayer player, Event ev)
        {
            var faceAndVoiceController = ev.CharacterController
                .First(x => x.CharacterControllerFaceVoice.First().VoiceTrackId.HasValue)
                .CharacterControllerFaceVoice.First();
            var voiceSoundVolume = faceAndVoiceController.VoiceSoundVolume/100f;

            var audioSource = _audioSourceManager.CharacterAudioSource;
            
            player.SetVoiceSoundVolume(voiceSoundVolume);
            player.SetAudioSource(audioSource);
        }
    }
}