using Bridge.Models.Common;
using StansAssets.Foundation.Patterns;
using UIManaging.Pages.Common.SongOption.Common;

namespace UIManaging.Pages.Common.SongOption.SongList
{
    public abstract class PlayableItemModel
    {
        //---------------------------------------------------------------------
        // Properties
        //---------------------------------------------------------------------

        public abstract long Id { get; }
        public abstract string Title { get; }
        public abstract string ArtistName { get;  }
        public int Duration => Music.Duration;
        public abstract IPlayableMusic Music { get; }
        
        //---------------------------------------------------------------------
        // Public
        //---------------------------------------------------------------------
        
        public void TogglePlay()
        {
            StaticBus<SongSelectedEvent>.Post(new SongSelectedEvent(Music));
        }
    }
}