using JetBrains.Annotations;
using UIManaging.Common.Buttons;
using Zenject;

namespace UIManaging.Pages.Common.SongOption
{
    internal sealed class SkipMusicSelectionButton : BaseButton
    {
        private MusicSelectionPageModel _musicSelectionPageModel;
        
        [Inject, UsedImplicitly]
        private void Construct(MusicSelectionPageModel musicSelectionPageModel)
        {
            _musicSelectionPageModel = musicSelectionPageModel;
        }

        protected override void OnClickHandler() => _musicSelectionPageModel.OnSkipRequested();
    }
}
