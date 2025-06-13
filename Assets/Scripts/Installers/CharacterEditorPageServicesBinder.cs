using UIManaging.Pages.UmaEditorPage.Ui.WardrobePanel;
using Zenject;

namespace Installers
{
    internal static class CharacterEditorPageServicesBinder
    {
        public static void BindCharacterEditorServices(this DiContainer container)
        {
            container.BindInterfacesAndSelfTo<WardrobesResponsesCache>().AsSingle();
        }
    }
}