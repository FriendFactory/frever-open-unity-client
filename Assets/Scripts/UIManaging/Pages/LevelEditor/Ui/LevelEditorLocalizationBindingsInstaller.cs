using UIManaging.Localization;
using UnityEngine;
using Zenject;

namespace UIManaging.Pages.LevelEditor.Ui
{
    public class LevelEditorLocalizationBindingsInstaller : MonoInstaller
    {
        [SerializeField] private MusicGalleryLocalization _musicGalleryLocalization;
        
        public override void InstallBindings()
        {
            Container.Bind<MusicGalleryLocalization>().FromInstance(_musicGalleryLocalization).AsCached();
        }
    }
}
