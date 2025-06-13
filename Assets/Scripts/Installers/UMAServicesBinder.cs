using Configs;
using Modules.FreverUMA;
using UIManaging.Pages.UmaEditorPage.Ui;
using UnityEngine;
using Zenject;

namespace Installers
{
    internal static class UMAServicesBinder
    {
        public static void BindUMAServices(this DiContainer container, DefaultSubCategoryColors defaultSubCategoryColors, 
                                            DNASlidersGroupingSettings _dnaSlidersGroupingSettings, ColorPalletHidingConfiguration _colorPalletHidingConfiguration)
        {
            var umaCharacterSource = Object.FindObjectOfType<UMACharacterSource>();
            var umaEditorRoom = Object.FindObjectOfType<UMACharacterEditorRoom>();
            
            container.Bind<DefaultSubCategoryColors>().FromInstance(defaultSubCategoryColors);
            container.Bind<UMACharacterSource>().FromInstance(umaCharacterSource);
            container.Bind<UMACharacterEditorRoom>().FromInstance(umaEditorRoom).AsSingle();
            container.Bind<ICharacterEditor>().To<UMACharacterEditor>().AsSingle();
            container.Bind<UmaBundleHelper>().AsSingle();
            container.Bind<AvatarHelper>().AsSingle();
            container.Bind<DNASlidersGroupingSettings>().FromInstance(_dnaSlidersGroupingSettings);
            container.Bind<ColorPalletHidingConfiguration>().FromInstance(_colorPalletHidingConfiguration);
        }
    }
}
