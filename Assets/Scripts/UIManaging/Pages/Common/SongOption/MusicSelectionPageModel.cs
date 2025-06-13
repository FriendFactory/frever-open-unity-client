using System;
using Bridge.Models.Common;
using JetBrains.Annotations;

namespace UIManaging.Pages.Common.SongOption
{
    [UsedImplicitly]
    public class MusicSelectionPageModel
    {
        public bool SkipAllowed { get; set; }
        public bool IsOpened { get; private set; }
        
        public event Action PageOpened;
        public event Action PageCloseRequested;
        public event Action PageClosed;
        public event Action<IPlayableMusic, int> SongApplied;
        public event Action SkipRequested;

        public void OnPageOpened()
        {
            IsOpened = true;
            
            PageOpened?.Invoke();
        }

        public void OnPageCloseRequested()
        {
            PageCloseRequested?.Invoke();
        }
        
        public void OnPageClosed()
        {
            IsOpened = false;
            
            PageClosed?.Invoke();
        }

        public void OnSongApplied(IPlayableMusic song, int activationCue)
        {
            SongApplied?.Invoke(song, activationCue);
        }
        
        public void OnSkipRequested()
        {
            SkipRequested?.Invoke();
        }
    }
}