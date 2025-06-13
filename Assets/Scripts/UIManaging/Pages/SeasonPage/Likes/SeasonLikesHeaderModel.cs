using UnityEngine.Events;

namespace UIManaging.Pages.SeasonPage.Likes
{
    internal sealed class SeasonLikesHeaderModel : SeasonLikesItemModel
    {
        public int LikesAmount { get; set; }

        public SeasonLikesHeaderModel(int likesAmount)
        {
            LikesAmount = likesAmount;
        }
    }
}