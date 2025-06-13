using UIManaging.Pages.AvatarPreview.Ui;
using Zenject;

namespace Installers
{
    internal static class AvatarDisplayBinder
    {
        public static void BindAvatarDisplay(this DiContainer container)
        {
            container.BindFactory<AvatarDisplaySelfieModel.Args, AvatarDisplaySelfieModel, AvatarDisplaySelfieModel.Factory>().AsSingle();
            container.BindFactory<AvatarDisplayCharacterModel.Args, AvatarDisplayCharacterModel, AvatarDisplayCharacterModel.Factory>().AsSingle();
        }
    }
}