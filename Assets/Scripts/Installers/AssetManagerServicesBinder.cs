using Common;
using Modules.AssetsManaging;
using Modules.AssetsManaging.Loaders;
using Modules.AssetsManaging.LoadingProfiles;
using Modules.AssetsManaging.UncompressedBundles;
using Modules.LevelManaging.Assets;
using Zenject;

namespace Installers
{
    internal static class AssetManagerServicesBinder
    {
        public static void BindAssetManager(this DiContainer container)
        {
            container.BindInterfacesAndSelfTo<AssetManager>().AsSingle();
            container.BindInterfacesAndSelfTo<AssetServicesProvider>().AsSingle();
            container.Bind<CaptionLoadProfile>().AsSingle();
            container.BindFactory<CaptionLoader, CaptionLoader.Factory>().AsSingle();
            container.Bind<UncompressedBundlesManager>().AsSingle();
            container.BindInterfacesAndSelfTo<SceneObjectHelper>().AsSingle().WithArguments(Constants.PERSISTENT_SCENE_INDEX);
            container.BindInterfacesAndSelfTo<VfxCategoryRuntimeAdjuster>().AsSingle();
        }
    }
}