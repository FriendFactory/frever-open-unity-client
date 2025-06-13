using System.Linq;
using Common.TimeManaging;
using Extensions;
using Modules.AssetsManaging;
using Modules.FaceAndVoice.Face.Playing.Core;
using Modules.LevelManaging.Assets;
using Modules.LevelManaging.Assets.AssetDependencies;
using Modules.LevelManaging.Editing.Players.AssetPlayers;
using UnityEngine;
using Event = Models.Event;

namespace Modules.LevelManaging.Editing.Players.PlayersSetup
{
    internal sealed class FaceAnimationPlayerSetup : GenericSetup<IFaceAnimationAsset, FaceAnimationAssetPlayer>
    {
        private readonly IAssetManager _assetManager;
        private readonly AudioSourceManager _audioSourceManager;
        private readonly IUnityTimeBasedTimeSource _unityTimeBasedTimeSource;
        private readonly IAudioBasedTimeSource _audioBasedTimeSource;

        public FaceAnimationPlayerSetup(IAssetManager assetManager, AudioSourceManager audioSourceManager, IUnityTimeBasedTimeSource unityTimeBasedTimeSource, IAudioBasedTimeSource audioBasedTimeSource)
        {
            _assetManager = assetManager;
            _audioSourceManager = audioSourceManager;
            _unityTimeBasedTimeSource = unityTimeBasedTimeSource;
            _audioBasedTimeSource = audioBasedTimeSource;
        }

        protected override void SetupPlayer(FaceAnimationAssetPlayer player, Event ev)
        {
            var characterAssets = _assetManager.GetAllLoadedAssets<ICharacterAsset>();

            var timeSource = PrepareTimeSource(player, ev);
            player.SetTimeSource(timeSource);
            var faces = GetFaces(player.AssetId, ev, characterAssets);
            player.SetTargetFaces(faces);
        }

        private ITimeSource PrepareTimeSource(FaceAnimationAssetPlayer player, Event ev)
        {
            if (!ev.HasAnyAudio())
            {
                return PrepareRealTimeBasedTimeSource(player);
            }

            return PrepareAudioBasedTimeSource(player, ev);
        }

        private IUnityTimeBasedTimeSource PrepareRealTimeBasedTimeSource(FaceAnimationAssetPlayer player)
        {
            var faceAnimAsset = player.TargetAsset as IFaceAnimationAsset;
            var initialTime = faceAnimAsset.FirstFrameCue.ToSeconds();
            _unityTimeBasedTimeSource.Start(initialTime);
            return _unityTimeBasedTimeSource;
        }

        private IAudioBasedTimeSource PrepareAudioBasedTimeSource(FaceAnimationAssetPlayer player, Event ev)
        {
            var audioSource = GetAudioSource(ev);
            var timeShifting = CalculateNecessaryAnimationTimeCurveShiftingToSyncWithAudio(player, ev);
            _audioBasedTimeSource.SetupAudioSource(audioSource);
            _audioBasedTimeSource.SetAudioTimeLineShift(timeShifting);
            return _audioBasedTimeSource;
        }

        private AudioSource GetAudioSource(Event ev)
        {
            return ev.HasMusic()
                ? _audioSourceManager.SongAudioSource
                : _audioSourceManager.CharacterAudioSource;
        }

        private static float CalculateNecessaryAnimationTimeCurveShiftingToSyncWithAudio(FaceAnimationAssetPlayer player, Event ev)
        {
            var faceAnimationAsset = player.TargetAsset as IFaceAnimationAsset;
            var faceAnimStartCue = faceAnimationAsset.FirstFrameCue;
            var audioStartCue = GetAudioActivationCue(ev);
            var shiftingMs = audioStartCue - faceAnimStartCue;
            return shiftingMs.ToSeconds();
        }
        
        private static int GetAudioActivationCue(Event ev)
        {
            return ev.HasMusic() ? ev.GetMusicController().ActivationCue : 0;
        }
        
        private static FaceAnimPlayer[] GetFaces(long faceAnimId, Event ev, ICharacterAsset[] characterAssets)
        {
            var charactersWithFaceAnimIds = ev.GetCharacterIdsWithFaceAnimationId(faceAnimId);
            var targetCharacters = characterAssets.Where(x => charactersWithFaceAnimIds.Contains(x.Id)).ToArray();
            var faces = targetCharacters.Select(x => x.FaceAnimPlayer).ToArray();
            return faces;
        }
    }
}