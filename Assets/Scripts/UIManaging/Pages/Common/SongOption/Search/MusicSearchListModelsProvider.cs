using System;
using Bridge;
using UIManaging.Pages.Common.SongOption.SongList;

namespace UIManaging.Pages.Common.SongOption.Search
{
    internal class MusicSearchListModelsProvider: IDisposable
    {
        private readonly TrackSearchListModel _trackSearchListModel;
        private readonly SongSearchListModel _songSearchListModel;
        private readonly TrendingUserSoundsSearchListModel _userSoundSearchListModel;
        private readonly CommercialSongSearchListModel _commercialSongSearchListModel;
        
        public MusicSearchListModelsProvider(IMusicBridge bridge)
        {
            _trackSearchListModel = new TrackSearchListModel(bridge);
            _songSearchListModel = new SongSearchListModel(bridge);
            _userSoundSearchListModel = new TrendingUserSoundsSearchListModel(bridge);
            _commercialSongSearchListModel = new CommercialSongSearchListModel(bridge);
        }

        // providing MusicSearchType formally is redundant, but w/o explicit type casting fails
        public TModel GetListModel<TModel>(MusicSearchType musicSearchType) where TModel: class, ISearchListModel<PlayableItemModel>
        {
            switch (musicSearchType)
            {
                case MusicSearchType.Moods:
                    return _songSearchListModel as TModel;
                case MusicSearchType.Music:
                    return _trackSearchListModel as TModel;
                case MusicSearchType.TrendingUserSounds:
                    return _userSoundSearchListModel as TModel;
                case MusicSearchType.CommercialSongs:
                    return _commercialSongSearchListModel as TModel;
                default:
                    throw new ArgumentOutOfRangeException(nameof(musicSearchType), musicSearchType, null);
            }
        }

        public void Dispose() { }
    }
}