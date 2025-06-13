using Bridge.Models.Common;

namespace UIManaging.Pages.Common.SongOption
{
    public class FullPlaylistListModel: MusicViewModel
    {
        public string Name;
        public IPlayableMusic[] Playables;
    }
}