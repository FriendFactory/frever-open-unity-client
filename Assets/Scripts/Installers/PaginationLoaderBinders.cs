using System;
using UIManaging.Pages.LevelEditor.Ui.SelectionViews.AssetSelection;
using UIManaging.Pages.LevelEditor.Ui.SelectionViews.AssetSelection.AssetSelectors;
using UIManaging.Pages.LevelEditor.Ui.SelectionViews.AssetSelection.PaginationLoaders;
using Zenject;

namespace Installers
{
    internal static class PaginationLoaderBinder
    {
        public static void BindPaginationLoaders(this DiContainer container)
        {
            container.BindFactory<PaginationLoaderType, Func<AssetSelectorModel>, long, NullableLong, string, NullableLong, 
                SetLocationPaginationLoader, SetLocationPaginationLoader.Factory>();
            container.BindFactory<PaginationLoaderType, Func<AssetSelectorModel>, long, NullableLong, string, NullableLong,
                BodyAnimationPaginationLoader, BodyAnimationPaginationLoader.Factory>();
            container.BindFactory<PaginationLoaderType, Func<AssetSelectorModel>, long, NullableLong, string, NullableLong, 
                CameraFilterPaginationLoader, CameraFilterPaginationLoader.Factory>();
            container.BindFactory<PaginationLoaderType, Func<AssetSelectorModel>, long, NullableLong, string, NullableLong, 
                VfxPaginationLoader, VfxPaginationLoader.Factory>();
            container.BindFactory<PaginationLoaderType, Func<AssetSelectorModel>, long, NullableLong, string, NullableLong, 
                CharactersPaginationLoader, CharactersPaginationLoader.Factory>();
        }
    }
}