
namespace UIManaging.PopupSystem.Popups.TaggedUsers
{
    internal sealed class FeedTaggedUserItemModel
    {
        public long GroupId { get; }
        public string NickName { get; }

        public FeedTaggedUserItemModel(long groupId, string nickName)
        {
            GroupId = groupId;
            NickName = nickName;
        }
    }
}