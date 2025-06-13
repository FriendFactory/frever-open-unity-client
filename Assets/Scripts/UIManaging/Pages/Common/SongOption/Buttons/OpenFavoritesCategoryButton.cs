namespace UIManaging.Pages.Common.SongOption.Buttons
{
    internal sealed class OpenFavoritesCategoryButton: OpenMusicCategoryButtonBase
    {
        protected override MusicNavigationCommand Command => MusicNavigationCommand.OpenFavorites;
    }
}