using Modules.PhotoBooth.Profile;
using Modules.ProfilePhotoEditing;
using UIManaging.Pages.LevelEditor.Ui;
using UIManaging.Pages.ProfilePhotoEditor.Pages;
using UnityEngine;
using Zenject;

namespace Installers
{
    internal sealed class ProfilePhotoEditorSceneBindingsInstaller : MonoInstaller
    {
        [SerializeField] private VirtualCameraBasedController _cameraController;
        [SerializeField] private ProfilePhotoBoothPresetProvider _presetProvider;

        public override void InstallBindings()
        {
            Container.BindInterfacesAndSelfTo<ProfilePhotoEditor>().AsSingle();
            Container.BindInterfacesAndSelfTo<ProfilePhotoEditorDefaults>().AsSingle();
            Container.Bind<ProfilePhotoBooth>().AsSingle();
            Container.Bind<ProfilePhotoBoothPresetProvider>().FromInstance(_presetProvider).AsSingle();
            Container.Bind<VirtualCameraBasedController>().FromComponentInNewPrefab(_cameraController.gameObject).AsSingle();
            Container.Bind<BaseEditorPageModel>().To<ProfilePhotoEditorPageModel>().AsSingle();
        }
    }
}