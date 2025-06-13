using Configs;
using Modules.CharacterManagement;
using Modules.FreverUMA.ViewManagement;
using Modules.LevelManaging.Assets;
using Modules.UserScenarios.Implementation.LevelCreation.Scenarios;
using UIManaging.Common.Args.Views.Profile;
using Zenject;

namespace Installers
{
    internal static class CharacterServicesBinder
    {
        public static void BindCharacterServices(this DiContainer container, CharacterManagerConfig characterManagerConfig, BoneColliderSettings[] boneColliderSettings)
        {
            container.Bind<CharacterManager>().AsSingle();
            container.Bind<CharacterViewContainer>().AsSingle();
            container.Bind<CharacterManagerConfig>().FromInstance(characterManagerConfig);
            container.Bind<BoneColliderSettings[]>().FromInstance(boneColliderSettings);
            container.Bind<CharacterThumbnailProvider>().AsSingle();
            container.Bind<CharacterThumbnailCacheAutoClearProcess>().AsSingle();
            container.BindInterfacesAndSelfTo<UndressingCharacterService>().AsSingle();
        }
    }
}
