using UIManaging.Pages.Common.VideoManagement;

namespace UIManaging.Pages.Common.TabsManager
{
    public sealed class FeedTabModel : TabModel
    {
        public VideoListType Type { get;}
        
        public FeedTabModel(int index, string name) : base(index, name){}

        public FeedTabModel(int index, string name, bool showUpdateMarker, bool containsNew) : base(index, name, showUpdateMarker, containsNew){}
        
        public FeedTabModel(int index, string name, VideoListType type, bool showUpdateMarker, bool containsNew) : base(index, name, showUpdateMarker, containsNew)
        {
            Type = type;
        }

        public FeedTabModel(int index, string name, VideoListType type) : base(index, name)
        {
            Type = type;
        }
    }
}