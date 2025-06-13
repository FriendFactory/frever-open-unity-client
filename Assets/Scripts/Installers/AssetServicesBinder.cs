using AssetBundleLoaders;
using Modules.CharacterManagement;
using Modules.FaceAndVoice.Voice.Recording.Core;
using Modules.LevelManaging.Assets;
using Modules.LevelManaging.Assets.AssetDependencies;
using Modules.LevelManaging.Assets.AssetHelpers;
using Modules.WardrobeManaging;
using UIManaging.Pages.Common.Helpers;
using UIManaging.Pages.Common.SongOption.Common;
using UnityEngine;
using Zenject;

namespace Installers
{
    internal static class AssetServicesBinder
    {
        public static void BindAssetServices(this DiContainer container, AudioSourceManager audioSourceManager)
        {
            container.Bind<AudioSourceManager>().FromInstance(audioSourceManager).AsSingle();
            container.Bind<IAssetBundleLoader>().To<AssetBundleLoader>().FromInstance(GetAssetBundleLoaderInstance()).AsSingle();
            container.Bind<IVoiceRecorder>().To<VoiceRecorder>().AsSingle();
            container.Bind<OutfitsManager>().AsSingle();
            container.Bind<ClothesCabinet>().AsSingle();
            container.BindInterfacesAndSelfTo<VoiceFilterController>().AsSingle();
            container.Bind<VfxBinder>().AsSingle();
            container.Bind<SongSelectionController>().AsSingle();
            container.Bind<MusicDownloadHelper>().AsSingle();
            container.Bind<SongPlayer>().AsSingle();
            container.BindInterfacesAndSelfTo<WardrobeStore>().AsSingle();
        }
        
        private static AssetBundleLoader GetAssetBundleLoaderInstance()
        {
            return new GameObject("AssetBundleLoader").AddComponent<AssetBundleLoader>();
        }
    }
}
