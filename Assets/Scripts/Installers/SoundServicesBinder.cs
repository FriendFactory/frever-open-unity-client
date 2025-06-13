using Common;
using Modules.MusicCacheManaging;
using Modules.MusicLicenseManaging;
using Modules.MusicServicesInitialization;
using Modules.Sound;
using UnityEngine;
using Zenject;

namespace Installers
{
    public static class SoundServicesBinder
    {
        public static void BindSoundServices(this DiContainer container, SoundBank soundBank)
        {
            container.Bind<SoundBank>().FromInstance(soundBank);
            var soundManager = Object.FindObjectOfType<SoundManager>();
            container.BindInterfacesAndSelfTo<SoundManager>().FromInstance(soundManager);
            container.BindInterfacesAndSelfTo<LicensedMusicProvider>().AsSingle();
            container.BindInterfacesAndSelfTo<MusicLicenseValidator>().AsSingle();
            container.BindInterfacesAndSelfTo<LicensedMusicUsageManager>().AsSingle().WithArguments(
                Constants.LicensedMusic.Constraints.LICENSED_SONGS_IN_LEVEL_COUNT_MAX,
                Constants.LicensedMusic.Constraints.LICENSED_SONG_DURATION_USAGE_MAX_SEC,
                Constants.LicensedMusic.Constraints.LICENSED_SONGS_CLIP_LENGTH_SEC,
                Constants.LevelDefaults.MIN_EVENT_DURATION_MS);
            container.BindInterfacesAndSelfTo<MusicServicesInitializationManager>().AsSingle();
        }
    }
}