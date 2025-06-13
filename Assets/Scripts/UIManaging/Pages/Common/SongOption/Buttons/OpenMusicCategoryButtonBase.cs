using UIManaging.Core;
using Zenject;

namespace UIManaging.Pages.Common.SongOption.Buttons
{
    internal abstract class OpenMusicCategoryButtonBase: ButtonBase
    {
        [Inject] private MusicSelectionStateController _musicSelectionStateController;
        
        protected abstract MusicNavigationCommand Command { get; }
        
        protected override void OnClick()
        {
            _musicSelectionStateController.FireAsync(Command);
        }
    }
}