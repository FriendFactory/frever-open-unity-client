namespace UIManaging.Pages.Common.SongOption.Buttons
{
    internal sealed class OpenUploadsCategoryButton: OpenMusicCategoryButtonBase 
    {
        protected override MusicNavigationCommand Command => MusicNavigationCommand.OpenUploads;
    }
}