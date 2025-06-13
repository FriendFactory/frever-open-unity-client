namespace UIManaging.Pages.Common.SongOption.Buttons
{
    internal sealed class OpenMoodsCategoryButton : OpenMusicCategoryButtonBase
    {
        protected override MusicNavigationCommand Command => MusicNavigationCommand.OpenMoods;
    }
}