using System.Linq;

namespace UIManaging.Pages.SeasonPage.Likes
{
    internal sealed class SeasonLikesListModel
    {
        public SeasonLikesItemModel[] Items { get; }
        public long? StartQuestId { get; set; }

        public int StartIndex
        {
            get
            {
                for (var i = 0; i < Items.Length; i++)
                {
                    if (Items[i] is SeasonLikesQuestModel questModel && questModel.Id == StartQuestId)
                    {
                        return i;
                    }
                }

                return -1;
            }
        }

        //---------------------------------------------------------------------
        // Public
        //---------------------------------------------------------------------

        public SeasonLikesListModel(SeasonLikesItemModel[] items, long? startQuestId = null)
        {
            Items = items;
            StartQuestId = startQuestId ?? QueryStartQuestId();
        }

        //---------------------------------------------------------------------
        // Helpers
        //---------------------------------------------------------------------

        private long? QueryStartQuestId()
        {
            var quests = Items
                        .Where(i => i is SeasonLikesQuestModel)
                        .Cast<SeasonLikesQuestModel>()
                        .OrderBy(q => q.CurrentQuestLikes)
                        .ToArray();

            if (!quests.Any()) return null;

            var userLikes = quests.First().CurrentUserLikes;
            return quests.FirstOrDefault(q => q.CurrentQuestLikes >= userLikes)?.Id;
        }
    }
}