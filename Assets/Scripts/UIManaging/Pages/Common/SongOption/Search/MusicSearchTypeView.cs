using Common.Abstract;

namespace UIManaging.Pages.Common.SongOption.Search
{
    internal abstract class MusicSearchTypeView : BaseContextView<MusicSearchListModelsProvider>
    {
       public abstract MusicSearchType MusicSearchType { get; }
    }
}